using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ScreenNavigators.Editors
{
    public class ScreenMapContext
    {
        private readonly ScreenGraph _graph;
        private readonly HashSet<string> _openScreenIds;

        public ScreenGraph Graph => _graph;

        public ScreenMapContext(ScreenGraph graph, HashSet<string> openScreenIds)
        {
            _graph = graph;
            _openScreenIds = openScreenIds;
        }

        public bool IsScreenOpen(string screenId)
        {
            return _openScreenIds.Contains(screenId);
        }

        public void PingScreen(string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (ReferenceEquals(asset, null))
                return;

            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }
    }
}
