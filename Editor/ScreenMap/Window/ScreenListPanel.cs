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

        private const string OpenGroupName = "OPEN";

        private readonly ListView _listView;
        private readonly List<ScreenListRow> _visibleRows = new List<ScreenListRow>();
        private readonly ScreenGroupNameProvider _groupNameProvider = new ScreenGroupNameProvider();
        private readonly HashSet<string> _collapsedGroups = new HashSet<string>();

        private ScreenMapContext _context;
        private ScreenListFilter _currentFilter;
        private bool _usesCompactBadges;

        public Action<ScreenNode> OnScreenSelected { get; set; }
        public Action OnCollapsedGroupsChanged { get; set; }

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

        public void SetCollapsedGroups(List<string> groupNames)
        {
            _collapsedGroups.Clear();

            foreach (string groupName in groupNames)
            {
                _collapsedGroups.Add(groupName);
            }
        }

        public List<string> GetCollapsedGroups()
        {
            return new List<string>(_collapsedGroups);
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
            _currentFilter = filter;
            RebuildRows();
        }

        private void RebuildRows()
        {
            if (ReferenceEquals(_currentFilter, null))
                return;

            _visibleRows.Clear();

            if (HasOpenHierarchy(_currentFilter))
            {
                AddOpenHierarchyRows();
                AddNodeRows(GetClosedScreens(_currentFilter), _currentFilter);
                _listView.RefreshItems();
                return;
            }

            AddNodeRows(GetFilteredScreens(_currentFilter), _currentFilter);
            _listView.RefreshItems();
        }

        private void ToggleGroup(string groupName)
        {
            if (_collapsedGroups.Contains(groupName))
            {
                _collapsedGroups.Remove(groupName);
                HandleCollapsedGroupsChanged();
                return;
            }

            _collapsedGroups.Add(groupName);
            HandleCollapsedGroupsChanged();
        }

        private void HandleCollapsedGroupsChanged()
        {
            RebuildRows();
            OnCollapsedGroupsChanged?.Invoke();
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
                if (_visibleRows[i].IsGroupHeader())
                    continue;

                if (_visibleRows[i].Node.ScreenId == screenId)
                    return i;
            }

            return -1;
        }

        /* ---------- row building ---------- */

        private void AddNodeRows(List<ScreenNode> nodes, ScreenListFilter filter)
        {
            if (filter.GroupsByFeature)
            {
                AddGroupedRows(nodes);
                return;
            }

            AddFlatRows(nodes);
        }

        private void AddFlatRows(List<ScreenNode> nodes)
        {
            nodes.Sort(CompareByScreenId);

            foreach (ScreenNode node in nodes)
            {
                _visibleRows.Add(ScreenListRow.CreateScreenRow(node, 0));
            }
        }

        private void AddGroupedRows(List<ScreenNode> nodes)
        {
            Dictionary<string, List<ScreenNode>> groups = GetGroups(nodes);

            List<string> groupNames = new List<string>(groups.Keys);
            groupNames.Sort(CompareGroupNames);

            foreach (string groupName in groupNames)
            {
                List<ScreenNode> groupNodes = groups[groupName];
                groupNodes.Sort(CompareByScreenId);

                _visibleRows.Add(ScreenListRow.CreateGroupHeader(groupName));

                if (_collapsedGroups.Contains(groupName))
                    continue;

                foreach (ScreenNode node in groupNodes)
                {
                    _visibleRows.Add(ScreenListRow.CreateScreenRow(node, 1));
                }
            }
        }

        private Dictionary<string, List<ScreenNode>> GetGroups(List<ScreenNode> nodes)
        {
            Dictionary<string, List<ScreenNode>> groups = new Dictionary<string, List<ScreenNode>>();

            foreach (ScreenNode node in nodes)
            {
                string groupName = _groupNameProvider.GetGroupName(node.AssetPath);

                if (!groups.ContainsKey(groupName))
                    groups.Add(groupName, new List<ScreenNode>());

                groups[groupName].Add(node);
            }

            return groups;
        }

        private List<ScreenNode> GetFilteredScreens(ScreenListFilter filter)
        {
            List<ScreenNode> nodes = new List<ScreenNode>();

            foreach (ScreenNode node in _context.Graph.Nodes)
            {
                if (!IsVisible(node, filter))
                    continue;

                nodes.Add(node);
            }

            return nodes;
        }

        private List<ScreenNode> GetClosedScreens(ScreenListFilter filter)
        {
            List<ScreenNode> nodes = new List<ScreenNode>();

            if (filter.ShowsOnlyOpen)
                return nodes;

            foreach (ScreenNode node in _context.Graph.Nodes)
            {
                if (_context.IsScreenOpen(node.ScreenId))
                    continue;

                nodes.Add(node);
            }

            return nodes;
        }

        /* ---------- open hierarchy ---------- */

        private bool HasOpenHierarchy(ScreenListFilter filter)
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
            _visibleRows.Add(ScreenListRow.CreateGroupHeader(OpenGroupName));

            if (_collapsedGroups.Contains(OpenGroupName))
                return;

            List<ScreenNode> roots = GetOpenRoots();
            roots.Sort(CompareByScreenId);

            HashSet<string> visited = new HashSet<string>();
            foreach (ScreenNode root in roots)
            {
                AddOpenBranchRows(root, 1, visited);
            }
        }

        private void AddOpenBranchRows(ScreenNode node, int depth, HashSet<string> visited)
        {
            if (visited.Contains(node.ScreenId))
                return;

            visited.Add(node.ScreenId);
            _visibleRows.Add(ScreenListRow.CreateScreenRow(node, depth));

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

        /* ---------- row rendering ---------- */

        private VisualElement CreateRow()
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("list-row");

            Label groupHeader = new Label();
            groupHeader.name = "group-header";
            groupHeader.AddToClassList("group-header");
            groupHeader.RegisterCallback<ClickEvent>(HandleGroupHeaderClicked);
            row.Add(groupHeader);

            VisualElement content = new VisualElement();
            content.name = "screen-content";
            content.AddToClassList("screen-row");

            Label childMark = new Label("↳");
            childMark.name = "child-mark";
            childMark.AddToClassList("screen-row__child-mark");
            content.Add(childMark);

            VisualElement statusDot = new VisualElement();
            statusDot.AddToClassList("status-dot");
            content.Add(statusDot);

            Label nameLabel = new Label();
            nameLabel.AddToClassList("screen-row__name");
            content.Add(nameLabel);

            content.Add(CreateBadge("warn"));
            content.Add(CreateBadge("locked"));
            content.Add(CreateBadge("closes-all"));

            row.Add(content);

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

            if (listRow.IsGroupHeader())
            {
                BindGroupHeader(row, listRow);
                return;
            }

            BindScreenRow(row, listRow);
        }

        private void BindGroupHeader(VisualElement row, ScreenListRow listRow)
        {
            Label groupHeader = row.Q<Label>("group-header");
            groupHeader.userData = listRow.GroupName;
            groupHeader.text = GetGroupHeaderText(listRow);
            groupHeader.tooltip = "Click to collapse or expand";
            groupHeader.RemoveFromClassList("is-hidden");

            row.Q("screen-content").AddToClassList("is-hidden");
        }

        private string GetGroupHeaderText(ScreenListRow listRow)
        {
            return GetFoldoutArrow(listRow.GroupName) + "  " + listRow.GroupName;
        }

        private string GetFoldoutArrow(string groupName)
        {
            if (_collapsedGroups.Contains(groupName))
                return "▶";

            return "▼";
        }

        private void HandleGroupHeaderClicked(ClickEvent clickEvent)
        {
            Label groupHeader = clickEvent.currentTarget as Label;
            if (ReferenceEquals(groupHeader, null))
                return;

            string groupName = groupHeader.userData as string;
            if (string.IsNullOrEmpty(groupName))
                return;

            ToggleGroup(groupName);
        }

        private void BindScreenRow(VisualElement row, ScreenListRow listRow)
        {
            row.Q<Label>("group-header").AddToClassList("is-hidden");

            VisualElement content = row.Q("screen-content");
            content.RemoveFromClassList("is-hidden");
            content.style.paddingLeft = RowPadding + listRow.Depth * DepthIndent;

            ScreenNode node = listRow.Node;

            BindChildMark(content, listRow);
            BindStatusDot(content, node);
            BindName(content, node);
            BindWarnBadge(content, node);
            BindLockedBadge(content, node);
            BindClosesAllBadge(content, node);
        }

        private void BindChildMark(VisualElement content, ScreenListRow listRow)
        {
            Label childMark = content.Q<Label>("child-mark");
            childMark.RemoveFromClassList("is-hidden");

            if (listRow.Depth > 1)
                return;

            childMark.AddToClassList("is-hidden");
        }

        private void BindStatusDot(VisualElement content, ScreenNode node)
        {
            VisualElement statusDot = content.Q(className: "status-dot");
            statusDot.RemoveFromClassList("status-dot--open");
            statusDot.tooltip = "Closed";

            if (!_context.IsScreenOpen(node.ScreenId))
                return;

            statusDot.AddToClassList("status-dot--open");
            statusDot.tooltip = "Open";
        }

        private void BindName(VisualElement content, ScreenNode node)
        {
            Label nameLabel = content.Q<Label>(className: "screen-row__name");
            nameLabel.RemoveFromClassList("screen-row__name--empty");
            nameLabel.text = node.ScreenId;
            nameLabel.tooltip = node.ScreenId;

            if (!string.IsNullOrEmpty(node.ScreenId))
                return;

            nameLabel.text = "(empty id)";
            nameLabel.tooltip = node.AssetPath;
            nameLabel.AddToClassList("screen-row__name--empty");
        }

        private void BindWarnBadge(VisualElement content, ScreenNode node)
        {
            Label badge = content.Q<Label>("warn");
            badge.text = "!";
            badge.tooltip = "Has validation issues";
            badge.AddToClassList("badge--warn");
            HideBadge(badge);

            if (!node.HasIssues())
                return;

            ShowBadge(badge);
        }

        private void BindLockedBadge(VisualElement content, ScreenNode node)
        {
            Label badge = content.Q<Label>("locked");
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

        private void BindClosesAllBadge(VisualElement content, ScreenNode node)
        {
            Label badge = content.Q<Label>("closes-all");
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

                if (listRow.IsGroupHeader())
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

            if (listRow.IsGroupHeader())
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

        private int CompareGroupNames(string first, string second)
        {
            return string.Compare(first, second, StringComparison.OrdinalIgnoreCase);
        }
    }
}
