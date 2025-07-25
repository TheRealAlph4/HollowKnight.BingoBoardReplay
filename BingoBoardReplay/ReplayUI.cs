using MagicUI.Core;
using MagicUI.Elements;
using System;
using System.Linq;
using ContentType = UnityEngine.UI.InputField.ContentType;

namespace BingoBoardReplay
{
    static class ReplayUI
    {
        private readonly static int NARROW_WIDTH = 180;
        private readonly static int WIDE_WIDTH = 622;
        private readonly static int NORMAL_FONT_SIZE = 22; 
        private readonly static int BIG_FONT_SIZE = 32;
        private readonly static Padding TITLE_PADDING = new(0, 25, 0, 5);

        private readonly static LayoutRoot layoutRoot;
        private readonly static StackLayout mainStack;
        private readonly static StackLayout secondaryStack;

        private readonly static TextObject sourceRoomText;
        private readonly static TextInput sourceLink;
        private readonly static TextInput sourcePassword;

        private readonly static TextObject destinationRoomText;
        private readonly static TextInput destinationLink;
        private readonly static TextInput destinationPassword;

        private readonly static TextObject mainDelayText;
        private readonly static TextInput mainDelay;

        private readonly static Button replayButton;
        
        private readonly static TextObject secondaryDelayText;
        private readonly static TextInput secondaryDelay;
        private readonly static Button decreaseSecondaryDelay;
        private readonly static Button increaseSecondaryDelay;

        private static Action<string> Log;

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
                if (value)
                {
                    BingoBoardReplay.Instance.StartReplay();
                }
                else
                {
                    SourceRoomTextSuffix = "";
                    DestinationRoomTextSuffix = "";
                    BingoBoardReplay.Instance.StopReplay();
                }
                SetUIReplaying(value);
            }
        }

        public static int MarksInQueue
        {
            set
            {
                replayButton.Content = IsReplaying ? "Stop Replay (" + value.ToString() + " marks in queue)" : "Start Replay";
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
                sourceRoomText.Text = "Source Room" + value;
            }
        }

        public static string DestinationRoomTextSuffix
        {
            set
            {
                destinationRoomText.Text = "Destination Room" + value;
            }
        }

        public static void SetLog(Action<string> log)
        {
            Log = log;
        }

        static ReplayUI()
        {
            layoutRoot = new LayoutRoot(true, "BingoBoardReplay LayoutRoot");
            mainStack = new StackLayout(layoutRoot, "BingoBoardReplay MainStack")
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Orientation = Orientation.Vertical,
                Padding = new Padding(50),
                Spacing = 10,
            };
            secondaryStack = new StackLayout(layoutRoot, "BingoBoardReplay SecondaryStack")
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                Spacing = 10,
            };

            sourceRoomText = new TextObject(layoutRoot, "BingoBoardReplay SourceRoomText")
            {
                Text = "Source Room",
                FontSize = BIG_FONT_SIZE,
                Padding = TITLE_PADDING,
            };
            sourceLink = new TextInput(layoutRoot, "BingoBoardReplay SourceLink")
            {
                Placeholder = "Room Link",
                MinWidth = WIDE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
            };
            sourcePassword = new TextInput(layoutRoot, "BingoBoardReplay SourcePassword")
            {
                Placeholder = "Password",
                MinWidth = WIDE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
                ContentType = ContentType.Password,
            };

            destinationRoomText = new TextObject(layoutRoot, "BingoBoardReplay DestinationRoomText")
            {
                Text = "Destination Room",
                FontSize = BIG_FONT_SIZE,
                Padding = TITLE_PADDING,
            };
            destinationLink = new TextInput(layoutRoot, "BingoBoardReplay DestinationLink")
            {
                Placeholder = "Room Link",
                MinWidth = WIDE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
            };
            destinationPassword = new TextInput(layoutRoot, "BingoBoardReplay DestinationPassword")
            {
                Placeholder = "Password",
                MinWidth = WIDE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
                ContentType = ContentType.Password,
            };

            mainDelayText = new TextObject(layoutRoot, "BingoBoardReplay MainDelayText")
            {
                Text = "Delay (seconds)",
                FontSize = BIG_FONT_SIZE,
                Padding = TITLE_PADDING,
            };
            mainDelay = new TextInput(layoutRoot, "BingoBoardReplay MainDelay")
            {
                Placeholder = "Delay (seconds)",
                Text = BingoBoardReplay.Instance.Settings.DefaultMainDelaySeconds.ToString(),
                MinWidth = WIDE_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
                ContentType = ContentType.DecimalNumber,
            };

            replayButton = new Button(layoutRoot, "BingoBoardReplay ReplayButton")
            {
                Content = "Start Replay",
                MinWidth = WIDE_WIDTH,
                MinHeight = 50,
                FontSize = NORMAL_FONT_SIZE,
                Padding = new Padding(0, 25, 0, 0)
            };

            secondaryDelayText = new TextObject(layoutRoot, "BingoBoardReplay SecondaryDelayText")
            {
                Text = "Seconday Delay",
                FontSize = BIG_FONT_SIZE,
                Padding = TITLE_PADDING,
            };
            secondaryDelay = new TextInput(layoutRoot, "BingoBoardReplay SecondaryDelay")
            {
                Placeholder = "Secondary Delay",
                Text = BingoBoardReplay.Instance.Settings.DefaultSecondaryDelaySeconds.ToString(),
                MinWidth = NARROW_WIDTH,
                FontSize = NORMAL_FONT_SIZE,
                Enabled = false,
            };
            decreaseSecondaryDelay = new Button(layoutRoot, "BingoBoardReplay DecreaseSecondaryDelay")
            {
                Content = "Decrease",
                MinWidth = NARROW_WIDTH,
                MinHeight = 40,
                FontSize = NORMAL_FONT_SIZE,
            };
            increaseSecondaryDelay = new Button(layoutRoot, "BingoBoardReplay IncreaseSecondaryDelay")
            {
                Content = "Increase",
                MinWidth = NARROW_WIDTH,
                MinHeight = 40,
                FontSize = NORMAL_FONT_SIZE,
            };

            ArrangeUIElements();
            SetupOnClicks();
        }

        private static void ArrangeUIElements()
        {

            mainStack.Children.Add(sourceRoomText);
            mainStack.Children.Add(sourceLink);
            mainStack.Children.Add(sourcePassword);

            mainStack.Children.Add(destinationRoomText);
            mainStack.Children.Add(destinationLink);
            mainStack.Children.Add(destinationPassword);

            mainStack.Children.Add(mainDelayText);
            mainStack.Children.Add(mainDelay);

            mainStack.Children.Add(secondaryDelayText);
            secondaryStack.Children.Add(decreaseSecondaryDelay);
            secondaryStack.Children.Add(secondaryDelay);
            secondaryStack.Children.Add(increaseSecondaryDelay);
            mainStack.Children.Add(secondaryStack);

            mainStack.Children.Add(replayButton);
        }

        private static void SetupOnClicks()
        {
            replayButton.Click += ReplayButtonOnClick;
            increaseSecondaryDelay.Click += IncreaseSecondayDelayOnClick;
            decreaseSecondaryDelay.Click += DecreaseSecondayDelayOnClick;
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
            replayButton.Content = replaying ? "Stop Replay (0 marks in queue)" : "Start Replay";
        }

        private static void IncreaseSecondayDelayOnClick(Button _)
        {
            secondaryDelay.Text = (SecondaryDelay + 1).ToString();
            decreaseSecondaryDelay.Enabled = true;
        }

        private static void DecreaseSecondayDelayOnClick(Button _)
        {
            secondaryDelay.Text = (SecondaryDelay - 1).ToString();
            if(SecondaryDelay == 0)
            {
                decreaseSecondaryDelay.Enabled = false;
            }
        }
    }
}
