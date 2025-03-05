using DependencyInjector.Core;
using DependencyInjector.Installers;
using MVVM.Core;
using ScreenActionNotifier.Core;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class ActiveCloseScreenControllerInstaller : SingleMonoInstaller<ActiveCloseScreenController>
    {
        [SerializeField] private EventViewModelSO _openRequestEventViewModelSO;
        [SerializeField] private bool _isScreenActive;
        
        [Inject] private IScreenPresenter _screenPresenter;
        protected override ActiveCloseScreenController GetData()
        {
            return new ActiveCloseScreenController(_openRequestEventViewModelSO.GetEventViewModel(), _screenPresenter, _isScreenActive);
        }
    }
}