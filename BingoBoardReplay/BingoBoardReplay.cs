using BingoSync.Clients.EventInfoObjects;
using BingoSync.CustomGoals;
using BingoSync.Interfaces;
using BingoSync.Sessions;
using Modding;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BingoBoardReplay
{
    public class BingoBoardReplay : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
    {
        new public string GetName() => "BingoBoardReplay";

        public static string version = "1.3.2.0";
        public override string GetVersion() => version;

        public static BingoBoardReplay Instance;
        public GlobalSettings Settings = new();
        internal Session Listener { get; set; }
        internal Session Replayer { get; set; }

        private EventHandler<GoalUpdateEventInfo> currentReplayer;

        private bool waitingForReproduceBoard = false;

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
            ReplayUI.SetLogger(Log);

            OrderedLoader.OnCompletelyLoaded += delegate
            {
                Listener = SessionManager.CreateSession("SourceListener", BingoSync.Clients.Servers.BingoSync, false);
                Replayer = SessionManager.CreateSession("TargetReplayer", BingoSync.Clients.Servers.BingoSync, false);

                Listener.OnNewCardReceived += RevealNewCard;
                Replayer.OnNewCardReceived += RevealNewCard;
                Replayer.OnNewCardReceived += ReceiveReproducedBoard;
                Listener.OnRoomSettingsReceived += (sender, settings) => ReplayUI.SourceRoomTextSuffix = settings.IsLockout ? " (Lockout)" : " (Non-Lockout)";
                Replayer.OnRoomSettingsReceived += (sender, settings) => ReplayUI.DestinationRoomTextSuffix = settings.IsLockout ? " (Lockout)" : " (Non-Lockout)";
                Listener.OnCardRevealedBroadcastReceived += OnListenerRevealed;
            };
        }

        private void OnListenerRevealed(object sender, CardRevealedEventInfo revealedEventInfo)
        {
            Session session = sender as Session;
            if(revealedEventInfo.Player.UUID == session.RoomPlayerUUID)
            {
                ReplayUI.ListenerHasRevealed = true;
            }
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

                Listener.JoinRoom(ReplayUI.SourceRoomCode, "ReplayBot", ReplayUI.SourceRoomPassword, (ex) => { });
                while(Listener.ClientIsConnecting())
                {
                    Thread.Sleep(250);
                }

                Replayer.JoinRoom(ReplayUI.DestinationRoomCode, "ReplayBot", ReplayUI.DestinationRoomPassword, (ex) => { });
                while(Replayer.ClientIsConnecting())
                {
                    Thread.Sleep(250);
                }

                ReplayUI.BothClientsConnected = true;

                Listener.RevealCard();
                Replayer.RevealCard();

                currentReplayer = MakeDelayedMarkReplayer(ReplayUI.MainDelay, Replayer, CurrentReplayId);
                Listener.OnGoalUpdateReceived += currentReplayer;
            });
        }

        public void StopReplay()
        {
            Listener.ExitRoom(() => { });
            Replayer.ExitRoom(() => { });
            ReplayUI.BothClientsConnected = false;
            GoalsInProgress = 0;
            Listener.OnGoalUpdateReceived -= currentReplayer;
            CurrentReplayId = Guid.NewGuid();
        }

        public void ReproduceState()
        {
            Task.Run(() =>
            {
                List<Square> backupSquares = Listener.Board.SquaresToDisplay;
                List<BingoGoal> goals = [];
                foreach (Square square in backupSquares)
                {
                    goals.Add(new BingoGoal(square.Name));
                }

                waitingForReproduceBoard = true;
                Replayer.NewCard(goals, false, false);
                while (waitingForReproduceBoard)
                {
                    Thread.Sleep(250);
                }

                foreach(Square square in backupSquares)
                {
                    int i = square.GoalIndex;
                    foreach (BingoSync.Colors color in square.MarkedBy)
                    {
                        if(color != BingoSync.Colors.Blank)
                        {
                            Replayer.SelectIndex(i, color, () => {
                                Log("Couldn't mark a goal");
                            });
                        }
                    }
                }
            });

        }

        private void RevealNewCard(object sender, NewCardEventInfo info)
        {
            Session session = sender as Session;
            if (session == Listener)
            {
                ReplayUI.ListenerHasRevealed = false;
            }
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                session.RevealCard();
            });
        }

        private void ReceiveReproducedBoard(object sender, NewCardEventInfo info)
        {
            waitingForReproduceBoard = false;
        }

        private EventHandler<GoalUpdateEventInfo> MakeDelayedMarkReplayer(int delaySeconds, Session replayer, Guid markReplayId)
        {
            return (sender, goalUpdate) =>
            {
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
                            replayer.SelectIndex(goalUpdate.Index, goalUpdate.Color, () => Log($"Could not mark slot {goalUpdate.Index + 1}."), goalUpdate.Unmarking);
                        }
                    });
                });
            };
        }

        public void SetUIVisible(bool visible)
        {
            ReplayUI.IsVisible = visible;
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            return ModMenu.CreateMenuScreen(modListMenu);
        }

        public bool ToggleButtonInsideMenu => false;

    }
}