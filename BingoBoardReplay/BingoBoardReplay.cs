using BingoSync.Clients.EventInfoObjects;
using BingoSync.Sessions;
using Modding;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BingoBoardReplay
{
    public class BingoBoardReplay : Mod, IGlobalSettings<GlobalSettings>
    {
        new public string GetName() => "BingoBoardReplay";

        public static string version = "1.1.0.0";
        public override string GetVersion() => version;

        public override int LoadPriority() => 10;

        public static BingoBoardReplay Instance;
        public GlobalSettings Settings = new();
        private Session listener;
        private Session replayer;

        private EventHandler<GoalUpdateEventInfo> currentReplayer;

        private int _goalsInProgress = 0;
        public int GoalsInProgress
        {
            get
            {
                return _goalsInProgress;
            }
            set
            {
                _goalsInProgress = value;
                ReplayUI.MarksInQueue = _goalsInProgress;
            }
        }

        private Guid CurrentReplayId { get; set; } = Guid.NewGuid();

        public override void Initialize()
        {
            Instance = this;
            Log("Initializing");
            ReplayUI.SetLog(Log);

            listener = BingoSync.Interfaces.SessionManager.CreateSession("SourceListener", BingoSync.Clients.Servers.BingoSync, false);
            replayer = BingoSync.Interfaces.SessionManager.CreateSession("TargetReplayer", BingoSync.Clients.Servers.BingoSync, true);

            listener.OnNewCardReceived += RevealNewCard;
            replayer.OnNewCardReceived += RevealNewCard;
            listener.OnRoomSettingsReceived += (sender, settings) => ReplayUI.SourceRoomTextSuffix = settings.IsLockout ? " (Lockout)" : " (Non-Lockout)";
            replayer.OnRoomSettingsReceived += (sender, settings) => ReplayUI.DestinationRoomTextSuffix = settings.IsLockout ? " (Lockout)" : " (Non-Lockout)";
        }

        public void OnLoadGlobal(GlobalSettings s)
        {
            Settings = s;
        }

        public GlobalSettings OnSaveGlobal()
        {
            return Settings;
        }

        public void StartReplay()
        {
            Task.Run(() =>
            {
                CurrentReplayId = Guid.NewGuid();

                listener.JoinRoom(ReplayUI.SourceRoomCode, "ReplayBot", ReplayUI.SourceRoomPassword, (ex) => { });
                while(listener.ClientIsConnecting())
                {
                    Thread.Sleep(1000);
                }

                replayer.JoinRoom(ReplayUI.DestinationRoomCode, "ReplayBot", ReplayUI.DestinationRoomPassword, (ex) => { });
                while(replayer.ClientIsConnecting())
                {
                    Thread.Sleep(1000);
                }

                listener.RevealCard();
                replayer.RevealCard();

                currentReplayer = MakeDelayedMarkReplayer(ReplayUI.MainDelay, replayer, CurrentReplayId);
                listener.OnGoalUpdateReceived += currentReplayer;
            });
        }

        public void StopReplay()
        {
            listener.ExitRoom(() => { });
            replayer.ExitRoom(() => { });
            GoalsInProgress = 0;
            listener.OnGoalUpdateReceived -= currentReplayer;
            CurrentReplayId = Guid.NewGuid();
        }

        private void RevealNewCard(object sender, NewCardEventInfo info)
        {
            Session session = sender as Session;
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                session.RevealCard();
            });
        }

        private EventHandler<GoalUpdateEventInfo> MakeDelayedMarkReplayer(int delaySeconds, Session replayer, Guid markReplayId)
        {
            return (sender, goalUpdate) =>
            {
                if (goalUpdate.Unmarking || !replayer.Board.GetSlot(goalUpdate.Slot).MarkedBy.Contains(BingoSync.Colors.Blank))
                {
                    return;
                }

                ++GoalsInProgress;
                
                Task.Run(() =>
                {
                    Thread.Sleep(delaySeconds * 1000);

                    Task.Run(() =>
                    {
                        Thread.Sleep(ReplayUI.SecondaryDelay * 1000);
                        if (markReplayId == CurrentReplayId)
                        {
                            --GoalsInProgress;
                            replayer.SelectSquare(goalUpdate.Slot + 1, goalUpdate.Color, () => Log($"Could not mark slot {goalUpdate.Slot + 1}."));
                        }
                    });
                });
            };
        }
    }
}