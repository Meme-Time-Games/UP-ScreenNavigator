using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ScreenNavigators.Editors
{
    public class ScreenListPanel : VisualElement
    {
        private const float RowHeight = 24f;
        private const float RowPadding = 8f;
        private const float DepthIndent = 14f;

        private readonly ListView _listView;
        private readonly List<ScreenListRow> _visibleRows = new List<ScreenListRow>();

        private ScreenMapContext _context;
        private bool _usesCompactBadges;

        public Action<ScreenNode> OnScreenSelected { get; set; }

        public ScreenListPanel()
        {
            AddToClassList("list-panel");

            _listView = new ListView();
            _listView.AddToClassList("screen-list");
            _listView.fixedItemHeight = RowHeight;
            _listView.selectionType = SelectionType.Single;
            _listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
            _listView.itemsSource = _visibleRows;
            _listView.makeItem = CreateRow;
            _listView.bindItem = BindRow;
            _listView.selectionChanged += HandleSelectionChanged;
            _listView.itemsChosen += HandleItemsChosen;

            Add(_listView);
        }

        public void SetContext(ScreenMapContext context)
        {
            _context = context;
        }

        public void SetCompactBadges()
        {
            _usesCompactBadges = true;
            _listView.RefreshItems();
        }

        public void SetWideBadges()
        {
            _usesCompactBadges = false;
            _listView.RefreshItems();
        }

        public void ApplyFilter(ScreenListFilter filter)
        {
            _visibleRows.Clear();

            if (HasHierarchy(filter))
            {
                AddOpenHierarchyRows();
                AddClosedRows(filter);
                _listView.RefreshItems();
                return;
            }

            AddFlatRows(filter);
            _listView.RefreshItems();
        }

        public void RefreshRows()
        {
            _listView.RefreshItems();
        }

        public void SelectScreen(string screenId)
        {
            int index = GetIndexOfScreen(screenId);
            if (index < 0)
                return;

            _listView.SetSelection(index);
            _listView.ScrollToItem(index);
        }

        public int GetIndexOfScreen(string screenId)
        {
            for (int i = 0; i < _visibleRows.Count; i++)
            {
                if (_visibleRows[i].Node.ScreenId == screenId)
                    return i;
            }

            return -1;
        }

        /* ---------- row building ---------- */

        private bool HasHierarchy(ScreenListFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.SearchText))
                return false;

            if (filter.ShowsOnlyIssues)
                return false;

            return HasAnyOpenScreen();
        }

        private bool HasAnyOpenScreen()
        {
            foreach (ScreenNode node in _context.Graph.Nodes)
            {
                if (_context.IsScreenOpen(node.ScreenId))
                    return true;
            }

            return false;
        }

        private void AddOpenHierarchyRows()
        {
            List<ScreenNode> roots = GetOpenRoots();
            roots.Sort(CompareByScreenId);

            HashSet<string> visited = new HashSet<string>();
            foreach (ScreenNode root in roots)
            {
                AddOpenBranchRows(root, 0, visited);
            }
        }

        private void AddOpenBranchRows(ScreenNode node, int depth, HashSet<string> visited)
        {
            if (visited.Contains(node.ScreenId))
                return;

            visited.Add(node.ScreenId);
            _visibleRows.Add(new ScreenListRow(node, depth));

            List<ScreenNode> children = GetOpenNestedChildren(node);
            children.Sort(CompareByScreenId);

            foreach (ScreenNode child in children)
            {
                AddOpenBranchRows(child, depth + 1, visited);
            }
        }

        private List<ScreenNode> GetOpenRoots()
        {
            List<ScreenNode> roots = new List<ScreenNode>();

            foreach (ScreenNode node in _context.Graph.Nodes)
            {
                if (!_context.IsScreenOpen(node.ScreenId))
                    continue;

                if (HasOpenParent(node))
                    continue;

                roots.Add(node);
            }

            if (roots.Count > 0)
                return roots;

            return GetAllOpenScreens();
        }

        private List<ScreenNode> GetAllOpenScreens()
        {
            List<ScreenNode> openScreens = new List<ScreenNode>();

            foreach (ScreenNode node in _context.Graph.Nodes)
            {
                if (!_context.IsScreenOpen(node.ScreenId))
                    continue;

                openScreens.Add(node);
            }

            return openScreens;
        }

        private bool HasOpenParent(ScreenNode node)
        {
            foreach (ScreenNode candidate in _context.Graph.Nodes)
            {
                if (candidate.ScreenId == node.ScreenId)
                    continue;

                if (!_context.IsScreenOpen(candidate.ScreenId))
                    continue;

                if (!ContainsScreenId(candidate.NestedScreenIds, node.ScreenId))
                    continue;

                return true;
            }

            return false;
        }

        private List<ScreenNode> GetOpenNestedChildren(ScreenNode node)
        {
            List<ScreenNode> children = new List<ScreenNode>();

            foreach (string nestedId in node.NestedScreenIds)
            {
                if (!_context.IsScreenOpen(nestedId))
                    continue;

                ScreenNode child = GetNodeById(nestedId);
                if (ReferenceEquals(child, null))
                    continue;

                children.Add(child);
            }

            return children;
        }

        private void AddClosedRows(ScreenListFilter filter)
        {
            if (filter.ShowsOnlyOpen)
                return;

            List<ScreenNode> closedScreens = new List<ScreenNode>();

            foreach (ScreenNode node in _context.Graph.Nodes)
            {
                if (_context.IsScreenOpen(node.ScreenId))
                    continue;

                closedScreens.Add(node);
            }

            closedScreens.Sort(CompareByScreenId);

            foreach (ScreenNode node in closedScreens)
            {
                _visibleRows.Add(new ScreenListRow(node, 0));
            }
        }

        private void AddFlatRows(ScreenListFilter filter)
        {
            List<ScreenNode> nodes = new List<ScreenNode>();

            foreach (ScreenNode node in _context.Graph.Nodes)
            {
                if (!IsVisible(node, filter))
                    continue;

                nodes.Add(node);
            }

            nodes.Sort(CompareByScreenId);

            foreach (ScreenNode node in nodes)
            {
                _visibleRows.Add(new ScreenListRow(node, 0));
            }
        }

        /* ---------- row rendering ---------- */

        private VisualElement CreateRow()
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("screen-row");

            Label childMark = new Label("↳");
            childMark.name = "child-mark";
            childMark.AddToClassList("screen-row__child-mark");
            row.Add(childMark);

            VisualElement statusDot = new VisualElement();
            statusDot.AddToClassList("status-dot");
            row.Add(statusDot);

            Label nameLabel = new Label();
            nameLabel.AddToClassList("screen-row__name");
            row.Add(nameLabel);

            row.Add(CreateBadge("warn"));
            row.Add(CreateBadge("locked"));
            row.Add(CreateBadge("closes-all"));

            return row;
        }

        private Label CreateBadge(string badgeName)
        {
            Label badge = new Label();
            badge.name = badgeName;
            badge.AddToClassList("badge");
            return badge;
        }

        private void BindRow(VisualElement row, int index)
        {
            ScreenListRow listRow = _visibleRows[index];
            ScreenNode node = listRow.Node;

            row.style.paddingLeft = RowPadding + listRow.Depth * DepthIndent;

            BindChildMark(row, listRow);
            BindStatusDot(row, node);
            BindName(row, node);
            BindWarnBadge(row, node);
            BindLockedBadge(row, node);
            BindClosesAllBadge(row, node);
        }

        private void BindChildMark(VisualElement row, ScreenListRow listRow)
        {
            Label childMark = row.Q<Label>("child-mark");
            childMark.RemoveFromClassList("is-hidden");

            if (listRow.Depth > 0)
                return;

            childMark.AddToClassList("is-hidden");
        }

        private void BindStatusDot(VisualElement row, ScreenNode node)
        {
            VisualElement statusDot = row.Q(className: "status-dot");
            statusDot.RemoveFromClassList("status-dot--open");
            statusDot.tooltip = "Closed";

            if (!_context.IsScreenOpen(node.ScreenId))
                return;

            statusDot.AddToClassList("status-dot--open");
            statusDot.tooltip = "Open";
        }

        private void BindName(VisualElement row, ScreenNode node)
        {
            Label nameLabel = row.Q<Label>(className: "screen-row__name");
            nameLabel.RemoveFromClassList("screen-row__name--empty");
            nameLabel.text = node.ScreenId;

            if (!string.IsNullOrEmpty(node.ScreenId))
                return;

            nameLabel.text = "(empty id)";
            nameLabel.AddToClassList("screen-row__name--empty");
        }

        private void BindWarnBadge(VisualElement row, ScreenNode node)
        {
            Label badge = row.Q<Label>("warn");
            badge.text = "!";
            badge.tooltip = "Has validation issues";
            badge.AddToClassList("badge--warn");
            HideBadge(badge);

            if (!node.HasIssues())
                return;

            ShowBadge(badge);
        }

        private void BindLockedBadge(VisualElement row, ScreenNode node)
        {
            Label badge = row.Q<Label>("locked");
            badge.text = GetLockedBadgeText();
            badge.tooltip = "Locked: nested closing skips this screen";
            HideBadge(badge);

            if (!node.IsLocked)
                return;

            ShowBadge(badge);
        }

        private string GetLockedBadgeText()
        {
            if (_usesCompactBadges)
                return "L";

            return "LOCK";
        }

        private void BindClosesAllBadge(VisualElement row, ScreenNode node)
        {
            Label badge = row.Q<Label>("closes-all");
            badge.text = GetClosesAllBadgeText();
            badge.tooltip = "Closes every other screen when it opens";
            badge.AddToClassList("badge--close");
            HideBadge(badge);

            if (!node.ClosesAllScreensOnOpen)
                return;

            ShowBadge(badge);
        }

        private string GetClosesAllBadgeText()
        {
            if (_usesCompactBadges)
                return "ALL";

            return "CLOSES ALL";
        }

        private void ShowBadge(Label badge)
        {
            badge.RemoveFromClassList("badge--hidden");
        }

        private void HideBadge(Label badge)
        {
            badge.AddToClassList("badge--hidden");
        }

        /* ---------- interaction ---------- */

        private void HandleSelectionChanged(IEnumerable<object> selection)
        {
            foreach (object item in selection)
            {
                ScreenListRow listRow = item as ScreenListRow;
                if (ReferenceEquals(listRow, null))
                    return;

                OnScreenSelected?.Invoke(listRow.Node);
                return;
            }
        }

        private void HandleItemsChosen(IEnumerable<object> chosenItems)
        {
            foreach (object item in chosenItems)
            {
                PingScreen(item as ScreenListRow);
                return;
            }
        }

        private void PingScreen(ScreenListRow listRow)
        {
            if (ReferenceEquals(listRow, null))
                return;

            _context.PingScreen(listRow.Node.AssetPath);
        }

        /* ---------- queries ---------- */

        private bool IsVisible(ScreenNode node, ScreenListFilter filter)
        {
            if (filter.ShowsOnlyOpen && !_context.IsScreenOpen(node.ScreenId))
                return false;

            if (filter.ShowsOnlyIssues && !node.HasIssues())
                return false;

            return IsMatchingSearch(node.ScreenId, filter.SearchText);
        }

        private bool IsMatchingSearch(string screenId, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return true;

            if (string.IsNullOrEmpty(screenId))
                return false;

            return screenId.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
        }

        private bool ContainsScreenId(IReadOnlyList<string> screenIds, string screenId)
        {
            foreach (string candidate in screenIds)
            {
                if (candidate == screenId)
                    return true;
            }

            return false;
        }

        private ScreenNode GetNodeById(string screenId)
        {
            foreach (ScreenNode node in _context.Graph.Nodes)
            {
                if (node.ScreenId == screenId)
                    return node;
            }

            return null;
        }

        private int CompareByScreenId(ScreenNode first, ScreenNode second)
        {
            return string.Compare(first.ScreenId, second.ScreenId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
