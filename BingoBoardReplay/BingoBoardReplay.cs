using BingoSync;
using BingoSync.Clients.EventInfoObjects;
using BingoSync.Clients.StateChangeInfoObjects;
using BingoSync.Interfaces;
using BingoSync.Sessions;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BingoBoardReplay
{
    public class BingoBoardReplay : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
    {
        new public string GetName() => "BingoBoardReplay";

        public static string version = "1.4.0.1";
        public override string GetVersion() => version;

        public static BingoBoardReplay Instance;
        public GlobalSettings Settings = new();
        internal Session Listener { get; set; }
        internal Session Replayer { get; set; }
        internal Session ColorDummy1 { get; set; }
        internal Session ColorDummy2 { get; set; }

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
                Interlocked.Exchange(ref _goalsInProgress, value);
                ReplayUI.MarksInQueue = _goalsInProgress;
            }
        }

        private Guid CurrentReplayId { get; set; } = Guid.NewGuid();

        public override void Initialize()
        {
            Instance = this;
            Log("Initializing");
            ReplayUI.SetLogger(Log);

            OrderedLoader.OnCompletelyLoaded += LoadSessions;
        }

        private void LoadSessions(object sender, EventArgs e)
        {
            Log("BingoBoardReplay.LoadSessions");
            Listener = SessionManager.CreateSession("SourceListener", BingoSync.Clients.Servers.BingoSync, false, false);
            Replayer = SessionManager.CreateSession("TargetReplayer", BingoSync.Clients.Servers.BingoSync, false, false);
            ColorDummy1 = SessionManager.CreateSession("ColorDummy1", BingoSync.Clients.Servers.BingoSync, false, false);
            ColorDummy2 = SessionManager.CreateSession("ColorDummy2", BingoSync.Clients.Servers.BingoSync, false, false);

            Listener.OnNewBoardEventReceived += RevealNewBoard;
            Replayer.OnNewBoardEventReceived += RevealNewBoard;
            Replayer.OnNewBoardEventReceived += ReceiveReproducedBoard;
            Listener.OnRoomSettingsChanged += OnListenerRoomSettingsReceived;
            Listener.OnRoomSettingsChanged += (sender, settings) => ReplayUI.SourceRoomTextSuffix = settings.IsLockout ? " (Lockout)" : " (Non-Lockout)";
            Replayer.OnRoomSettingsChanged += (sender, settings) => ReplayUI.DestinationRoomTextSuffix = settings.IsLockout ? " (Lockout)" : " (Non-Lockout)";
            Listener.OnBoardRevealed += OnListenerRevealed;
        }

        private void OnListenerRevealed(object sender, EventArgs _)
        {
            ReplayUI.ListenerHasRevealed = true;
        }

        private void OnListenerRoomSettingsReceived(object sender, RoomSettings roomSettings)
        {
            if(!roomSettings.HideBoard)
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

        public void StartReplay(Action callback = null)
        {
            ReplayUI.IsReplaying = true;
            Task.Run(() =>
            {
                CurrentReplayId = Guid.NewGuid();

                Listener.JoinRoom(ReplayUI.SourceRoomCode, "ReplayBot", ReplayUI.SourceRoomPassword, 0);
                ColorDummy1.JoinRoom(ReplayUI.DestinationRoomCode, "_Dummy1", ReplayUI.DestinationRoomPassword, 0);
                ColorDummy2.JoinRoom(ReplayUI.DestinationRoomCode, "_Dummy2", ReplayUI.DestinationRoomPassword, 0);
                Replayer.JoinRoom(ReplayUI.DestinationRoomCode, "ReplayBot", ReplayUI.DestinationRoomPassword, 0);

                while(Listener.ClientIsConnecting() || ColorDummy1.ClientIsConnecting() || ColorDummy2.ClientIsConnecting() || Replayer.ClientIsConnecting())
                {
                    Thread.Sleep(100);
                }

                if (!Listener.ClientIsConnected() || !ColorDummy1.ClientIsConnected() || !ColorDummy2.ClientIsConnected() || !Replayer.ClientIsConnected())
                {
                    StopReplay();
                    return;
                }

                ReplayUI.BothClientsConnected = true;

                currentReplayer = MakeDelayedMarkReplayer(TimeSpan.FromSeconds(ReplayUI.MainDelay), Replayer, CurrentReplayId);
                Listener.OnGoalUpdateEventReceived += currentReplayer;

                Listener.RevealBoard();
                Replayer.RevealBoard();

                callback?.Invoke();
            });
        }

        public void StopReplay()
        {
            ReplayUI.IsReplaying = false;
            Listener.ExitRoom(() => { });
            Replayer.ExitRoom(() => { });
            ColorDummy1.ExitRoom(() => { });
            ColorDummy2.ExitRoom(() => { });
            ReplayUI.BothClientsConnected = false;
            GoalsInProgress = 0;
            Listener.OnGoalUpdateEventReceived -= currentReplayer;
            CurrentReplayId = Guid.NewGuid();
        }

        public void ReproduceState()
        {
            Task.Run(() =>
            {
                List<BoardSquare> backupSquares = Listener.SquareManager.SquaresToDisplay;
                List<string> goals = [.. backupSquares.Select(goal => string.Copy(goal.Name))];

                waitingForReproduceBoard = true;

                bool lockout = false;
                switch(Settings.LockoutWhenCloning)
                {
                    case GlobalSettings.CloneRoomOption.Always: lockout = true; break;
                    case GlobalSettings.CloneRoomOption.Never: lockout = false; break;
                    case GlobalSettings.CloneRoomOption.Copy: lockout = Listener.RoomIsLockout; break;
                }
                bool hide = false;
                switch (Settings.HideWhenCloning)
                {
                    case GlobalSettings.CloneRoomOption.Always: hide = true; break;
                    case GlobalSettings.CloneRoomOption.Never: hide = false; break;
                    case GlobalSettings.CloneRoomOption.Copy: hide = Listener.RoomHidBoardInitially; break;
                }

                Replayer.NewBoard(goals, lockout, hide);
                while (waitingForReproduceBoard)
                {
                    Thread.Sleep(250);
                }

                foreach(BoardSquare square in backupSquares)
                {
                    int i = square.GoalIndex;
                    foreach (int color in square.MarkedBy)
                    {
                        if(color != -1)
                        {
                            Replayer.SelectIndex(i, color);
                        }
                    }
                }
            });

        }

        private void RevealNewBoard(object sender, NewBoardEventInfo info)
        {
            Session session = sender as Session;
            if (session == Listener)
            {
                ReplayUI.ListenerHasRevealed = false;
            }
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                session.RevealBoard();
            });
        }

        private void ReceiveReproducedBoard(object sender, NewBoardEventInfo info)
        {
            waitingForReproduceBoard = false;
        }

        private EventHandler<GoalUpdateEventInfo> MakeDelayedMarkReplayer(TimeSpan delay, Session replayer, Guid markReplayId)
        {
            return (sender, goalUpdate) =>
            {
                ++GoalsInProgress;
                Task.Run(() =>
                {
                    Thread.Sleep(delay);

                    Task.Run(() =>
                    {
                        Thread.Sleep(ReplayUI.SecondaryDelay * 1000);
                        if (markReplayId == CurrentReplayId)
                        {
                            --GoalsInProgress;
                            replayer.SelectIndex(goalUpdate.Index, goalUpdate.Color, goalUpdate.Unmark);
                        }
                    });
                });
            };
        }

        public void Reconnect()
        {
            static Tuple<DateTimeOffset, DateTimeOffset> findStartEndTimestamps(List<RoomEventInfo> infoList)
            {
                int lastNewCard = 0;
                int replayBotReveal = infoList.Count - 1;
                for (int i = 0; i < infoList.Count; ++i)
                {
                    if (infoList[i] is NewBoardEventInfo)
                    {
                        lastNewCard = i;
                    }
                    if (infoList[i] is BoardRevealedEventInfo)
                    {
                        BoardRevealedEventInfo reveal = infoList[i] as BoardRevealedEventInfo;
                        if (reveal.Player.UUID == Instance.Listener.RoomPlayerUUID)
                        {
                            replayBotReveal = i;
                        }
                    }
                }
                DateTimeOffset start = ParseTimestampString(infoList[lastNewCard].Timestamp);
                DateTimeOffset end = ParseTimestampString(infoList[replayBotReveal].Timestamp);
                return new Tuple<DateTimeOffset, DateTimeOffset>(start, end);
            }
            static int parseStateUntilCutoff(List<GoalUpdateEventInfo> goalUpdates, List<HashSet<int>> stateAtReplayCutoff)
            {
                DateTimeOffset cutoff = DateTimeOffset.UtcNow - TimeSpan.FromSeconds(ReplayUI.MainDelay + ReplayUI.SecondaryDelay);
                for (int j = 0; j < 25; ++j)
                {
                    stateAtReplayCutoff.Add([]);
                }

                int i = 0;
                for (; i < goalUpdates.Count; ++i)
                {
                    if (ParseTimestampString(goalUpdates[i].Timestamp) >= cutoff)
                    {
                        break;
                    }
                    GoalUpdateEventInfo goalUpdate = goalUpdates[i];
                    if (goalUpdate.Unmark)
                    {
                        stateAtReplayCutoff[goalUpdate.Index].Remove(goalUpdate.Color);
                    }
                    else
                    {
                        stateAtReplayCutoff[goalUpdate.Index].Add(goalUpdate.Color);
                    }
                }
                return i;
            }
            void replicatePreCutoff(List<HashSet<int>> stateAtReplayCutoff, bool unmark = false)
            {
                for(int i = 0; i < 25; ++i)
                {
                    foreach (int color in stateAtReplayCutoff[i])
                    {
                        if (!Replayer.SquareManager.SquaresToDisplay[i].MarkedBy.Contains(color))
                        {
                            Replayer.SelectIndex(i, color);
                        }
                    }
                    if (unmark)
                    {
                        foreach (int color in Replayer.SquareManager.SquaresToDisplay[i].MarkedBy)
                        {
                            if (!stateAtReplayCutoff[i].Contains(color))
                            {
                                Replayer.SelectIndex(i, color, true);
                            }
                        }
                    }
                }
            }
            int restartSecondaryDelays(List<GoalUpdateEventInfo> goalUpdates, int firstAfterCutoff)
            {
                DateTimeOffset cutoff = DateTimeOffset.UtcNow - TimeSpan.FromSeconds(ReplayUI.MainDelay);
                int i = firstAfterCutoff;
                for (; i < goalUpdates.Count; ++i)
                {
                    if (ParseTimestampString(goalUpdates[i].Timestamp) >= cutoff)
                    {
                        break;
                    }
                    GoalUpdateEventInfo goalUpdate = goalUpdates[i];
                    TimeSpan delayLeft = TimeSpan.FromSeconds(ReplayUI.SecondaryDelay) - (cutoff - ParseTimestampString(goalUpdate.Timestamp));
                    CancellableDelayedMark(Replayer, goalUpdate, CurrentReplayId, delayLeft);
                }
                return i;
            }
            void restartMainDelays(List<GoalUpdateEventInfo> goalUpdates, int mainDelayCutoff)
            {
                int i = mainDelayCutoff;
                for (; i < goalUpdates.Count; ++i)
                {
                    GoalUpdateEventInfo goalUpdate = goalUpdates[i];
                    TimeSpan mainDelay = TimeSpan.FromSeconds(ReplayUI.MainDelay) - (DateTimeOffset.UtcNow - ParseTimestampString(goalUpdate.Timestamp));
                    MakeDelayedMarkReplayer(mainDelay, Replayer, CurrentReplayId)(Listener, goalUpdate);
                }
            }

            StartReplay(() =>
            {
                Listener.ProcessRoomHistory((infoList) =>
                {
                    List<GoalUpdateEventInfo> goalUpdates = [];
                    Tuple<DateTimeOffset, DateTimeOffset> startEndTuple = findStartEndTimestamps(infoList);
                    DateTimeOffset start = startEndTuple.Item1;
                    DateTimeOffset end = startEndTuple.Item2;
                    foreach (RoomEventInfo info in infoList)
                    {
                        if(info is GoalUpdateEventInfo)
                        {
                            GoalUpdateEventInfo goalUpdate = info as GoalUpdateEventInfo;
                            DateTimeOffset timestamp = ParseTimestampString(goalUpdate.Timestamp);
                            if ((start <= timestamp) && (timestamp <= end))
                            {
                                goalUpdates.Add(goalUpdate);
                            }
                        }
                    }
                    List<HashSet<int>> stateAtReplayCutoff = [];
                    int cutoffIndex = parseStateUntilCutoff(goalUpdates, stateAtReplayCutoff);
                    replicatePreCutoff(stateAtReplayCutoff, false);
                    int mainDelayCutoff = restartSecondaryDelays(goalUpdates, cutoffIndex);
                    restartMainDelays(goalUpdates, mainDelayCutoff);
                });
            });
        }

        public void CancellableDelayedMark(Session replayer, GoalUpdateEventInfo goalUpdate, Guid markReplayId, TimeSpan delay)
        {
            Task.Run(() =>
            {
                ++GoalsInProgress;
                Thread.Sleep(delay);
                if (markReplayId == CurrentReplayId)
                {
                    --GoalsInProgress;
                    replayer.SelectIndex(goalUpdate.Index, goalUpdate.Color, goalUpdate.Unmark);
                }
            });
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

        private static DateTimeOffset ParseTimestampString(string timestamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(Double.Parse(timestamp.Trim().Replace('.', ',')) * 1000));
        }
    }
}