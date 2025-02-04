using DependencyInjector.Core;
using DependencyInjector.Installers;

namespace ScreenNavigators.Core
{
    public abstract class EmbeddedScreenPresenterInstaller<TScreenPresenter> : SingleMonoInstaller<IScreenPresenter>
    {
        [Inject] private TScreenPresenter _screenPresenter;

        protected override IScreenPresenter GetData()
        {
            return GetEmbeddedScreenPresenter(_screenPresenter);
        }

        protected abstract EmbeddedScreenPresenter<TScreenPresenter> GetEmbeddedScreenPresenter(
            TScreenPresenter screenPresenter);
    }
}