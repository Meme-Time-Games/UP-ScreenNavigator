using System.Collections.Generic;

namespace ScreenNavigators.Editors
{
    public class ScreenNode
    {
        private readonly string _screenId;
        private readonly string _assetPath;
        private readonly bool _isLocked;
        private readonly bool _closesAllScreensOnOpen;
        private readonly List<string> _nestedScreenIds;
        private readonly List<string> _toCloseScreenIds;
        private readonly int _emptyReferenceCount;
        private readonly List<ValidationIssue> _issues;

        private bool _isOpen;

        public string ScreenId => _screenId;
        public string AssetPath => _assetPath;
        public bool IsLocked => _isLocked;
        public bool ClosesAllScreensOnOpen => _closesAllScreensOnOpen;
        public IReadOnlyList<string> NestedScreenIds => _nestedScreenIds;
        public IReadOnlyList<string> ToCloseScreenIds => _toCloseScreenIds;
        public int EmptyReferenceCount => _emptyReferenceCount;
        public IReadOnlyList<ValidationIssue> Issues => _issues;
        public bool IsOpen => _isOpen;

        public ScreenNode(string screenId, string assetPath, bool isLocked, bool closesAllScreensOnOpen,
            List<string> nestedScreenIds, List<string> toCloseScreenIds, int emptyReferenceCount)
        {
            _screenId = screenId;
            _assetPath = assetPath;
            _isLocked = isLocked;
            _closesAllScreensOnOpen = closesAllScreensOnOpen;
            _nestedScreenIds = nestedScreenIds;
            _toCloseScreenIds = toCloseScreenIds;
            _emptyReferenceCount = emptyReferenceCount;
            _issues = new List<ValidationIssue>();
            _isOpen = false;
        }

        public void AddIssue(ValidationIssue issue)
        {
            _issues.Add(issue);
        }

        public bool HasIssues()
        {
            return _issues.Count > 0;
        }

        public void MarkOpen()
        {
            _isOpen = true;
        }

        public void MarkClosed()
        {
            _isOpen = false;
        }
    }
}
