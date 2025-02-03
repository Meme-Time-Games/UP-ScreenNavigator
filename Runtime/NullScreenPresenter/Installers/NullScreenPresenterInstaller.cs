using DependencyInjector.Installers;

namespace ScreenNavigators.Core
{
    public class NullScreenPresenterInstaller : SingleMonoInstaller<IScreenPresenter>
    {
        protected override IScreenPresenter GetData()
        {
            return new NullScreenPresenter();
        }
    }
}