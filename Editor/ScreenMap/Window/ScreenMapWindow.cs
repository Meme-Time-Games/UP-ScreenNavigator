using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScreenNavigators.Editors
{
    public class ScreenMapWindow : EditorWindow
    {
        private const string StyleSheetPath =
            "Packages/com.custom.screennavigator/Editor/ScreenMap/Window/ScreenMapStyles.uss";

        private const float NarrowWidthThreshold = 460f;
        private const float ListPaneWidth = 240f;
        private const float ListPaneHeight = 220f;
        private const long OpenStateRefreshMilliseconds = 300;

        private readonly ScreenGraphBuilder _graphBuilder = new ScreenGraphBuilder();
        private readonly ScreenGraphValidator _validator = new ScreenGraphValidator();
        private readonly EditorScreenNavigatorProvider _navigatorProvider = new EditorScreenNavigatorProvider();

        private OpenScreensProvider _openScreensProvider;
        private ScreenListPanel _listPanel;
        private ScreenDetailPanel _detailPanel;
        private VisualElement _body;
        private ToolbarSearchField _searchField;
        private ToolbarToggle _onlyOpenToggle;
        private ToolbarToggle _onlyIssuesToggle;
        private ToolbarButton _rebuildButton;
        private Label _issueCountLabel;

        private ScreenGraph _graph;
        private HashSet<string> _openScreenIds = new HashSet<string>();
        private string _selectedScreenId;
        private bool _isNarrowLayout;
        private bool _hasResolvedLayout;

        [MenuItem("Tools/Screen Map")]
        public static void ShowWindow()
        {
            ScreenMapWindow window = GetWindow<ScreenMapWindow>();
            window.titleContent = new GUIContent("Screen Map");
            window.minSize = new Vector2(200f, 240f);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.projectChanged += RebuildGraph;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= RebuildGraph;
        }

        public void CreateGUI()
        {
            _openScreensProvider = new OpenScreensProvider(_navigatorProvider);

            rootVisualElement.AddToClassList("screen-map");
            ApplyTheme();
            ApplyStyleSheet();

            rootVisualElement.Add(CreateToolbar());
            rootVisualElement.Add(CreateSearchRow());

            _listPanel = new ScreenListPanel();
            _listPanel.OnScreenSelected = HandleScreenSelected;

            _detailPanel = new ScreenDetailPanel();
            _detailPanel.OnScreenRequested = HandleScreenRequested;

            _body = new VisualElement();
            _body.AddToClassList("sm-body");
            rootVisualElement.Add(_body);

            BuildSplitView();
            RebuildGraph();

            rootVisualElement.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            rootVisualElement.schedule.Execute(RefreshOpenState).Every(OpenStateRefreshMilliseconds);
        }

        /* ---------- responsive layout ---------- */

        private void HandleGeometryChanged(GeometryChangedEvent changedEvent)
        {
            bool isNarrow = changedEvent.newRect.width < NarrowWidthThreshold;

            if (_hasResolvedLayout && isNarrow == _isNarrowLayout)
                return;

            _isNarrowLayout = isNarrow;
            _hasResolvedLayout = true;

            ApplyLayoutMode();
            BuildSplitView();
        }

        private void ApplyLayoutMode()
        {
            if (_isNarrowLayout)
            {
                ApplyCompactMode();
                return;
            }

            ApplyWideMode();
        }

        private void ApplyCompactMode()
        {
            rootVisualElement.AddToClassList("compact");

            _onlyOpenToggle.text = "Open";
            _onlyIssuesToggle.text = "!";
            _rebuildButton.text = "⟳";

            _listPanel.SetCompactBadges();
        }

        private void ApplyWideMode()
        {
            rootVisualElement.RemoveFromClassList("compact");

            _onlyOpenToggle.text = "Only open";
            _onlyIssuesToggle.text = "Issues";
            _rebuildButton.text = "Rebuild";

            _listPanel.SetWideBadges();
        }

        private void BuildSplitView()
        {
            _body.Clear();

            TwoPaneSplitView splitView = CreateSplitView();
            splitView.Add(_listPanel);
            splitView.Add(_detailPanel);

            _body.Add(splitView);
        }

        private TwoPaneSplitView CreateSplitView()
        {
            if (_isNarrowLayout)
                return new TwoPaneSplitView(0, ListPaneHeight, TwoPaneSplitViewOrientation.Vertical);

            return new TwoPaneSplitView(0, ListPaneWidth, TwoPaneSplitViewOrientation.Horizontal);
        }

        /* ---------- chrome ---------- */

        private void ApplyTheme()
        {
            if (EditorGUIUtility.isProSkin)
            {
                rootVisualElement.AddToClassList("theme-dark");
                return;
            }

            rootVisualElement.AddToClassList("theme-light");
        }

        private void ApplyStyleSheet()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (ReferenceEquals(styleSheet, null))
                return;

            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private VisualElement CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();

            _onlyOpenToggle = new ToolbarToggle();
            _onlyOpenToggle.text = "Only open";
            _onlyOpenToggle.tooltip = "Show only screens that are currently open in Play Mode";
            _onlyOpenToggle.RegisterValueChangedCallback(changedEvent => ApplyFilter());
            toolbar.Add(_onlyOpenToggle);

            _onlyIssuesToggle = new ToolbarToggle();
            _onlyIssuesToggle.text = "Issues";
            _onlyIssuesToggle.tooltip = "Show only screens with validation issues";
            _onlyIssuesToggle.RegisterValueChangedCallback(changedEvent => ApplyFilter());
            toolbar.Add(_onlyIssuesToggle);

            ToolbarSpacer spacer = new ToolbarSpacer();
            spacer.style.flexGrow = 1f;
            toolbar.Add(spacer);

            _issueCountLabel = new Label();
            _issueCountLabel.AddToClassList("sm-toolbar__count");
            toolbar.Add(_issueCountLabel);

            _rebuildButton = new ToolbarButton(RebuildGraph);
            _rebuildButton.text = "Rebuild";
            _rebuildButton.tooltip = "Rescan the project for ScreenDataSO assets";
            toolbar.Add(_rebuildButton);

            return toolbar;
        }

        private VisualElement CreateSearchRow()
        {
            VisualElement searchRow = new VisualElement();
            searchRow.AddToClassList("sm-search-row");

            _searchField = new ToolbarSearchField();
            _searchField.AddToClassList("sm-search");
            _searchField.RegisterValueChangedCallback(changedEvent => ApplyFilter());
            searchRow.Add(_searchField);

            return searchRow;
        }

        /* ---------- data ---------- */

        private void RebuildGraph()
        {
            _graph = _graphBuilder.Build();
            _validator.Validate(_graph);

            if (ReferenceEquals(_listPanel, null))
                return;

            _openScreenIds = _openScreensProvider.GetOpenScreenIds();
            UpdateContext();
            ApplyFilter();
            UpdateIssueCount();
            RestoreSelection();
        }

        private void UpdateContext()
        {
            ScreenMapContext context = new ScreenMapContext(_graph, _openScreenIds);
            _listPanel.SetContext(context);
            _detailPanel.SetContext(context);
        }

        private void ApplyFilter()
        {
            if (ReferenceEquals(_graph, null))
                return;

            ScreenListFilter filter = new ScreenListFilter(
                _searchField.value,
                _onlyOpenToggle.value,
                _onlyIssuesToggle.value);

            _listPanel.ApplyFilter(filter);
        }

        private void UpdateIssueCount()
        {
            int issueCount = GetIssueCount();

            _issueCountLabel.RemoveFromClassList("sm-toolbar__count--alert");
            _issueCountLabel.text = "";

            if (issueCount == 0)
                return;

            _issueCountLabel.text = issueCount.ToString();
            _issueCountLabel.tooltip = issueCount + " validation issue(s)";
            _issueCountLabel.AddToClassList("sm-toolbar__count--alert");
        }

        private int GetIssueCount()
        {
            int count = 0;
            foreach (ScreenNode node in _graph.Nodes)
            {
                count += node.Issues.Count;
            }

            return count;
        }

        private void RefreshOpenState()
        {
            if (ReferenceEquals(_graph, null))
                return;

            HashSet<string> openScreenIds = _openScreensProvider.GetOpenScreenIds();
            if (openScreenIds.SetEquals(_openScreenIds))
                return;

            _openScreenIds = openScreenIds;
            UpdateContext();
            ApplyFilter();
            RefreshDetail();
        }

        private void RefreshDetail()
        {
            ScreenNode node = GetNodeById(_selectedScreenId);
            if (ReferenceEquals(node, null))
                return;

            _detailPanel.ShowScreen(node);
        }

        private void RestoreSelection()
        {
            ScreenNode node = GetNodeById(_selectedScreenId);
            if (ReferenceEquals(node, null))
            {
                _detailPanel.ShowEmptyState();
                return;
            }

            _listPanel.SelectScreen(_selectedScreenId);
            _detailPanel.ShowScreen(node);
        }

        /* ---------- interaction ---------- */

        private void HandleScreenSelected(ScreenNode node)
        {
            if (ReferenceEquals(node, null))
                return;

            _selectedScreenId = node.ScreenId;
            _detailPanel.ShowScreen(node);
        }

        private void HandleScreenRequested(string screenId)
        {
            ScreenNode node = GetNodeById(screenId);
            if (ReferenceEquals(node, null))
                return;

            _selectedScreenId = screenId;
            _listPanel.SelectScreen(screenId);
            _detailPanel.ShowScreen(node);
        }

        private ScreenNode GetNodeById(string screenId)
        {
            if (string.IsNullOrEmpty(screenId))
                return null;

            foreach (ScreenNode node in _graph.Nodes)
            {
                if (node.ScreenId == screenId)
                    return node;
            }

            return null;
        }
    }
}
