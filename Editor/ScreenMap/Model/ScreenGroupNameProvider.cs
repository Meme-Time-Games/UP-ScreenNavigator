namespace ScreenNavigators.Editors
{
    public class ScreenGroupNameProvider
    {
        private const string UngroupedName = "Ungrouped";

        private static readonly string[] GenericFolderNames =
        {
            "assets",
            "packages",
            "data",
            "screen",
            "screens",
            "scriptableobject",
            "scriptableobjects",
            "so",
            "sos",
            "resources",
            "prefabs",
            "scene",
            "scenes",
            "settings",
            "config",
            "configs",
            "runtime",
            "editor"
        };

        public string GetGroupName(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return UngroupedName;

            string[] segments = assetPath.Split('/');

            for (int i = segments.Length - 2; i >= 0; i--)
            {
                if (IsGenericFolder(segments[i]))
                    continue;

                return segments[i];
            }

            return UngroupedName;
        }

        private bool IsGenericFolder(string folderName)
        {
            string normalizedName = folderName.ToLowerInvariant();

            foreach (string genericName in GenericFolderNames)
            {
                if (normalizedName == genericName)
                    return true;
            }

            return false;
        }
    }
}
