namespace ScreenNavigators.Editors
{
    public class ScreenEdge
    {
        private readonly string _fromScreenId;
        private readonly string _toScreenId;
        private readonly ScreenEdgeKind _kind;

        public string FromScreenId => _fromScreenId;
        public string ToScreenId => _toScreenId;
        public ScreenEdgeKind Kind => _kind;

        public ScreenEdge(string fromScreenId, string toScreenId, ScreenEdgeKind kind)
        {
            _fromScreenId = fromScreenId;
            _toScreenId = toScreenId;
            _kind = kind;
        }
    }
}
