using System.Collections.Generic;
using ScreenNavigators.Core;
using UnityEditor;

namespace ScreenNavigators.Editors
{
    public class ScreenGraphBuilder
    {
        public ScreenGraph Build()
        {
            List<ScreenNode> nodes = new List<ScreenNode>();
            List<ScreenEdge> edges = new List<ScreenEdge>();

            string[] guids = AssetDatabase.FindAssets("t:ScreenDataSO");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ScreenDataSO screenData = AssetDatabase.LoadAssetAtPath<ScreenDataSO>(assetPath);
                if (ReferenceEquals(screenData, null))
                    continue;

                ScreenNode node = BuildNode(screenData, assetPath);
                nodes.Add(node);
                AddEdges(node, edges);
            }

            return new ScreenGraph(nodes, edges);
        }

        private ScreenNode BuildNode(ScreenDataSO screenData, string assetPath)
        {
            SerializedObject serializedScreen = new SerializedObject(screenData);

            bool isLocked = serializedScreen.FindProperty("_isLocked").boolValue;
            bool closesAll = serializedScreen.FindProperty("_hasToCloseAllScreensOnOpen").boolValue;

            int emptyReferenceCount = 0;
            List<string> nestedIds = GetReferencedScreenIds(serializedScreen.FindProperty("_nestedScreens"), ref emptyReferenceCount);
            List<string> toCloseIds = GetReferencedScreenIds(serializedScreen.FindProperty("_toCloseScreens"), ref emptyReferenceCount);

            return new ScreenNode(screenData.ScreenId, assetPath, isLocked, closesAll, nestedIds, toCloseIds, emptyReferenceCount);
        }

        private List<string> GetReferencedScreenIds(SerializedProperty arrayProperty, ref int emptyReferenceCount)
        {
            List<string> screenIds = new List<string>();
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);
                ScreenDataSO referenced = element.objectReferenceValue as ScreenDataSO;
                if (ReferenceEquals(referenced, null))
                {
                    emptyReferenceCount++;
                    continue;
                }

                screenIds.Add(referenced.ScreenId);
            }

            return screenIds;
        }

        private void AddEdges(ScreenNode node, List<ScreenEdge> edges)
        {
            foreach (string nestedId in node.NestedScreenIds)
            {
                edges.Add(new ScreenEdge(node.ScreenId, nestedId, ScreenEdgeKind.Opens));
            }

            foreach (string toCloseId in node.ToCloseScreenIds)
            {
                edges.Add(new ScreenEdge(node.ScreenId, toCloseId, ScreenEdgeKind.Closes));
            }
        }
    }
}
