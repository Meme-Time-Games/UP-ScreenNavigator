using NUnit.Framework;

namespace ScreenNavigators.Editors.Tests
{
    public class ScreenGroupNameProviderTests
    {
        [Test]
        public void GetGroupName_FeatureFolderWithDataSubfolder_ReturnsFeatureFolder()
        {
            string groupName = new ScreenGroupNameProvider()
                .GetGroupName("Assets/Project/Game/Gameplay/Chat/Data/Chat-ScreenDataSO.asset");

            Assert.AreEqual("Chat", groupName);
        }

        [Test]
        public void GetGroupName_NestedGenericFolders_SkipsAllGenericFolders()
        {
            string groupName = new ScreenGroupNameProvider()
                .GetGroupName("Assets/ScreenNavigator/Data/Screens/Screen1-ScreenDataSO.asset");

            Assert.AreEqual("ScreenNavigator", groupName);
        }

        [Test]
        public void GetGroupName_AssetInPackage_ReturnsPackageFolder()
        {
            string groupName = new ScreenGroupNameProvider()
                .GetGroupName("Packages/UP-Chat-Core/Runtime/Data/Chat-ScreenDataSO.asset");

            Assert.AreEqual("UP-Chat-Core", groupName);
        }

        [Test]
        public void GetGroupName_OnlyGenericFolders_ReturnsUngrouped()
        {
            string groupName = new ScreenGroupNameProvider()
                .GetGroupName("Assets/Data/Screen1-ScreenDataSO.asset");

            Assert.AreEqual("Ungrouped", groupName);
        }

        [Test]
        public void GetGroupName_EmptyPath_ReturnsUngrouped()
        {
            string groupName = new ScreenGroupNameProvider().GetGroupName("");

            Assert.AreEqual("Ungrouped", groupName);
        }
    }
}
