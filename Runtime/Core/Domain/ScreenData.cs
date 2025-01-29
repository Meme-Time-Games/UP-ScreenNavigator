namespace ScreenNavigators.Core
{
    public class ScreenData
    {
        private readonly string _screenId;
        private readonly ScreenData[] _nestedScreens;
        private readonly ScreenData[] _closeScreens;
        private readonly bool _isLocked;
        private readonly bool _hasToCloseAllScreensOnOpen;
        
        public string ScreenId => _screenId;
        public ScreenData[] NestedScreens => _nestedScreens;
        public ScreenData[] CloseScreens => _closeScreens;
        public bool IsLocked => _isLocked;
        public bool HasToCloseAllScreensOnOpen => _hasToCloseAllScreensOnOpen;

        protected ScreenData(string screenId, ScreenData[] nestedScreens, ScreenData[] closeScreens, bool isLocked, bool hasToCloseAllScreensOnOpen)
        {
            _screenId = screenId;
            _nestedScreens = nestedScreens;
            _closeScreens = closeScreens;
            _isLocked = isLocked;
            _hasToCloseAllScreensOnOpen = hasToCloseAllScreensOnOpen;
        }
    }
}