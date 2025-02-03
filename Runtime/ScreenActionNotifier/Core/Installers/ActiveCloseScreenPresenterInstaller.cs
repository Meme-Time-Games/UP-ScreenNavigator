using DependencyInjector.Core;
using DependencyInjector.Installers;

namespace ScreenNavigators.Core
{
    public class ActiveCloseScreenPresenterInstaller : SingleMonoInstaller<IScreenPresenter>
    {
        [Inject] private IScreenActivatorPresenter _screenActivatorPresenter;
        [Inject] private IScreenDeactivatorPresenter _screenDeactivatorPresenter;
        [Inject] private IScreenPresenter _screenPresenter;
        
        protected override IScreenPresenter GetData()
        {
            return new ActiveCloseScreenPresenter(_screenActivatorPresenter, _screenDeactivatorPresenter, _screenPresenter);
        }
    }
}