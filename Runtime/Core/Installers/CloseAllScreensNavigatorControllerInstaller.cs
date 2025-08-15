using DependencyInjector.Core;
using MVVM.Core;
using MVVM.Core.Installers;
using MVVM.Core.InterfaceAdapters;

namespace ScreenNavigators.Core
{
    public class CloseAllScreensNavigatorControllerInstaller : ControllerInstaller
    {        
        [Inject] private IScreenNavigator _screenNavigator;

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            return new CloseAllScreensNavigatorController(eventBindingViewModel, _screenNavigator);
        }

        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }
    }
}