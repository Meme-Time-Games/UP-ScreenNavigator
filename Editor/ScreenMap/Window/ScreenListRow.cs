namespace ScreenNavigators.Editors
{
    public class ScreenListRow
    {
        private readonly ScreenNode _node;
        private readonly int _depth;

        public ScreenNode Node => _node;
        public int Depth => _depth;

        public ScreenListRow(ScreenNode node, int depth)
        {
            _node = node;
            _depth = depth;
        }
    }
}
