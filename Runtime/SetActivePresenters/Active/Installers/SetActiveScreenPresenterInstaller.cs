using DependencyInjector.Installers;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class SetActiveScreenPresenterInstaller : SingleMonoInstaller<IActiveScreenPresenter>
    {
        [Header("References")]
        [SerializeField] private GameObject _gameObjectToActivate;
        
        protected override IActiveScreenPresenter GetData()
        {
            return new SetActiveScreenPresenter(_gameObjectToActivate);
        }
    }
}