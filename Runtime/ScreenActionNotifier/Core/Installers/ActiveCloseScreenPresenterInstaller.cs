using DependencyInjector.Core;
using DependencyInjector.Installers;

namespace ScreenNavigators.Core
{
    public class ActiveCloseScreenPresenterInstaller : SingleMonoInstaller<IScreenPresenter>
    {
        [Inject] private IActiveScreenPresenter _activeScreenPresenter;
        [Inject] private ICloseScreenPresenter _closeScreenPresenter;
        [Inject] private IScreenPresenter _screenPresenter;
        
        protected override IScreenPresenter GetData()
        {
            return new ActiveCloseScreenPresenter(_activeScreenPresenter, _closeScreenPresenter, _screenPresenter);
        }
    }
}