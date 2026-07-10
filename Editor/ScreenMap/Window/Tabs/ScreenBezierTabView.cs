using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenBezierTabView : IScreenMapTab
    {
        private static readonly Color OpensColor = new Color(0.4f, 0.75f, 0.45f);
        private static readonly Color ClosesColor = new Color(0.85f, 0.45f, 0.45f);
        private static readonly Color OpenNodeColor = new Color(0.45f, 0.8f, 0.5f);
        private static readonly Vector2 NodeSize = new Vector2(140f, 48f);

        private readonly Dictionary<string, Rect> _nodeRects = new Dictionary<string, Rect>();

        private string _draggedScreenId;

        public string Title => "Graph (IMGUI)";

        public void Draw(ScreenMapContext context)
        {
            EnsureLayout(context.Graph);

            Rect canvas = GUILayoutUtility.GetRect(0f, 100000f, 0f, 100000f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUI.Box(canvas, GUIContent.none);

            DrawEdges(context.Graph);
            DrawNodes(context);
            HandleDrag(context.Graph);
        }

        private void EnsureLayout(ScreenGraph graph)
        {
            int index = 0;
            foreach (ScreenNode node in graph.Nodes)
            {
                if (_nodeRects.ContainsKey(node.ScreenId))
                {
                    index++;
                    continue;
                }

                int column = index % 4;
                int row = index / 4;
                Vector2 origin = new Vector2(40f + column * 200f, 60f + row * 110f);
                _nodeRects.Add(node.ScreenId, new Rect(origin, NodeSize));
                index++;
            }
        }

        private void DrawEdges(ScreenGraph graph)
        {
            foreach (ScreenEdge edge in graph.Edges)
            {
                if (!_nodeRects.ContainsKey(edge.FromScreenId))
                    continue;

                if (!_nodeRects.ContainsKey(edge.ToScreenId))
                    continue;

                DrawEdge(_nodeRects[edge.FromScreenId], _nodeRects[edge.ToScreenId], GetEdgeColor(edge.Kind));
            }
        }

        private void DrawEdge(Rect fromRect, Rect toRect, Color color)
        {
            Vector3 start = new Vector3(fromRect.center.x, fromRect.yMax, 0f);
            Vector3 end = new Vector3(toRect.center.x, toRect.yMin, 0f);
            Vector3 startTangent = start + Vector3.up * 40f;
            Vector3 endTangent = end + Vector3.down * 40f;
            Handles.DrawBezier(start, end, startTangent, endTangent, color, null, 3f);
        }

        private Color GetEdgeColor(ScreenEdgeKind kind)
        {
            if (kind == ScreenEdgeKind.Closes)
                return ClosesColor;

            return OpensColor;
        }

        private void DrawNodes(ScreenMapContext context)
        {
            foreach (ScreenNode node in context.Graph.Nodes)
            {
                DrawNode(context, node);
            }
        }

        private void DrawNode(ScreenMapContext context, ScreenNode node)
        {
            Rect rect = _nodeRects[node.ScreenId];

            Color previousColor = GUI.backgroundColor;
            if (context.IsScreenOpen(node.ScreenId))
                GUI.backgroundColor = OpenNodeColor;

            GUI.Box(rect, GetNodeLabel(node), EditorStyles.helpBox);
            GUI.backgroundColor = previousColor;

            if (GUI.Button(new Rect(rect.x, rect.yMax - 18f, rect.width, 16f), "Select"))
                context.PingScreen(node.AssetPath);
        }

        private string GetNodeLabel(ScreenNode node)
        {
            if (node.HasIssues())
                return node.ScreenId + "  ⚠";

            return node.ScreenId;
        }

        private void HandleDrag(ScreenGraph graph)
        {
            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.MouseDown)
                BeginDrag(graph, currentEvent.mousePosition);

            if (currentEvent.type == EventType.MouseUp)
                _draggedScreenId = null;

            if (currentEvent.type != EventType.MouseDrag)
                return;

            if (string.IsNullOrEmpty(_draggedScreenId))
                return;

            Rect rect = _nodeRects[_draggedScreenId];
            rect.position += currentEvent.delta;
            _nodeRects[_draggedScreenId] = rect;
            currentEvent.Use();
        }

        private void BeginDrag(ScreenGraph graph, Vector2 mousePosition)
        {
            foreach (ScreenNode node in graph.Nodes)
            {
                if (!_nodeRects[node.ScreenId].Contains(mousePosition))
                    continue;

                _draggedScreenId = node.ScreenId;
                return;
            }
        }
    }
}
