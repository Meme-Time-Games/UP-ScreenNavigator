using System.Collections.Generic;
using ScreenNavigators.Core;

namespace ScreenNavigators.Editors
{
    public class OpenScreensProvider
    {
        private readonly EditorScreenNavigatorProvider _navigatorProvider;

        public OpenScreensProvider(EditorScreenNavigatorProvider navigatorProvider)
        {
            _navigatorProvider = navigatorProvider;
        }

        public HashSet<string> GetOpenScreenIds()
        {
            HashSet<string> openScreenIds = new HashSet<string>();

            IScreenNavigator navigator = _navigatorProvider.GetScreenNavigator();
            if (ReferenceEquals(navigator, null))
                return openScreenIds;

            foreach (string screenId in navigator.GetOpenScreenIds())
            {
                openScreenIds.Add(screenId);
            }

            return openScreenIds;
        }
    }
}
