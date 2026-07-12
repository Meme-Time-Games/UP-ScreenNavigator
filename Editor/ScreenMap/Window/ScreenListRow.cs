namespace ScreenNavigators.Editors
{
    public class ScreenListRow
    {
        private readonly ScreenNode _node;
        private readonly int _depth;
        private readonly string _groupName;

        public ScreenNode Node => _node;
        public int Depth => _depth;
        public string GroupName => _groupName;

        private ScreenListRow(ScreenNode node, int depth, string groupName)
        {
            _node = node;
            _depth = depth;
            _groupName = groupName;
        }

        public static ScreenListRow CreateScreenRow(ScreenNode node, int depth)
        {
            return new ScreenListRow(node, depth, null);
        }

        public static ScreenListRow CreateGroupHeader(string groupName)
        {
            return new ScreenListRow(null, 0, groupName);
        }

        public bool IsGroupHeader()
        {
            return !string.IsNullOrEmpty(_groupName);
        }
    }
}
