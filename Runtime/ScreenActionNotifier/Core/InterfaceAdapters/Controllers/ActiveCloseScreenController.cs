using System;
using MVVM.Core;
using MVVM.Core.InterfaceAdapters;

namespace ScreenActionNotifier.Core
{
    public class ActiveCloseScreenController : Controller
    {
        private readonly IEventViewModel _openScreenEventViewModel;
        private readonly IEventViewModel _closeScreenEventViewModel;
        private bool _isScreenActive;
        
        public ActiveCloseScreenController(IEventViewModel openRequestEventViewModel, IEventViewModel openScreenEventViewModel, IEventViewModel closeScreenEventViewModel, bool isScreenActive) : base(openRequestEventViewModel)
        {
            _openScreenEventViewModel = openScreenEventViewModel;
            _closeScreenEventViewModel = closeScreenEventViewModel;
            
            _closeScreenEventViewModel.OnEventRaised -= SetScreenInactive;
            _closeScreenEventViewModel.OnEventRaised += SetScreenInactive;
            
            _isScreenActive = isScreenActive;
        }

        public override void Execute()
        {
            if (_isScreenActive)
            {
                _closeScreenEventViewModel.RaiseEvent();
                _isScreenActive = false;
                return;
            }
            
            _isScreenActive = true;
            _openScreenEventViewModel.RaiseEvent();
        }

        private void SetScreenInactive()
        {
            _isScreenActive = false;
        }
    }
}