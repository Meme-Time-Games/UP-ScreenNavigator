using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScreenNavigators.Editors
{
    public class ScreenGraphElement : GraphView
    {
        public ScreenGraphElement()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        }

        public void Rebuild(ScreenMapContext context, Color opensColor, Color closesColor)
        {
            DeleteElements(new List<GraphElement>(graphElements));

            Dictionary<string, Node> nodesById = new Dictionary<string, Node>();
            int index = 0;
            foreach (ScreenNode screenNode in context.Graph.Nodes)
            {
                Node node = CreateNode(screenNode, index);
                AddElement(node);
                if (!nodesById.ContainsKey(screenNode.ScreenId))
                    nodesById.Add(screenNode.ScreenId, node);

                index++;
            }

            foreach (ScreenEdge screenEdge in context.Graph.Edges)
            {
                AddEdge(nodesById, screenEdge, opensColor, closesColor);
            }
        }

        private Node CreateNode(ScreenNode screenNode, int index)
        {
            Node node = new Node();
            node.title = screenNode.ScreenId;
            node.SetPosition(new Rect(40f + (index % 5) * 200f, 60f + (index / 5) * 140f, 160f, 100f));

            Port inputPort = node.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "in";
            node.inputContainer.Add(inputPort);

            Port outputPort = node.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "out";
            node.outputContainer.Add(outputPort);

            node.RefreshPorts();
            node.RefreshExpandedState();
            return node;
        }

        private void AddEdge(Dictionary<string, Node> nodesById, ScreenEdge screenEdge, Color opensColor, Color closesColor)
        {
            if (!nodesById.ContainsKey(screenEdge.FromScreenId))
                return;

            if (!nodesById.ContainsKey(screenEdge.ToScreenId))
                return;

            Node fromNode = nodesById[screenEdge.FromScreenId];
            Node toNode = nodesById[screenEdge.ToScreenId];

            Port outputPort = fromNode.outputContainer[0] as Port;
            Port inputPort = toNode.inputContainer[0] as Port;

            Edge edge = outputPort.ConnectTo(inputPort);
            edge.edgeControl.inputColor = GetEdgeColor(screenEdge.Kind, opensColor, closesColor);
            edge.edgeControl.outputColor = GetEdgeColor(screenEdge.Kind, opensColor, closesColor);
            AddElement(edge);
        }

        private Color GetEdgeColor(ScreenEdgeKind kind, Color opensColor, Color closesColor)
        {
            if (kind == ScreenEdgeKind.Closes)
                return closesColor;

            return opensColor;
        }
    }
}
