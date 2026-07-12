namespace ScreenNavigators.Editors
{
    public class ScreenListFilter
    {
        private readonly string _searchText;
        private readonly bool _showsOnlyOpen;
        private readonly bool _showsOnlyIssues;
        private readonly bool _groupsByFeature;

        public string SearchText => _searchText;
        public bool ShowsOnlyOpen => _showsOnlyOpen;
        public bool ShowsOnlyIssues => _showsOnlyIssues;
        public bool GroupsByFeature => _groupsByFeature;

        public ScreenListFilter(string searchText, bool showsOnlyOpen, bool showsOnlyIssues, bool groupsByFeature)
        {
            _searchText = searchText;
            _showsOnlyOpen = showsOnlyOpen;
            _showsOnlyIssues = showsOnlyIssues;
            _groupsByFeature = groupsByFeature;
        }
    }
}
