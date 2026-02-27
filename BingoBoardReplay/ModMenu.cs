using Modding.Menu;
using System.Collections;
using UnityEngine.UI;

namespace BingoBoardReplay
{
    internal class ModMenu
    {
        private static MenuScreen _ReplayMenuScreen;
        private static MenuButton _BackButton;
        public static MenuScreen CreateMenuScreen(MenuScreen parentMenu)
        {
            MenuBuilder replayMenuBuilder = MenuUtils.CreateMenuBuilderWithBackButton("BingoBoardReplay", parentMenu, out _BackButton);

            _ReplayMenuScreen = replayMenuBuilder.Build();

            _BackButton.submitAction += _ =>
            {
                BingoBoardReplay.Instance.SetUIVisible(false);
            };

            On.UIManager.ShowMenu += ShowMenuHook;

            return _ReplayMenuScreen;
        }

        private static IEnumerator ShowMenuHook(On.UIManager.orig_ShowMenu orig, UIManager self, MenuScreen menu)
        {
            if (menu == _ReplayMenuScreen)
            {
                BingoSync.BingoSync.HideMenu();
                BingoBoardReplay.Instance.SetUIVisible(true);
            }
            yield return orig(self, menu);
        }
    }
}


