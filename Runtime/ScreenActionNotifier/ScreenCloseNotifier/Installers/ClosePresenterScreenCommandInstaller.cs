using Commands.Core;
using DependencyInjector.Core;

namespace ScreenNavigators.Core
{
    public class ClosePresenterScreenCommandInstaller : CommandInstaller
    {
        [Inject] private IScreenPresenter _screenPresenter;
        
        protected override ICommand GetData()
        {
            return new ClosePresenterScreenCommand(_screenPresenter);
        }
    }
}