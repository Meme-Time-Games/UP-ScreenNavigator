namespace ScreenNavigators.Core
{
    public class ScreenData
    {
        private readonly string _screenId;
        private readonly string[] _nestedScreenIds;
        private readonly string[] _closeScreenIds;
        private readonly bool _isLocked;
        private readonly bool _hasToCloseAllScreensOnOpen;
        
        public string ScreenId => _screenId;
        public string[] NestedScreenIds => _nestedScreenIds;
        public string[] CloseScreenIds => _closeScreenIds;
        public bool IsLocked => _isLocked;
        public bool HasToCloseAllScreensOnOpen => _hasToCloseAllScreensOnOpen;

        public ScreenData(string screenId, string[] nestedScreenIds, string[] closeScreenIds, bool isLocked, bool hasToCloseAllScreensOnOpen)
        {
            _screenId = screenId;
            _nestedScreenIds = nestedScreenIds;
            _closeScreenIds = closeScreenIds;
            _isLocked = isLocked;
            _hasToCloseAllScreensOnOpen = hasToCloseAllScreensOnOpen;
        }
    }
}