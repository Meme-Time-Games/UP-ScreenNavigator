using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScreenNavigators.Editors
{
    public class ScreenGraphViewTabView : IScreenMapTab
    {
        private static readonly Color OpensColor = new Color(0.4f, 0.75f, 0.45f);
        private static readonly Color ClosesColor = new Color(0.85f, 0.45f, 0.45f);

        private readonly ScreenGraphElement _graphElement;

        public string Title => "Graph (Node)";
        public VisualElement RootElement => _graphElement;

        public ScreenGraphViewTabView()
        {
            _graphElement = new ScreenGraphElement();
            _graphElement.style.position = Position.Absolute;
            _graphElement.style.left = 0f;
            _graphElement.style.right = 0f;
            _graphElement.style.top = 20f;
            _graphElement.style.bottom = 0f;
            _graphElement.style.display = DisplayStyle.None;
        }

        public void Draw(ScreenMapContext context)
        {
        }

        public void Rebuild(ScreenMapContext context)
        {
            _graphElement.Rebuild(context, OpensColor, ClosesColor);
        }

        public void SetVisible()
        {
            _graphElement.style.display = DisplayStyle.Flex;
        }

        public void SetHidden()
        {
            _graphElement.style.display = DisplayStyle.None;
        }
    }
}
