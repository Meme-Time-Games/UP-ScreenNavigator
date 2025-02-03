using Commands.Core;
using DependencyInjector.Core;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class OpenScreenCommandInstaller : CommandInstaller
    {
        [Header("References")] 
        [SerializeField] private ScreenDataSO _screenDataSo;
        
        [Inject] private IScreenNavigator _screenNavigator;
        
        protected override ICommand GetData()
        {
            return new OpenScreenCommand(_screenNavigator, _screenDataSo.ScreenId);
        }
    }
}