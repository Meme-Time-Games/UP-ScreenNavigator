using System;

namespace ScreenNavigators.Core
{
    public interface IScreenNavigator
    {
        Action<string> OnScreenOpened { get; set; }
        Action<string> OnScreenClosed { get; set; }
        void AddScreen(ScreenData screenData);
        void OpenScreen(string screenId);
        void CloseScreen(string screenId);
        void CloseAllScreens();
        bool IsScreenOpen(string characterCustomizationScreenName);
    }
}