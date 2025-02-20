using System;
using UnityEditor;

namespace ScreenNavigators.Editors
{
    public class OpenedScreenMonitor : EditorWindow
    {
        [MenuItem("Tools/OpenedScreenMonitor")]
        public static void GetWindow()
        {
            EditorWindow.GetWindow(typeof(OpenedScreenMonitor));
        }

        private void OnEnable()
        {
            
        }

        private void OnGUI()
        {
            
        }
    }
}