namespace ScreenNavigators.Editors
{
    public class ScreenListRow
    {
        private readonly ScreenNode _node;
        private readonly int _depth;
        private readonly string _groupTitle;

        public ScreenNode Node => _node;
        public int Depth => _depth;
        public string GroupTitle => _groupTitle;

        private ScreenListRow(ScreenNode node, int depth, string groupTitle)
        {
            _node = node;
            _depth = depth;
            _groupTitle = groupTitle;
        }

        public static ScreenListRow CreateScreenRow(ScreenNode node, int depth)
        {
            return new ScreenListRow(node, depth, null);
        }

        public static ScreenListRow CreateGroupHeader(string groupTitle)
        {
            return new ScreenListRow(null, 0, groupTitle);
        }

        public bool IsGroupHeader()
        {
            return !string.IsNullOrEmpty(_groupTitle);
        }
    }
}
