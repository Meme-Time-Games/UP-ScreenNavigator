using System.Collections.Generic;

namespace ScreenNavigators.Editors
{
    public class ScreenGraph
    {
        private readonly List<ScreenNode> _nodes;
        private readonly List<ScreenEdge> _edges;

        public IReadOnlyList<ScreenNode> Nodes => _nodes;
        public IReadOnlyList<ScreenEdge> Edges => _edges;

        public ScreenGraph(List<ScreenNode> nodes, List<ScreenEdge> edges)
        {
            _nodes = nodes;
            _edges = edges;
        }

        public bool HasNodeWithId(string screenId)
        {
            foreach (ScreenNode node in _nodes)
            {
                if (node.ScreenId == screenId)
                    return true;
            }

            return false;
        }

        public int CountNodesWithId(string screenId)
        {
            int count = 0;
            foreach (ScreenNode node in _nodes)
            {
                if (node.ScreenId == screenId)
                    count++;
            }

            return count;
        }
    }
}
