using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenWarningsPanel
    {
        private const float MaxPanelHeight = 140f;

        private bool _isExpanded = true;
        private Vector2 _scrollPosition;

        public void Draw(ScreenMapContext context)
        {
            int issueCount = GetIssueCount(context.Graph);
            if (issueCount == 0)
                return;

            _isExpanded = EditorGUILayout.Foldout(_isExpanded, "Issues (" + issueCount + ")", true);
            if (!_isExpanded)
                return;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(MaxPanelHeight));

            foreach (ScreenNode node in context.Graph.Nodes)
            {
                DrawNodeIssues(context, node);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawNodeIssues(ScreenMapContext context, ScreenNode node)
        {
            foreach (ValidationIssue issue in node.Issues)
            {
                DrawIssue(context, node, issue);
            }
        }

        private void DrawIssue(ScreenMapContext context, ScreenNode node, ValidationIssue issue)
        {
            EditorGUILayout.BeginHorizontal();

            MessageType messageType = GetMessageType(issue.Severity);
            EditorGUILayout.HelpBox("[" + issue.ScreenId + "] " + issue.Message, messageType);

            if (GUILayout.Button("Select", GUILayout.Width(60f), GUILayout.Height(38f)))
                context.PingScreen(node.AssetPath);

            EditorGUILayout.EndHorizontal();
        }

        private int GetIssueCount(ScreenGraph graph)
        {
            int count = 0;
            foreach (ScreenNode node in graph.Nodes)
            {
                count += node.Issues.Count;
            }

            return count;
        }

        private MessageType GetMessageType(ValidationSeverity severity)
        {
            if (severity == ValidationSeverity.Error)
                return MessageType.Error;

            return MessageType.Warning;
        }
    }
}
