using DependencyInjector.Core;

namespace ScreenNavigators.Core
{
    public class ScreenOpenNotifierInstaller : ScreenActionNotifierInstaller
    {
        
        
        [Inject] private IScreenPresenter _screenPresenter;
        
        protected override ScreenActionNotifier GetScreenActionNotifier(IScreenNavigator screenNavigator, string screenIdToOpen)
        {
            return new ScreenOpenNotifier(screenNavigator, screenIdToOpen, _screenPresenter);
        }
    }
}