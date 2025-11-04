using MVVM.Core;
using MVVM.Core.InterfaceAdapters;

namespace ScreenNavigators.Core
{
    public class CloseAllScreensNavigatorController : Controller
    {
        private readonly IScreenNavigator _screenNavigator;

        public CloseAllScreensNavigatorController(IEventViewModel eventViewModel, IScreenNavigator screenNavigator) : base(eventViewModel) 
        { 
            _screenNavigator = screenNavigator;
        }

        public override void Execute()
        {
            _screenNavigator.CloseAllScreens();
        }
    }
}