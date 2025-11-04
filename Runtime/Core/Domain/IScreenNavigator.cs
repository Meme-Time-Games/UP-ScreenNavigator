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
        bool IsScreenOpen(string characterCustomizationScreenName);

#if UNITY_EDITOR
        string[] GetOpenScreenIds();
#endif
    }
}