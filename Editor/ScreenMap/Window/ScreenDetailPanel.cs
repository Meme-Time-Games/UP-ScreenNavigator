using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ScreenNavigators.Editors
{
    public class ScreenDetailPanel : VisualElement
    {
        private readonly ScrollView _scrollView;
        private readonly VisualElement _body;
        private readonly VisualElement _emptyState;

        private ScreenMapContext _context;

        public Action<string> OnScreenRequested { get; set; }

        public ScreenDetailPanel()
        {
            AddToClassList("detail-panel");

            _emptyState = CreateEmptyState();
            Add(_emptyState);

            _scrollView = new ScrollView();
            _scrollView.AddToClassList("detail__scroll");
            Add(_scrollView);

            _body = new VisualElement();
            _body.AddToClassList("detail__body");
            _scrollView.Add(_body);

            ShowEmptyState();
        }

        public void SetContext(ScreenMapContext context)
        {
            _context = context;
        }

        public void ShowEmptyState()
        {
            _emptyState.style.display = DisplayStyle.Flex;
            _scrollView.style.display = DisplayStyle.None;
        }

        public void ShowScreen(ScreenNode node)
        {
            _emptyState.style.display = DisplayStyle.None;
            _scrollView.style.display = DisplayStyle.Flex;

            _body.Clear();
            _body.Add(CreateHeader(node));
            _body.Add(CreateSection("OPENS", "section--opens", node.NestedScreenIds));
            _body.Add(CreateSection("CLOSES", "section--closes", node.ToCloseScreenIds));
            _body.Add(CreateSection("OPENED BY", "section--opened-by", GetSources(node.ScreenId, ScreenEdgeKind.Opens)));
            _body.Add(CreateSection("CLOSED BY", "section--closed-by", GetSources(node.ScreenId, ScreenEdgeKind.Closes)));
            AddIssues(node);
        }

        private VisualElement CreateEmptyState()
        {
            VisualElement empty = new VisualElement();
            empty.AddToClassList("detail__empty");

            Label label = new Label("Select a screen to see how it connects.");
            label.AddToClassList("detail__empty-label");
            empty.Add(label);

            return empty;
        }

        private VisualElement CreateHeader(ScreenNode node)
        {
            VisualElement header = new VisualElement();

            VisualElement titleRow = new VisualElement();
            titleRow.AddToClassList("detail__header");

            Label title = new Label(GetDisplayId(node.ScreenId));
            title.AddToClassList("detail__title");
            titleRow.Add(title);

            header.Add(titleRow);
            header.Add(CreateMetaRow(node));

            Label path = new Label(node.AssetPath);
            path.AddToClassList("detail__path");
            path.tooltip = node.AssetPath;
            header.Add(path);

            return header;
        }

        private VisualElement CreateMetaRow(ScreenNode node)
        {
            VisualElement meta = new VisualElement();
            meta.AddToClassList("detail__meta");

            meta.Add(CreateStatusPill(node));

            if (node.IsLocked)
                meta.Add(CreatePill("locked"));

            if (node.ClosesAllScreensOnOpen)
                meta.Add(CreatePill("closes all on open"));

            return meta;
        }

        private Label CreateStatusPill(ScreenNode node)
        {
            Label pill = CreatePill("Closed");

            if (!_context.IsScreenOpen(node.ScreenId))
                return pill;

            pill.text = "Open";
            pill.AddToClassList("pill--open");
            return pill;
        }

        private Label CreatePill(string text)
        {
            Label pill = new Label(text);
            pill.AddToClassList("pill");
            return pill;
        }

        private VisualElement CreateSection(string label, string modifierClass, IReadOnlyList<string> screenIds)
        {
            VisualElement section = new VisualElement();
            section.AddToClassList("section");
            section.AddToClassList(modifierClass);

            VisualElement head = new VisualElement();
            head.AddToClassList("section__head");

            Label sectionLabel = new Label(label);
            sectionLabel.AddToClassList("section__label");
            head.Add(sectionLabel);

            Label count = new Label(screenIds.Count.ToString());
            count.AddToClassList("section__count");
            head.Add(count);

            section.Add(head);
            section.Add(CreateChips(screenIds));

            return section;
        }

        private VisualElement CreateChips(IReadOnlyList<string> screenIds)
        {
            if (screenIds.Count == 0)
            {
                Label none = new Label("none");
                none.AddToClassList("section__none");
                return none;
            }

            VisualElement chips = new VisualElement();
            chips.AddToClassList("section__chips");

            foreach (string screenId in screenIds)
            {
                chips.Add(CreateChip(screenId));
            }

            return chips;
        }

        private Button CreateChip(string screenId)
        {
            Button chip = new Button(() => OnScreenRequested?.Invoke(screenId));
            chip.text = GetDisplayId(screenId);
            chip.AddToClassList("chip");
            return chip;
        }

        private void AddIssues(ScreenNode node)
        {
            if (!node.HasIssues())
                return;

            VisualElement issues = new VisualElement();
            issues.AddToClassList("issues");

            foreach (ValidationIssue issue in node.Issues)
            {
                issues.Add(CreateIssue(issue));
            }

            _body.Add(issues);
        }

        private VisualElement CreateIssue(ValidationIssue issue)
        {
            VisualElement element = new VisualElement();
            element.AddToClassList("issue");
            element.AddToClassList(GetIssueClass(issue.Severity));

            Label message = new Label(issue.Message);
            message.AddToClassList("issue__message");
            element.Add(message);

            return element;
        }

        private string GetIssueClass(ValidationSeverity severity)
        {
            if (severity == ValidationSeverity.Error)
                return "issue--error";

            return "issue--warning";
        }

        private List<string> GetSources(string screenId, ScreenEdgeKind kind)
        {
            List<string> sources = new List<string>();

            foreach (ScreenEdge edge in _context.Graph.Edges)
            {
                if (edge.Kind != kind)
                    continue;

                if (edge.ToScreenId != screenId)
                    continue;

                sources.Add(edge.FromScreenId);
            }

            return sources;
        }

        private string GetDisplayId(string screenId)
        {
            if (string.IsNullOrEmpty(screenId))
                return "(empty id)";

            return screenId;
        }
    }
}
