using MagicUI.Core;
using MagicUI.Elements;
using System;
using System.Linq;
using ContentType = UnityEngine.UI.InputField.ContentType;

namespace BingoBoardReplay
{
    static class ReplayUI
    {
        private readonly static int WIDE_WIDTH = 600;
        private readonly static int MIDDLE_WIDTH = 200;

        private readonly static int NORMAL_FONT_SIZE = 22;
        private readonly static int BIG_FONT_SIZE = 32;

        private readonly static Padding TITLE_PADDING = new(0, 25, 0, 5);
        private readonly static Padding GENERAL_PADDING = new(5);

        private readonly static LayoutRoot layoutRoot;
        private readonly static StackLayout mainStack;
        private readonly static StackLayout sourceRoomStack;
        private readonly static StackLayout middleStack;
        private readonly static StackLayout destinationRoomStack;
        private readonly static StackLayout secondaryDelayStack;

        private readonly static TextObject sourceRoomText;
        private readonly static TextInput sourceLink;
        private readonly static TextInput sourcePassword;

        private readonly static TextObject destinationRoomText;
        private readonly static TextInput destinationLink;
        private readonly static TextInput destinationPassword;

        private readonly static TextObject mainDelayText;
        private readonly static TextInput mainDelay;

        private readonly static Button replayButton;
        private readonly static TextInput goalsInProgressText;
        private readonly static Button reproduceButton;
        
        private readonly static TextObject secondaryDelayText;
        private readonly static TextInput secondaryDelay;
        private readonly static Button decreaseSecondaryDelay;
        private readonly static Button increaseSecondaryDelay;

        private static Action<string> Log;

        public static bool IsVisible { get; set; } = false;

        private static bool _isReplaying = false;
        private static bool IsReplaying
        {
            get
            {
                return _isReplaying;
            }
            set
            {
                _isReplaying = value;
                if (_isReplaying)
                {
                    BingoBoardReplay.Instance.StartReplay();
                    replayButton.Enabled = false;
                }
                else
                {
                    SourceRoomTextSuffix = "";
                    DestinationRoomTextSuffix = "";
                    BingoBoardReplay.Instance.StopReplay();
                    reproduceButton.Enabled = false;
                }
                SetUIReplaying(value);
            }
        }

        public static bool BothClientsConnected
        {
            set
            {
                if(value)
                {
                    replayButton.Enabled = true;
                }
            }
        }

        public static bool ListenerHasRevealed
        {
            set
            {
                reproduceButton.Enabled = value;
            }
        }

        public static int MarksInQueue
        {
            set
            {
                goalsInProgressText.Text = IsReplaying ? $"In Queue: {value}" : "No Replay";
            }
        }

        public static int MainDelay {
            get
            {
                return string.IsNullOrWhiteSpace(mainDelay.Text) ? 0 : int.Parse(mainDelay.Text);
            }
        }

        public static int SecondaryDelay {
            get
            {
                return string.IsNullOrWhiteSpace(secondaryDelay.Text) ? 0 : int.Parse(secondaryDelay.Text);
            }
        }

        public static string SourceRoomCode
        {
            get
            {
                return sourceLink.Text.Split('/').Last();
            }
        }

        public static string SourceRoomPassword
        {
            get
            {
                return sourcePassword.Text;
            }
        }

        public static string DestinationRoomCode
        {
            get
            {
                return destinationLink.Text.Split('/').Last();
            }
        }

        public static string DestinationRoomPassword
        {
            get
            {
                return destinationPassword.Text;
            }
        }

        public static string SourceRoomTextSuffix
        {
            set
            {
                sourceRoomText.Text = "Player Room" + value;
            }
        }

        public static string DestinationRoomTextSuffix
        {
            set
            {
                destinationRoomText.Text = "Replay Room" + value;
            }
        }

        public static void SetLogger(Action<string> log)
        {
            Log = log;
        }

        static ReplayUI()
        {
            layoutRoot = new LayoutRoot(true, "BingoBoardReplay LayoutRoot")
            {
                VisibilityCondition = () => IsVisible
            };
            mainStack = new StackLayout(layoutRoot, "BingoBoardReplay MainStack")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                Padding = GENERAL_PADDING,
            };

            sourceRoomStack = new StackLayout(layoutRoot, "BingoBoardReplay SourceRoomStack")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
                Padding = GENERAL_PADDING,
            };
            middleStack = new StackLayout(layoutRoot, "BingoBoardReplay MiddleStack")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
                Padding = GENERAL_PADDING,
            };
            destinationRoomStack = new StackLayout(layoutRoot, "BingoBoardReplay DestinationRoomStack")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
                Padding = GENERAL_PADDING,
            };

            secondaryDelayStack = new StackLayout(layoutRoot, "BingoBoardReplay SecondaryDelayStack")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                Padding = GENERAL_PADDING,
            };

            sourceRoomText = new TextObject(layoutRoot, "BingoBoardReplay SourceRoomText")
            {
                Text = "Player Room",
                FontSize = BIG_FONT_SIZE,
                Padding = TITLE_PADDING,
            };
            sourceLink = new TextInput(layoutRoot, "BingoBoardReplay SourceLink")
            {
                Placeholder = "Room Link",
                MinWidth = WIDE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
                Padding = GENERAL_PADDING,
            };
            sourcePassword = new TextInput(layoutRoot, "BingoBoardReplay SourcePassword")
            {
                Placeholder = "Password",
                MinWidth = WIDE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
                ContentType = ContentType.Password,
                Padding = GENERAL_PADDING,
            };

            destinationRoomText = new TextObject(layoutRoot, "BingoBoardReplay DestinationRoomText")
            {
                Text = "Replay Room",
                FontSize = BIG_FONT_SIZE,
                Padding = TITLE_PADDING,
            };
            destinationLink = new TextInput(layoutRoot, "BingoBoardReplay DestinationLink")
            {
                Placeholder = "Room Link",
                MinWidth = WIDE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
                Padding = GENERAL_PADDING,
            };
            destinationPassword = new TextInput(layoutRoot, "BingoBoardReplay DestinationPassword")
            {
                Placeholder = "Password",
                MinWidth = WIDE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
                ContentType = ContentType.Password,
                Padding = GENERAL_PADDING,
            };

            mainDelayText = new TextObject(layoutRoot, "BingoBoardReplay MainDelayText")
            {
                Text = "Main Delay",
                FontSize = BIG_FONT_SIZE,
                Padding = TITLE_PADDING,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            mainDelay = new TextInput(layoutRoot, "BingoBoardReplay MainDelay")
            {
                Placeholder = "Main Delay",
                Text = BingoBoardReplay.Instance.Settings.DefaultMainDelaySeconds.ToString(),
                MinWidth = MIDDLE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
                ContentType = ContentType.DecimalNumber,
                Padding = GENERAL_PADDING,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            replayButton = new Button(layoutRoot, "BingoBoardReplay ReplayButton")
            {
                Content = "Start Replay",
                MinWidth = MIDDLE_WIDTH,
                MinHeight = 50,
                FontSize = NORMAL_FONT_SIZE,
                Padding = GENERAL_PADDING,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            goalsInProgressText = new TextInput(layoutRoot, "BingoBoardReplay GoalsInProgressText")
            {
                Placeholder = "Queue",
                Text = "No Replay",
                MinWidth = MIDDLE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
                Padding = GENERAL_PADDING,
                HorizontalAlignment = HorizontalAlignment.Center,
                Enabled = false,
            };
            reproduceButton = new Button(layoutRoot, "BingoBoardReplay ReproduceButton")
            {
                Content = "Clone Board",
                MinWidth = MIDDLE_WIDTH,
                MinHeight = 50,
                FontSize = NORMAL_FONT_SIZE,
                Enabled = false,
                Padding = GENERAL_PADDING,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            secondaryDelayText = new TextObject(layoutRoot, "BingoBoardReplay SecondaryDelayText")
            {
                Text = "Secondary Delay",
                FontSize = NORMAL_FONT_SIZE - 1,
                Padding = TITLE_PADDING,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            secondaryDelay = new TextInput(layoutRoot, "BingoBoardReplay SecondaryDelay")
            {
                Text = BingoBoardReplay.Instance.Settings.DefaultSecondaryDelaySeconds.ToString(),
                MinWidth = MIDDLE_WIDTH - 2 * 50,
                FontSize = NORMAL_FONT_SIZE,
                Enabled = false,
                Padding = GENERAL_PADDING,
            };
            decreaseSecondaryDelay = new Button(layoutRoot, "BingoBoardReplay DecreaseSecondaryDelay")
            {
                Content = "-",
                MinWidth = 40,
                MinHeight = 40,
                FontSize = NORMAL_FONT_SIZE,
                Padding = GENERAL_PADDING,
            };
            increaseSecondaryDelay = new Button(layoutRoot, "BingoBoardReplay IncreaseSecondaryDelay")
            {
                Content = "+",
                MinWidth = 40,
                MinHeight = 40,
                FontSize = NORMAL_FONT_SIZE,
                Padding = GENERAL_PADDING,
            };

            ArrangeUIElements();
            SetupOnClicks();
        }

        private static void ArrangeUIElements()
        {

            sourceRoomStack.Children.Add(sourceRoomText);
            sourceRoomStack.Children.Add(sourceLink);
            sourceRoomStack.Children.Add(sourcePassword);

            destinationRoomStack.Children.Add(destinationRoomText);
            destinationRoomStack.Children.Add(destinationLink);
            destinationRoomStack.Children.Add(destinationPassword);

            middleStack.Children.Add(mainDelayText);
            middleStack.Children.Add(mainDelay);

            middleStack.Children.Add(secondaryDelayText);
            secondaryDelayStack.Children.Add(decreaseSecondaryDelay);
            secondaryDelayStack.Children.Add(secondaryDelay);
            secondaryDelayStack.Children.Add(increaseSecondaryDelay);
            middleStack.Children.Add(secondaryDelayStack);

            middleStack.Children.Add(replayButton);
            middleStack.Children.Add(goalsInProgressText);
            middleStack.Children.Add(reproduceButton);

            mainStack.Children.Add(sourceRoomStack);
            mainStack.Children.Add(middleStack);
            mainStack.Children.Add(destinationRoomStack);
        }

        private static void SetupOnClicks()
        {
            replayButton.Click += ReplayButtonOnClick;
            increaseSecondaryDelay.Click += IncreaseSecondaryDelayOnClick;
            decreaseSecondaryDelay.Click += DecreaseSecondaryDelayOnClick;
            reproduceButton.Click += ReproduceButtonOnClick; 
        }

        private static void ReplayButtonOnClick(Button _)
        {
            if(!IsReplaying && 
               (string.IsNullOrWhiteSpace(SourceRoomCode) ||
                string.IsNullOrWhiteSpace(SourceRoomPassword) ||
                string.IsNullOrWhiteSpace(DestinationRoomCode) ||
                string.IsNullOrWhiteSpace(DestinationRoomPassword)))
            {
                return;
            }
            IsReplaying = !IsReplaying;
        }

        private static void SetUIReplaying(bool replaying)
        {
            sourceLink.Enabled = !replaying;
            sourcePassword.Enabled = !replaying;
            destinationLink.Enabled = !replaying;
            destinationPassword.Enabled = !replaying;
            mainDelay.Enabled = !replaying;
            MarksInQueue = 0;
            replayButton.Content = replaying ? "Stop Replay" : "Start Replay";
        }

        private static void IncreaseSecondaryDelayOnClick(Button _)
        {
            secondaryDelay.Text = (SecondaryDelay + 1).ToString();
            decreaseSecondaryDelay.Enabled = true;
        }

        private static void DecreaseSecondaryDelayOnClick(Button _)
        {
            secondaryDelay.Text = (SecondaryDelay - 1).ToString();
            if(SecondaryDelay == 0)
            {
                decreaseSecondaryDelay.Enabled = false;
            }
        }

        private static void ReproduceButtonOnClick(Button _)
        {
            BingoBoardReplay.Instance.ReproduceState();
        }
    }
}
