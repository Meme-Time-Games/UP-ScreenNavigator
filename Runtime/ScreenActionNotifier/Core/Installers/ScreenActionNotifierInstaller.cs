using DependencyInjector.Core;
using DependencyInjector.Installers;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public abstract class ScreenActionNotifierInstaller : SingleMonoInstaller<ScreenActionNotifier>
    {
        [Header("References")] 
        [SerializeField] private ScreenDataSO _screenDataSo;
        
        [Inject] private IScreenNavigator _screenNavigator;
        
        protected override ScreenActionNotifier GetData()
        {
            return GetScreenActionNotifier(_screenNavigator, _screenDataSo.ScreenId);
        }

        protected abstract ScreenActionNotifier GetScreenActionNotifier(IScreenNavigator screenNavigator, string screenIdToOpen);
    }
}