using Commands.Core;
using DependencyInjector.Core;

namespace ScreenNavigators.Core
{
    public class ScreenOpenNotifierInstaller : ScreenActionNotifierInstaller
    {
        [Inject] private ICommand _command;
        
        protected override ScreenActionNotifier GetScreenActionNotifier(IScreenNavigator screenNavigator, string screenIdToOpen)
        {
            return new ScreenOpenCommandExecutor(screenNavigator, screenIdToOpen, _command);
        }
    }
}