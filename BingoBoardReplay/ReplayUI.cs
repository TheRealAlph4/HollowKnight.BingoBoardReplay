using BingoSync;
using BingoSync.Sessions;
using MagicUI.Core;
using MagicUI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ContentType = UnityEngine.UI.InputField.ContentType;

namespace BingoBoardReplay
{
    static class ReplayUI
    {
        private const int WIDE_WIDTH = 600;
        private const int MIDDLE_WIDTH = 200;
        private const int COLOR_BUTTON_WIDTH = 100;
        private const int SETTING_BUTTON_WIDTH = 100;

        private const int SETTING_BUTTON_HEIGHT = 35;
        private const int BIG_BUTTON_HEIGHT = 50;

        private const int SMALL_FONT_SIZE = 16;
        private const int NORMAL_FONT_SIZE = 22;
        private const int BIG_FONT_SIZE = 32;

        private readonly static Padding TITLE_PADDING = new(0, 25, 0, 5);
        private readonly static Padding GENERAL_PADDING = new(5);

        private readonly static Color GLOBAL_SETTINGS_INACTIVE_COLOR = Color.white;
        private readonly static Color GLOBAL_SETTINGS_ACTIVE_COLOR = Color.red;

        private readonly static LayoutRoot layoutRoot;
        private readonly static StackLayout mainVerticalStack;
        private readonly static StackLayout mainReplayStack;
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

        private readonly static TextObject secondaryDelayText;
        private readonly static TextInput secondaryDelay;
        private readonly static Button decreaseSecondaryDelay;
        private readonly static Button increaseSecondaryDelay;

        private readonly static Button replayButton;
        private readonly static TextInput goalsInProgressText;
        private readonly static Button reproduceButton;
        private readonly static Button reconnectButton;

        private readonly static StackLayout colorDummiesStack;
        private readonly static StackLayout dummy1Row;
        private readonly static StackLayout dummy2Row;
        private readonly static TextObject dummy1Text;
        private readonly static TextObject dummy2Text;
        private readonly static StackLayout dummy1ColorSelector;
        private readonly static StackLayout dummy2ColorSelector;

        private readonly static List<Button> dummy1ColorButtons = [];
        private readonly static List<Button> dummy2ColorButtons = [];

        private readonly static StackLayout globalSettingsStack;
        private readonly static StackLayout hideCardSettingStack;
        private readonly static StackLayout lockoutSettingStack;

        private readonly static TextObject globalSettingsText;

        private readonly static TextObject lockoutSettingText;
        private readonly static Button lockoutSettingAlwaysButton;
        private readonly static Button lockoutSettingNeverButton;
        private readonly static Button lockoutSettingCopyButton;

        private readonly static TextObject hideCardSettingText;
        private readonly static Button hideCardSettingAlwaysButton;
        private readonly static Button hideCardSettingNeverButton;
        private readonly static Button hideCardSettingCopyButton;
        
        private static Action<string> Log;

        public static bool IsVisible { get; set; } = false;

        private static bool _isReplaying = false;
        public static bool IsReplaying
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
                    replayButton.Enabled = false;
                    reconnectButton.Enabled = false;
                }
                else
                {
                    SourceRoomTextSuffix = "";
                    DestinationRoomTextSuffix = "";
                    reproduceButton.Enabled = false;
                    reconnectButton.Enabled = true;
                }
                SetUIReplaying(value);
            }
        }

        public static bool BothClientsConnected
        {
            set
            {
                replayButton.Enabled = true;
                foreach (Button button in dummy1ColorButtons)
                {
                    button.Enabled = value;
                }
                foreach (Button button in dummy2ColorButtons)
                {
                    button.Enabled = value;
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
            mainVerticalStack = new StackLayout(layoutRoot, "BingoBoardReplay MainVerticalStack")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
                Padding = GENERAL_PADDING,
            };

            mainReplayStack = new StackLayout(layoutRoot, "BingoBoardReplay MainReplayStack")
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

            colorDummiesStack = new StackLayout(layoutRoot, "BingoBoardReplay ColorDummiesStack")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
                Padding = GENERAL_PADDING,
            };
            dummy1Row = new StackLayout(layoutRoot, "BingoBoardReplay Dummy1Row")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                Padding = GENERAL_PADDING,
            };
            dummy2Row = new StackLayout(layoutRoot, "BingoBoardReplay Dummy2Row")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                Padding = GENERAL_PADDING,
            };
            dummy1Text = new TextObject(layoutRoot, "BingoBoardReplay Dummy1Text")
            {
                Text = "Color Dummy 1: ",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = BIG_FONT_SIZE,
                Padding = GENERAL_PADDING,
            };
            dummy2Text = new TextObject(layoutRoot, "BingoBoardReplay Dummy2Text")
            {
                Text = "Color Dummy 2: ",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = BIG_FONT_SIZE,
                Padding = GENERAL_PADDING,
            };

            dummy1ColorSelector = CreateDummyColorSelector(true);
            dummy2ColorSelector = CreateDummyColorSelector(false);

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
                MinHeight = BIG_BUTTON_HEIGHT,
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
                MinHeight = BIG_BUTTON_HEIGHT,
                FontSize = NORMAL_FONT_SIZE,
                Enabled = false,
                Padding = GENERAL_PADDING,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            reconnectButton = new Button(layoutRoot, "BingoBoardReplay ReconnectButton")
            {
                Content = "Reconnect",
                MinWidth = MIDDLE_WIDTH,
                MinHeight = BIG_BUTTON_HEIGHT,
                FontSize = NORMAL_FONT_SIZE,
                Enabled = true,
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

            globalSettingsStack = new StackLayout(layoutRoot, "BingoBoardReplay GlobalSettingsStack")
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Orientation = Orientation.Vertical,
                Padding = new(5, 0, 0, 20),
            };
            lockoutSettingStack = new StackLayout(layoutRoot, "BingoBoardReplay LockoutSettingStack")
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Orientation = Orientation.Horizontal,
                Padding = GENERAL_PADDING,
            };
            hideCardSettingStack = new StackLayout(layoutRoot, "BingoBoardReplay HideCardSettingStack")
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Orientation = Orientation.Horizontal,
                Padding = GENERAL_PADDING,
            };

            globalSettingsText = new TextObject(layoutRoot, "BingoBoardReplay GlobalSettingsText")
            {
                Text = "Clone Settings",
                FontSize = BIG_FONT_SIZE,
                Padding = GENERAL_PADDING,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };

            lockoutSettingText = new TextObject(layoutRoot, "BingoBoardReplay LockoutSettingText")
            {
                Text = "Lockout Mode: ",
                FontSize = NORMAL_FONT_SIZE,
                Padding = GENERAL_PADDING,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            lockoutSettingAlwaysButton = new Button(layoutRoot, "BingoBoardReplay LockoutSettingAlwaysButton")
            {
                Content = "Always",
                MinWidth = SETTING_BUTTON_WIDTH,
                MinHeight = SETTING_BUTTON_HEIGHT,
                FontSize = SMALL_FONT_SIZE,
                Padding = GENERAL_PADDING,
                BorderColor = BingoBoardReplay.Instance.Settings.LockoutWhenCloning == GlobalSettings.CloneRoomOption.Always ? GLOBAL_SETTINGS_ACTIVE_COLOR : GLOBAL_SETTINGS_INACTIVE_COLOR,
            };
            lockoutSettingNeverButton = new Button(layoutRoot, "BingoBoardReplay LockoutSettingNeverButton")
            {
                Content = "Never",
                MinWidth = SETTING_BUTTON_WIDTH,
                MinHeight = SETTING_BUTTON_HEIGHT,
                FontSize = SMALL_FONT_SIZE,
                Padding = GENERAL_PADDING,
                BorderColor = BingoBoardReplay.Instance.Settings.LockoutWhenCloning == GlobalSettings.CloneRoomOption.Never ? GLOBAL_SETTINGS_ACTIVE_COLOR : GLOBAL_SETTINGS_INACTIVE_COLOR,
            };
            lockoutSettingCopyButton = new Button(layoutRoot, "BingoBoardReplay LockoutSettingCopyButton")
            {
                Content = "Copy",
                MinWidth = SETTING_BUTTON_WIDTH,
                MinHeight = SETTING_BUTTON_HEIGHT,
                FontSize = SMALL_FONT_SIZE,
                Padding = GENERAL_PADDING,
                BorderColor = BingoBoardReplay.Instance.Settings.LockoutWhenCloning == GlobalSettings.CloneRoomOption.Copy ? GLOBAL_SETTINGS_ACTIVE_COLOR : GLOBAL_SETTINGS_INACTIVE_COLOR,
            };

            hideCardSettingText = new TextObject(layoutRoot, "BingoBoardReplay HideCardSettingText")
            {
                Text = "Hide Card: ",
                FontSize = NORMAL_FONT_SIZE,
                Padding = GENERAL_PADDING,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            hideCardSettingAlwaysButton = new Button(layoutRoot, "BingoBoardReplay HideCardSettingAlwaysButton")
            {
                Content = "Always",
                MinWidth = SETTING_BUTTON_WIDTH,
                MinHeight = SETTING_BUTTON_HEIGHT,
                FontSize = SMALL_FONT_SIZE,
                Padding = GENERAL_PADDING,
                BorderColor = BingoBoardReplay.Instance.Settings.HideWhenCloning == GlobalSettings.CloneRoomOption.Always ? GLOBAL_SETTINGS_ACTIVE_COLOR : GLOBAL_SETTINGS_INACTIVE_COLOR,
            };
            hideCardSettingNeverButton = new Button(layoutRoot, "BingoBoardReplay HideCardSettingNeverButton")
            {
                Content = "Never",
                MinWidth = SETTING_BUTTON_WIDTH,
                MinHeight = SETTING_BUTTON_HEIGHT,
                FontSize = SMALL_FONT_SIZE,
                Padding = GENERAL_PADDING,
                BorderColor = BingoBoardReplay.Instance.Settings.HideWhenCloning == GlobalSettings.CloneRoomOption.Never ? GLOBAL_SETTINGS_ACTIVE_COLOR : GLOBAL_SETTINGS_INACTIVE_COLOR,
            };
            hideCardSettingCopyButton = new Button(layoutRoot, "BingoBoardReplay HideCardSettingCopyButton")
            {
                Content = "Copy",
                MinWidth = SETTING_BUTTON_WIDTH,
                MinHeight = SETTING_BUTTON_HEIGHT,
                FontSize = SMALL_FONT_SIZE,
                Padding = GENERAL_PADDING,
                BorderColor = BingoBoardReplay.Instance.Settings.HideWhenCloning == GlobalSettings.CloneRoomOption.Copy ? GLOBAL_SETTINGS_ACTIVE_COLOR : GLOBAL_SETTINGS_INACTIVE_COLOR,
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
            middleStack.Children.Add(reconnectButton);

            mainReplayStack.Children.Add(sourceRoomStack);
            mainReplayStack.Children.Add(middleStack);
            mainReplayStack.Children.Add(destinationRoomStack);

            dummy1Row.Children.Add(dummy1Text);
            dummy1Row.Children.Add(dummy1ColorSelector);

            dummy2Row.Children.Add(dummy2Text);
            dummy2Row.Children.Add(dummy2ColorSelector);

            colorDummiesStack.Children.Add(dummy1Row); 
            colorDummiesStack.Children.Add(dummy2Row); 

            mainVerticalStack.Children.Add(mainReplayStack);
            mainVerticalStack.Children.Add(colorDummiesStack);

            lockoutSettingStack.Children.Add(lockoutSettingAlwaysButton);
            lockoutSettingStack.Children.Add(lockoutSettingNeverButton);
            lockoutSettingStack.Children.Add(lockoutSettingCopyButton);

            hideCardSettingStack.Children.Add(hideCardSettingAlwaysButton);
            hideCardSettingStack.Children.Add(hideCardSettingNeverButton);
            hideCardSettingStack.Children.Add(hideCardSettingCopyButton);

            globalSettingsStack.Children.Add(globalSettingsText);
            globalSettingsStack.Children.Add(lockoutSettingText);
            globalSettingsStack.Children.Add(lockoutSettingStack);
            globalSettingsStack.Children.Add(hideCardSettingText);
            globalSettingsStack.Children.Add(hideCardSettingStack);
        }

        private static void SetupOnClicks()
        {
            replayButton.Click += ReplayButtonOnClick;
            increaseSecondaryDelay.Click += IncreaseSecondaryDelayOnClick;
            decreaseSecondaryDelay.Click += DecreaseSecondaryDelayOnClick;
            reproduceButton.Click += ReproduceButtonOnClick;
            reconnectButton.Click += ReconnectButtonOnClick;

            lockoutSettingAlwaysButton.Click += SetLockoutGlobalSettingTo(GlobalSettings.CloneRoomOption.Always);
            lockoutSettingNeverButton.Click += SetLockoutGlobalSettingTo(GlobalSettings.CloneRoomOption.Never);
            lockoutSettingCopyButton.Click += SetLockoutGlobalSettingTo(GlobalSettings.CloneRoomOption.Copy);

            hideCardSettingAlwaysButton.Click += SetHideCardGlobalSettingTo(GlobalSettings.CloneRoomOption.Always);
            hideCardSettingNeverButton.Click += SetHideCardGlobalSettingTo(GlobalSettings.CloneRoomOption.Never);
            hideCardSettingCopyButton.Click += SetHideCardGlobalSettingTo(GlobalSettings.CloneRoomOption.Copy);
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
            if(IsReplaying)
            {
                BingoBoardReplay.Instance.StopReplay();
            }
            else
            {
                BingoBoardReplay.Instance.StartReplay();
            }
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

        private static StackLayout CreateDummyColorSelector(bool dummy1)
        {
            int dummy = dummy1 ? 1 : 2;
            List<Button> buttons = dummy1 ? dummy1ColorButtons : dummy2ColorButtons;
            StackLayout colorSelectorStack = new(layoutRoot, $"BingoBoardReplay Dummy{dummy}RoomColorRow")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10,
                Orientation = Orientation.Vertical,
                Padding = new Padding(0, 10, 0, 0),
            };
            StackLayout row1 = new(layoutRoot, $"BingoBoardReplay Dummy{dummy}ColorRow1")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };
            StackLayout row2 = new(layoutRoot, $"BingoBoardReplay Dummy{dummy}ColorRow2")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };
            foreach (Colors color in Enum.GetValues(typeof(Colors)))
            {
                string name = char.ToUpper(color.GetName()[0]) + color.GetName().Substring(1);
                Action<Button> onClick = MakeColorButtonOnClick(dummy1, buttons, color);
                Button button = CreateColorButton(name, color.GetColor(), onClick);
                buttons.Add(button);
            }

            row1.Children.Add(buttons[0]);
            row1.Children.Add(buttons[1]);
            row1.Children.Add(buttons[2]);
            row1.Children.Add(buttons[3]);
            row1.Children.Add(buttons[4]);

            row2.Children.Add(buttons[5]);
            row2.Children.Add(buttons[6]);
            row2.Children.Add(buttons[7]);
            row2.Children.Add(buttons[8]);
            row2.Children.Add(buttons[9]);

            colorSelectorStack.Children.Add(row1);
            colorSelectorStack.Children.Add(row2);

            return colorSelectorStack;
        }

        private static Button CreateColorButton(string text, Color color, Action<Button> onClick)
        {
            Button button = new(layoutRoot, text.ToLower())
            {
                Content = text,
                FontSize = 15,
                Margin = 20,
                BorderColor = color,
                ContentColor = color,
                MinWidth = COLOR_BUTTON_WIDTH,
                Enabled = false,
            };
            button.Click += onClick;
            return button;
        }

        private static Action<Button> MakeColorButtonOnClick(bool dummy1, List<Button> otherButtons, Colors color)
        {
            return button =>
            {
                foreach (Button otherButton in otherButtons)
                {
                    otherButton.BorderColor = otherButton.ContentColor;
                }
                button.BorderColor = Color.white;
                if(dummy1)
                {
                    BingoBoardReplay.Instance.ColorDummy1.SetColor(color);
                }
                else
                {
                    BingoBoardReplay.Instance.ColorDummy2.SetColor(color);
                }
            };
        }

        private static Action<Button> SetLockoutGlobalSettingTo(GlobalSettings.CloneRoomOption value)
        {
            return button => {
                foreach (ArrangableElement element in lockoutSettingStack.Children)
                {
                    if (element is Button)
                    {
                        Button other = element as Button;
                        other.BorderColor = GLOBAL_SETTINGS_INACTIVE_COLOR;
                    }
                }
                button.BorderColor = GLOBAL_SETTINGS_ACTIVE_COLOR;
                BingoBoardReplay.Instance.Settings.LockoutWhenCloning = value;
            };
        }

        private static Action<Button> SetHideCardGlobalSettingTo(GlobalSettings.CloneRoomOption value)
        {
            return button => {
                foreach (ArrangableElement element in hideCardSettingStack.Children)
                {
                    if (element is Button)
                    {
                        Button other = element as Button;
                        other.BorderColor = GLOBAL_SETTINGS_INACTIVE_COLOR;
                    }
                }
                button.BorderColor = GLOBAL_SETTINGS_ACTIVE_COLOR;
                BingoBoardReplay.Instance.Settings.HideWhenCloning = value;
            };
        }

        private static void ReconnectButtonOnClick(Button _)
        {
            if (string.IsNullOrWhiteSpace(SourceRoomCode) ||
                string.IsNullOrWhiteSpace(SourceRoomPassword) ||
                string.IsNullOrWhiteSpace(DestinationRoomCode) ||
                string.IsNullOrWhiteSpace(DestinationRoomPassword))
            {
                return;
            }
            BingoBoardReplay.Instance.Reconnect();
        }
    }
}
