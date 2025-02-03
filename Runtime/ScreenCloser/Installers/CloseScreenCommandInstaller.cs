using Commands.Core;
using DependencyInjector.Core;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class CloseScreenCommandInstaller : CommandInstaller
    {
        [Header("References")] 
        [SerializeField] private ScreenDataSO _screenDataSo;
        
        [Inject] private IScreenNavigator _screenNavigator;
        
        protected override ICommand GetData()
        {
            return new CloseScreenCommand(_screenNavigator, _screenDataSo.ScreenId);
        }
    }
}