using DependencyInjector.Installers;
using MVVM.Core;
using ScreenActionNotifier.Core;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class ActiveCloseScreenControllerInstaller : SingleMonoInstaller<ActiveCloseScreenController>
    {
        [SerializeField] private EventViewModelSO _openRequestEventViewModelSO;
        [SerializeField] private EventViewModelSO _openScreenEventViewModelSO;
        [SerializeField] private EventViewModelSO _closeScreenEventViewModelSO;
        [SerializeField] private bool _isScreenActive;
        
        protected override ActiveCloseScreenController GetData()
        {
            return new ActiveCloseScreenController(_openRequestEventViewModelSO.GetEventViewModel(), _openScreenEventViewModelSO.GetEventViewModel(), _closeScreenEventViewModelSO.GetEventViewModel() , _isScreenActive);
        }
    }
}