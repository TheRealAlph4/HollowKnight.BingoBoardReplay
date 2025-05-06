using BingoSync.Clients.EventInfoObjects;
using BingoSync.Sessions;
using BingoSync.Settings;
using Modding;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BingoBoardReplay
{
    public class GlobalSettings
    {
        public string SourceRoom = "-";
        public string SourcePassword = "-";
        public string DestinationRoom = "-";
        public string DestinationPassword = "-";
        public int ReplayDelaySeconds = 5;
    }

    public class BingoBoardReplay : Mod, IGlobalSettings<GlobalSettings>
    {
        new public string GetName() => "BingoBoardReplay";

        public static string version = "1.0.0.0";
        public override string GetVersion() => version;

        public override int LoadPriority() => 10;

        private GlobalSettings settings = new();
        private Session listener;
        private Session replayer;

        public override void Initialize()
        {
            Log("Initializing");
            ModHooks.FinishedLoadingModsHook += ConnectToRooms;
        }

        public void OnLoadGlobal(GlobalSettings s)
        {
            settings = s;
            settings.SourceRoom = settings.SourceRoom.Split('/').Last();
            settings.DestinationRoom = settings.DestinationRoom.Split('/').Last();
        }

        public GlobalSettings OnSaveGlobal()
        {
            return settings;
        }

        private void ConnectToRooms()
        {
            Task.Run(() =>
            {
                listener = BingoSync.Interfaces.SessionManager.CreateSession("SourceListener", BingoSync.Clients.Servers.BingoSync, false);
                listener.JoinRoom(settings.SourceRoom, "ReplayBot", settings.SourcePassword, (ex) => { });

                Thread.Sleep(2000);

                replayer = BingoSync.Interfaces.SessionManager.CreateSession("TargetReplayer", BingoSync.Clients.Servers.BingoSync, true);
                replayer.JoinRoom(settings.DestinationRoom, "ReplayBot", settings.DestinationPassword, (ex) => { });

                Thread.Sleep(5000);

                listener.RevealCard();
                replayer.RevealCard();

                replayer.SendChatMessage($"Replaying marks from room {settings.SourceRoom} with {settings.ReplayDelaySeconds} seconds delay.");

                listener.OnGoalUpdateReceived += MakeDelayedMarkReplayer(settings.ReplayDelaySeconds, replayer);

                listener.OnNewCardReceived += OnNewCardReceived;
                replayer.OnNewCardReceived += OnNewCardReceived;
            });
        }

        private void OnNewCardReceived(object sender, NewCardEventInfo e)
        {
            Session session = sender as Session;
            Task.Run(() =>
            {
                Thread.Sleep(2000);
                session.RevealCard();
            });
        }

        private EventHandler<GoalUpdateEventInfo> MakeDelayedMarkReplayer(int delaySeconds, Session replayer)
        {
            return (object sender, GoalUpdateEventInfo goalUpdate) =>
            {
                if (goalUpdate.Unmarking || !replayer.Board.GetSlot(goalUpdate.Slot).MarkedBy.Contains(BingoSync.Colors.Blank))
                {
                    return;
                }
                Task remark = new(() =>
                {
                    Thread.Sleep(delaySeconds * 1000);
                    replayer.SelectSquare(goalUpdate.Slot + 1, goalUpdate.Color, () => {
                        Log($"There was some error trying to mark slot {goalUpdate.Slot + 1}.");
                    });
                });
                remark.Start();
            };
        }
    }
}