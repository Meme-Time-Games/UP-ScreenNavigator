using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenTreeTabView : IScreenMapTab
    {
        private static readonly Color OpenColor = new Color(0.45f, 0.8f, 0.5f);

        private Vector2 _scrollPosition;

        public string Title => "Tree";

        public void Draw(ScreenMapContext context)
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (ScreenNode node in context.Graph.Nodes)
            {
                DrawNode(context, node);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawNode(ScreenMapContext context, ScreenNode node)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawNodeHeader(context, node);
            DrawReferences("opens", node.NestedScreenIds);
            DrawReferences("closes", node.ToCloseScreenIds);

            EditorGUILayout.EndVertical();
        }

        private void DrawNodeHeader(ScreenMapContext context, ScreenNode node)
        {
            EditorGUILayout.BeginHorizontal();

            Color previousColor = GUI.color;
            if (context.IsScreenOpen(node.ScreenId))
                GUI.color = OpenColor;

            EditorGUILayout.LabelField(GetNodeTitle(node), EditorStyles.boldLabel);
            GUI.color = previousColor;

            if (GUILayout.Button("Select", GUILayout.Width(60f)))
                context.PingScreen(node.AssetPath);

            EditorGUILayout.EndHorizontal();

            if (node.HasIssues())
                EditorGUILayout.LabelField("  ⚠ " + node.Issues.Count + " issue(s)");
        }

        private string GetNodeTitle(ScreenNode node)
        {
            string title = node.ScreenId;
            if (node.IsLocked)
                title = title + "  [locked]";

            if (node.ClosesAllScreensOnOpen)
                title = title + "  [closeAllOnOpen]";

            return title;
        }

        private void DrawReferences(string label, System.Collections.Generic.IReadOnlyList<string> screenIds)
        {
            if (screenIds.Count == 0)
                return;

            EditorGUILayout.LabelField("   " + label + " → " + string.Join(", ", screenIds));
        }
    }
}
