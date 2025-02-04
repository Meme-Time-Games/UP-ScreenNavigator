using DependencyInjector.Installers;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class SetActiveCloseScreenPresenterInstaller : SingleMonoInstaller<IScreenDeactivatorPresenter>
    {
        [Header("References")]
        [SerializeField] private GameObject _gameObjectToActivate;
        
        protected override IScreenDeactivatorPresenter GetData()
        {
            return new SetActiveScreenDeactivatorPresenter(_gameObjectToActivate);
        }
    }
}