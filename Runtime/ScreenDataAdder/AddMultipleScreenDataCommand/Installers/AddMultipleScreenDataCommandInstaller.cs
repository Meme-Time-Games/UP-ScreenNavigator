using Commands.Core;
using DependencyInjector.Core;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class AddMultipleScreenDataCommandInstaller : CommandInstaller
    {
        [Header("References")] 
        [SerializeField] private ScreenDataSO[] _screensDataSo;
        
        [Inject] private readonly IScreenNavigator _screenNavigator;
        
        protected override ICommand GetData()
        {
            int totalScreens = _screensDataSo.Length;
            ScreenData[] screensData = new ScreenData[totalScreens];
            for (int i = 0; i < totalScreens; i++)
            {
                screensData[i] = _screensDataSo[i].GetScreenData();
            }
            
            return new AddMultipleScreenDataCommand(_screenNavigator, screensData);
        }
    }
}