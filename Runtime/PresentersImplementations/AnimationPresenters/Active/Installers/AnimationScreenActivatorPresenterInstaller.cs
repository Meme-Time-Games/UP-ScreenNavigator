using DependencyInjector.Installers;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class AnimationScreenActivatorPresenterInstaller : SingleMonoInstaller<IScreenActivatorPresenter>
    {
        [Header("References")]
        [SerializeField] private GameObject _gameObjectToOpen;
        [SerializeField] private Animator _animator;
        
        [Header("Config")]
        [SerializeField] private string _triggerName; 
        
        protected override IScreenActivatorPresenter GetData()
        {
            return new AnimationScreenActivatorPresenter(_gameObjectToOpen, _animator, _triggerName);
        }
    }
}