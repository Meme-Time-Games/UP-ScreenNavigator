using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenMapWindow : EditorWindow
    {
        private static readonly Color OpenColor = new Color(0.45f, 0.8f, 0.5f);
        private static readonly Color SelectedRowColor = new Color(0.24f, 0.49f, 0.9f, 0.25f);

        private const float ListWidth = 240f;

        private readonly ScreenGraphBuilder _graphBuilder = new ScreenGraphBuilder();
        private readonly ScreenGraphValidator _validator = new ScreenGraphValidator();
        private readonly EditorScreenNavigatorProvider _navigatorProvider = new EditorScreenNavigatorProvider();
        private readonly ScreenWarningsPanel _warningsPanel = new ScreenWarningsPanel();
        private readonly List<ScreenNode> _sortedNodes = new List<ScreenNode>();

        private OpenScreensProvider _openScreensProvider;
        private ScreenGraph _graph;
        private string _selectedScreenId;
        private string _searchText = "";
        private bool _showOnlyOpen;
        private Vector2 _listScrollPosition;
        private Vector2 _detailScrollPosition;

        [MenuItem("Tools/Screen Map")]
        public static void ShowWindow()
        {
            ScreenMapWindow window = GetWindow<ScreenMapWindow>();
            window.titleContent = new GUIContent("Screen Map");
            window.Show();
        }

        private void OnEnable()
        {
            _openScreensProvider = new OpenScreensProvider(_navigatorProvider);
            RebuildGraph();
            EditorApplication.projectChanged += RebuildGraph;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= RebuildGraph;
        }

        private void OnInspectorUpdate()
        {
            if (!Application.isPlaying)
                return;

            Repaint();
        }

        private void RebuildGraph()
        {
            _graph = _graphBuilder.Build();
            _validator.Validate(_graph);

            _sortedNodes.Clear();
            _sortedNodes.AddRange(_graph.Nodes);
            _sortedNodes.Sort(CompareByScreenId);

            Repaint();
        }

        private int CompareByScreenId(ScreenNode first, ScreenNode second)
        {
            return string.Compare(first.ScreenId, second.ScreenId, StringComparison.OrdinalIgnoreCase);
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (ReferenceEquals(_graph, null))
                return;

            ScreenMapContext context = CreateContext();

            _warningsPanel.Draw(context);

            EditorGUILayout.BeginHorizontal();
            DrawScreenList(context);
            DrawDetailPanel(context);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Screens", EditorStyles.boldLabel);
            _showOnlyOpen = GUILayout.Toggle(_showOnlyOpen, "Only open", EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Rebuild", EditorStyles.toolbarButton))
                RebuildGraph();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawScreenList(ScreenMapContext context)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(ListWidth));

            _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);

            _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);
            foreach (ScreenNode node in _sortedNodes)
            {
                DrawListRow(context, node);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawListRow(ScreenMapContext context, ScreenNode node)
        {
            if (!IsVisibleInList(context, node))
                return;

            Rect rowRect = EditorGUILayout.BeginHorizontal();

            if (node.ScreenId == _selectedScreenId)
                EditorGUI.DrawRect(rowRect, SelectedRowColor);

            DrawOpenDot(context, node);

            if (GUILayout.Button(GetListRowLabel(node), EditorStyles.label))
                SetSelectedScreen(node.ScreenId);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawOpenDot(ScreenMapContext context, ScreenNode node)
        {
            string dot = " ";
            if (context.IsScreenOpen(node.ScreenId))
                dot = "●";

            Color previousColor = GUI.color;
            GUI.color = OpenColor;
            GUILayout.Label(dot, GUILayout.Width(14f));
            GUI.color = previousColor;
        }

        private string GetListRowLabel(ScreenNode node)
        {
            string label = GetDisplayId(node.ScreenId);
            if (node.HasIssues())
                label = label + "  ⚠";

            return label;
        }

        private void DrawDetailPanel(ScreenMapContext context)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            ScreenNode node = GetSelectedNode();
            if (ReferenceEquals(node, null))
            {
                EditorGUILayout.LabelField("Select a screen to see its relationships.");
                EditorGUILayout.EndVertical();
                return;
            }

            _detailScrollPosition = EditorGUILayout.BeginScrollView(_detailScrollPosition);

            DrawDetailHeader(context, node);
            DrawRelationship("Opens →", node.NestedScreenIds);
            DrawRelationship("Closes →", node.ToCloseScreenIds);
            DrawRelationship("Opened by ←", GetOpenedBy(node.ScreenId));
            DrawRelationship("Closed by ←", GetClosedBy(node.ScreenId));
            DrawDetailIssues(node);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailHeader(ScreenMapContext context, ScreenNode node)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GetDetailTitle(node), EditorStyles.boldLabel);

            if (GUILayout.Button("Select asset", GUILayout.Width(90f)))
                context.PingScreen(node.AssetPath);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(node.AssetPath, EditorStyles.miniLabel);
            DrawStatus(context, node);
            EditorGUILayout.Space();
        }

        private string GetDetailTitle(ScreenNode node)
        {
            string title = GetDisplayId(node.ScreenId);
            if (node.IsLocked)
                title = title + "   [locked]";

            if (node.ClosesAllScreensOnOpen)
                title = title + "   [closeAllOnOpen]";

            return title;
        }

        private void DrawStatus(ScreenMapContext context, ScreenNode node)
        {
            string status = "Closed";
            Color color = Color.gray;
            if (context.IsScreenOpen(node.ScreenId))
            {
                status = "Open";
                color = OpenColor;
            }

            Color previousColor = GUI.color;
            GUI.color = color;
            EditorGUILayout.LabelField("Status: " + status);
            GUI.color = previousColor;
        }

        private void DrawRelationship(string label, IReadOnlyList<string> screenIds)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (screenIds.Count == 0)
            {
                EditorGUILayout.LabelField("   (none)");
                EditorGUILayout.Space();
                return;
            }

            foreach (string screenId in screenIds)
            {
                DrawRelationshipRow(screenId);
            }

            EditorGUILayout.Space();
        }

        private void DrawRelationshipRow(string screenId)
        {
            if (GUILayout.Button("   " + GetDisplayId(screenId), EditorStyles.label))
                SetSelectedScreen(screenId);
        }

        private void DrawDetailIssues(ScreenNode node)
        {
            if (!node.HasIssues())
                return;

            EditorGUILayout.LabelField("Issues", EditorStyles.boldLabel);
            foreach (ValidationIssue issue in node.Issues)
            {
                EditorGUILayout.HelpBox(issue.Message, GetMessageType(issue.Severity));
            }
        }

        private MessageType GetMessageType(ValidationSeverity severity)
        {
            if (severity == ValidationSeverity.Error)
                return MessageType.Error;

            return MessageType.Warning;
        }

        private ScreenNode GetSelectedNode()
        {
            if (string.IsNullOrEmpty(_selectedScreenId))
                return null;

            foreach (ScreenNode node in _graph.Nodes)
            {
                if (node.ScreenId == _selectedScreenId)
                    return node;
            }

            return null;
        }

        private void SetSelectedScreen(string screenId)
        {
            _selectedScreenId = screenId;
            GUI.FocusControl(null);
        }

        private List<string> GetOpenedBy(string screenId)
        {
            return GetSources(screenId, ScreenEdgeKind.Opens);
        }

        private List<string> GetClosedBy(string screenId)
        {
            return GetSources(screenId, ScreenEdgeKind.Closes);
        }

        private List<string> GetSources(string screenId, ScreenEdgeKind kind)
        {
            List<string> sources = new List<string>();
            foreach (ScreenEdge edge in _graph.Edges)
            {
                if (edge.Kind != kind)
                    continue;

                if (edge.ToScreenId != screenId)
                    continue;

                sources.Add(edge.FromScreenId);
            }

            return sources;
        }

        private bool IsVisibleInList(ScreenMapContext context, ScreenNode node)
        {
            if (_showOnlyOpen && !context.IsScreenOpen(node.ScreenId))
                return false;

            return IsMatchingSearch(node.ScreenId);
        }

        private bool IsMatchingSearch(string screenId)
        {
            if (string.IsNullOrEmpty(_searchText))
                return true;

            if (string.IsNullOrEmpty(screenId))
                return false;

            return screenId.ToLowerInvariant().Contains(_searchText.ToLowerInvariant());
        }

        private string GetDisplayId(string screenId)
        {
            if (string.IsNullOrEmpty(screenId))
                return "(empty id)";

            return screenId;
        }

        private ScreenMapContext CreateContext()
        {
            HashSet<string> openScreenIds = _openScreensProvider.GetOpenScreenIds();
            return new ScreenMapContext(_graph, openScreenIds);
        }
    }
}
