using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenMonitorTabView : IScreenMapTab
    {
        public string Title => "Monitor";

        public void Draw(ScreenMapContext context)
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see open screens.", MessageType.Info);
                return;
            }

            bool anyOpen = false;
            foreach (ScreenNode node in context.Graph.Nodes)
            {
                if (!context.IsScreenOpen(node.ScreenId))
                    continue;

                anyOpen = true;
                EditorGUILayout.LabelField(node.ScreenId);
            }

            if (!anyOpen)
                EditorGUILayout.LabelField("No screens open.");
        }
    }
}
