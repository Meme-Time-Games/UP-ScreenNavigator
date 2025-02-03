using DependencyInjector.Core;

namespace ScreenNavigators.Core
{
    public class ScreenCloseNotifierInstaller : ScreenActionNotifierInstaller
    {
        [Inject] private IScreenPresenter _screenPresenter;
        
        protected override ScreenActionNotifier GetScreenActionNotifier(IScreenNavigator screenNavigator, string screenIdToOpen)
        {
            return new ScreenCloseNotifier(screenNavigator, screenIdToOpen, _screenPresenter);
        }
    }
}