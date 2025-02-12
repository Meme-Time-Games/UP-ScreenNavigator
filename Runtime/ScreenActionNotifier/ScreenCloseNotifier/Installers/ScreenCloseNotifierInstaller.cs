using Commands.Core;
using DependencyInjector.Core;

namespace ScreenNavigators.Core
{
    public class ScreenCloseNotifierInstaller : ScreenActionNotifierInstaller
    {
        [Inject] private ICommand _command;
        
        protected override ScreenActionNotifier GetScreenActionNotifier(IScreenNavigator screenNavigator, string screenIdToOpen)
        {
            return new ScreenCloseCommandExecutor(screenNavigator, screenIdToOpen, _command);
        }
    }
}