using DependencyInjector.Core;
using ScreenNavigators.Core;
using ServiceLocatorPattern;
using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    [CustomEditor(typeof(ScreenDataSO))]
    public class ScreenDataSOEditor : Editor
    {
        private static readonly Color OpenColor = new Color(0.4f, 0.75f, 0.45f);
        private static readonly Color CloseColor = new Color(0.85f, 0.45f, 0.45f);
        private static readonly Color OpenStatusColor = new Color(0.45f, 0.8f, 0.5f);
        private static readonly Color ClosedStatusColor = new Color(0.65f, 0.65f, 0.65f);
        private static readonly Color SeparatorColor = new Color(0.3f, 0.3f, 0.3f);

        private IScreenNavigator _screenNavigator;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawControlsHeader();

            _screenNavigator = GetScreenNavigator();

            DrawScreenControls();
            DrawMonitorButton();
        }

        private void DrawScreenControls()
        {
            if (ReferenceEquals(_screenNavigator, null))
            {
                DrawDisabledScreenControls();
                return;
            }

            ScreenDataSO screenData = (ScreenDataSO)target;
            bool isScreenOpen = _screenNavigator.IsScreenOpen(screenData.ScreenId);

            DrawStatus(screenData);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(isScreenOpen);
            if (HasClickedColoredButton("Open Screen", OpenColor))
                _screenNavigator.OpenScreen(screenData.ScreenId);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!isScreenOpen);
            if (HasClickedColoredButton("Close Screen", CloseColor))
                _screenNavigator.CloseScreen(screenData.ScreenId);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawDisabledScreenControls()
        {
            EditorGUILayout.HelpBox("Enter Play Mode to open or close this screen.", MessageType.Info);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();
            HasClickedColoredButton("Open Screen", OpenColor);
            HasClickedColoredButton("Close Screen", CloseColor);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawStatus(ScreenDataSO screenData)
        {
            bool isScreenOpen = _screenNavigator.IsScreenOpen(screenData.ScreenId);

            GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel);
            statusStyle.normal.textColor = ClosedStatusColor;
            string statusText = "Closed";

            if (isScreenOpen)
            {
                statusStyle.normal.textColor = OpenStatusColor;
                statusText = "Open";
            }

            EditorGUILayout.LabelField("Status", statusText, statusStyle);
        }

        private void DrawControlsHeader()
        {
            EditorGUILayout.Space();
            DrawSeparator();
            EditorGUILayout.LabelField("Screen Controls", EditorStyles.boldLabel);
        }

        private void DrawMonitorButton()
        {
            EditorGUILayout.Space();

            if (GUILayout.Button("Open Screen Monitor"))
                OpenedScreenMonitor.GetWindow();
        }

        private void DrawSeparator()
        {
            Rect separatorRect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(separatorRect, SeparatorColor);
        }

        private bool HasClickedColoredButton(string label, Color backgroundColor)
        {
            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;

            bool isClicked = GUILayout.Button(label, GUILayout.Height(26f));

            GUI.backgroundColor = previousColor;
            return isClicked;
        }

        private IScreenNavigator GetScreenNavigator()
        {
            if (!ServiceLocatorInstance.Instance.IsContained<IDIContainer>())
                return null;

            IDIContainer container = ServiceLocatorInstance.Instance.Get<IDIContainer>();

            if (!container.IsTypeContained(typeof(IScreenNavigator)))
                return null;

            return container.Get<IScreenNavigator>();
        }
    }
}
