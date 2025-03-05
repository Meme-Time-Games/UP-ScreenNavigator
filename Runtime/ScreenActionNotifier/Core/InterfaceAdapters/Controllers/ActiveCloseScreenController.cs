using MVVM.Core;
using MVVM.Core.InterfaceAdapters;
using ScreenNavigators.Core;

namespace ScreenActionNotifier.Core
{
    public class ActiveCloseScreenController : Controller
    {
        private IScreenPresenter _screenPresenter;
        private bool _isScreenActive;
        
        public ActiveCloseScreenController(IEventViewModel eventViewModel, IScreenPresenter screenPresenter, bool isScreenActive) : base(eventViewModel)
        {
            _screenPresenter = screenPresenter;
            _isScreenActive = isScreenActive;
        }

        public override void Execute()
        {
            if (_isScreenActive)
            {
                _screenPresenter.Close();
                _isScreenActive = false;
                return;
            }
            
            _isScreenActive = true;
            _screenPresenter.Present();
        }
    }
}