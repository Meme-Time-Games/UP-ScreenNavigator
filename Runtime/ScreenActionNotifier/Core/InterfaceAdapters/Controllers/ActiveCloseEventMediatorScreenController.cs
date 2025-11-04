using MVVM.Core;
using MVVM.Core.InterfaceAdapters;

namespace ScreenActionNotifier.Core
{
    public class ActiveCloseEventMediatorScreenController : Controller
    {
        private readonly IEventViewModel _openScreenEventViewModel;
        private readonly IEventViewModel _closeScreenEventViewModel;
        private readonly IEventViewModel _openScreenRequestEventViewModel;
        private readonly IEventViewModel _closeScreenRequestEventViewModel;
        private bool _isScreenActive;

        public ActiveCloseEventMediatorScreenController(IEventViewModel openRequestEventViewModel,
            IEventViewModel openScreenEventViewModel,
            IEventViewModel closeScreenEventViewModel,
            bool isScreenActive,
            IEventViewModel openScreenRequestEventViewModel,
            IEventViewModel closeScreenRequestEventViewModel)
            : base(openRequestEventViewModel)
        {
            _openScreenEventViewModel = openScreenEventViewModel;
            _closeScreenEventViewModel = closeScreenEventViewModel;
            _openScreenRequestEventViewModel = openScreenRequestEventViewModel;
            _closeScreenRequestEventViewModel = closeScreenRequestEventViewModel;

            _openScreenRequestEventViewModel.OnEventRaised += RequestOpenScreen;
            _closeScreenRequestEventViewModel.OnEventRaised += RequestCloseScreen;

            _isScreenActive = isScreenActive;
        }

        public override void Execute()
        {
            if (_isScreenActive)
            {
                RequestCloseScreen();
                return;
            }

            RequestOpenScreen();
        }

        private void RequestOpenScreen()
        {
            if (_isScreenActive)
                return;

            _openScreenEventViewModel.RaiseEvent();
            _isScreenActive = true;
        }

        private void RequestCloseScreen()
        {
            if (!_isScreenActive)
                return;

            _closeScreenEventViewModel.RaiseEvent();
            _isScreenActive = false;
        }

        public override void Dispose()
        {
            base.Dispose();

            _openScreenRequestEventViewModel.OnEventRaised -= RequestOpenScreen;
            _closeScreenRequestEventViewModel.OnEventRaised -= RequestCloseScreen;
        }
    }
}