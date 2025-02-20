using Commands.Core;
using DependencyInjector.Core;

namespace ScreenNavigators.Core
{
    public class PresentScreenCommandInstaller : CommandInstaller
    {
        [Inject] private IScreenPresenter _screenPresenter;
        
        protected override ICommand GetData()
        {
            return new PresentScreenCommand(_screenPresenter);
        }
    }
}