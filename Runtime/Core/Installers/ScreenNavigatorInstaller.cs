using CrudRepository.Core;
using DependencyInjector.Core;
using DependencyInjector.Installers;

namespace ScreenNavigators.Core
{
    public class ScreenNavigatorInstaller : SingleMonoInstaller<IScreenNavigator>
    {
        [Inject] private ICrudRepository<string, ScreenData> _screensRepository;
        
        protected override IScreenNavigator GetData()
        {
            return new ScreenNavigator(_screensRepository);
        }
    }
}