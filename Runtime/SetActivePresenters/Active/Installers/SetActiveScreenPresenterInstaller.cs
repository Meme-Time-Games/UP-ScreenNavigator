using DependencyInjector.Installers;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class SetActiveScreenPresenterInstaller : SingleMonoInstaller<IScreenActivatorPresenter>
    {
        [Header("References")]
        [SerializeField] private GameObject _gameObjectToActivate;
        
        protected override IScreenActivatorPresenter GetData()
        {
            return new SetScreenActivatorPresenter(_gameObjectToActivate);
        }
    }
}