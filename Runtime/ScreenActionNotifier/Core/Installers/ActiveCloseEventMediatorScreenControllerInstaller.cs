using DependencyInjector.Installers;
using MVVM.Core;
using ScreenActionNotifier.Core;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class ActiveCloseEventMediatorScreenControllerInstaller : SingleMonoInstaller<ActiveCloseEventMediatorScreenController>
    {
        [SerializeField] private EventViewModelSO _openRequestEventViewModelSO;
        [SerializeField] private EventViewModelSO _openScreenEventViewModelSO;
        [SerializeField] private EventViewModelSO _closeScreenEventViewModelSO;
        [SerializeField] private bool _isScreenActive;
        [SerializeField] private EventViewModelSO _openScreenRequestEventViewModelSO;
        [SerializeField] private EventViewModelSO _closeScreenRequestEventViewModelSO;

        protected override ActiveCloseEventMediatorScreenController GetData()
        {
            return new ActiveCloseEventMediatorScreenController(_openRequestEventViewModelSO.GetEventViewModel(), 
                _openScreenEventViewModelSO.GetEventViewModel(), 
                _closeScreenEventViewModelSO.GetEventViewModel(),
                _isScreenActive,
                _openScreenRequestEventViewModelSO.GetEventViewModel(),
                _closeScreenRequestEventViewModelSO.GetEventViewModel());
        }
    }
}