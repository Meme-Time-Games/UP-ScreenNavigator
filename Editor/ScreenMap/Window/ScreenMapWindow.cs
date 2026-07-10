using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenMapWindow : EditorWindow
    {
        private readonly ScreenGraphBuilder _graphBuilder = new ScreenGraphBuilder();
        private readonly ScreenGraphValidator _validator = new ScreenGraphValidator();
        private readonly EditorScreenNavigatorProvider _navigatorProvider = new EditorScreenNavigatorProvider();
        private readonly ScreenWarningsPanel _warningsPanel = new ScreenWarningsPanel();
        private readonly List<IScreenMapTab> _tabs = new List<IScreenMapTab>();

        private OpenScreensProvider _openScreensProvider;
        private ScreenGraph _graph;
        private int _activeTabIndex;
        private ScreenGraphViewTabView _graphViewTab;
        private bool _graphViewNeedsRebuild;

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

            _tabs.Clear();
            _tabs.Add(new ScreenTreeTabView());
            _tabs.Add(new ScreenBezierTabView());
            _tabs.Add(new ScreenMonitorTabView());

            _graphViewTab = new ScreenGraphViewTabView();
            _tabs.Add(_graphViewTab);
            rootVisualElement.Add(_graphViewTab.RootElement);

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
            _graphViewNeedsRebuild = true;
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (ReferenceEquals(_graph, null))
                return;

            ScreenMapContext context = CreateContext();
            SyncGraphViewTab(context);

            if (IsGraphViewActive())
                return;

            _warningsPanel.Draw(context);
            _tabs[_activeTabIndex].Draw(context);
        }

        private bool IsGraphViewActive()
        {
            return _tabs[_activeTabIndex] == _graphViewTab;
        }

        private void SyncGraphViewTab(ScreenMapContext context)
        {
            if (ReferenceEquals(_graphViewTab, null))
                return;

            if (!IsGraphViewActive())
            {
                _graphViewTab.SetHidden();
                return;
            }

            _graphViewTab.SetVisible();

            if (!_graphViewNeedsRebuild)
                return;

            _graphViewTab.Rebuild(context);
            _graphViewNeedsRebuild = false;
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            for (int i = 0; i < _tabs.Count; i++)
            {
                DrawTabButton(i);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Rebuild", EditorStyles.toolbarButton))
                RebuildGraph();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabButton(int tabIndex)
        {
            bool isActive = _activeTabIndex == tabIndex;
            bool clicked = GUILayout.Toggle(isActive, _tabs[tabIndex].Title, EditorStyles.toolbarButton);
            if (!clicked)
                return;

            if (_activeTabIndex == tabIndex)
                return;

            _activeTabIndex = tabIndex;
            _graphViewNeedsRebuild = true;
        }

        private ScreenMapContext CreateContext()
        {
            HashSet<string> openScreenIds = _openScreensProvider.GetOpenScreenIds();
            return new ScreenMapContext(_graph, openScreenIds);
        }
    }
}
