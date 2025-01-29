using DependencyInjector.Installers;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class SetActiveCloseScreenPresenterInstaller : SingleMonoInstaller<ICloseScreenPresenter>
    {
        [Header("References")]
        [SerializeField] private GameObject _gameObjectToActivate;
        
        protected override ICloseScreenPresenter GetData()
        {
            return new SetActiveCloseScreenPresenter(_gameObjectToActivate);
        }
    }
}