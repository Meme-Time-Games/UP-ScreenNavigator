using DependencyInjector.Core;
using ScreenNavigators.Core;
using ServiceLocatorPattern;

namespace ScreenNavigators.Editors
{
    public class EditorScreenNavigatorProvider
    {
        public IScreenNavigator GetScreenNavigator()
        {
            if (!ServiceLocatorInstance.Instance.IsContained<IDIContainer>())
                return null;

            IDIContainer container = ServiceLocatorInstance.Instance.Get<IDIContainer>();

            if (!container.IsTypeContained(typeof(IScreenNavigator)))
                return null;

            return container.Get<IScreenNavigator>();
        }
    }
}
