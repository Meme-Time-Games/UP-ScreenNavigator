using DependencyInjector.Installers;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class AnimationScreenDeactivatorPresenterInstaller : SingleMonoInstaller<IScreenDeactivatorPresenter>
    {
        [Header("References")]
        [SerializeField] private GameObject _gameObjectToClose;
        [SerializeField] private Animator _animator;
        
        [Header("Config")]
        [SerializeField] private AnimationClip _animationClip;
        [SerializeField] private string _triggerName; 
        
        protected override IScreenDeactivatorPresenter GetData()
        {
            GameObject animationScreenDeactivatorPresenterGameObject = new GameObject("AnimationScreenDeactivatorPresenter");
            animationScreenDeactivatorPresenterGameObject.transform.parent = gameObject.transform;
            AnimationScreenDeactivatorPresenter animationScreenDeactivatorPresenter = animationScreenDeactivatorPresenterGameObject.AddComponent<AnimationScreenDeactivatorPresenter>();
            animationScreenDeactivatorPresenter.Install(_gameObjectToClose, _animator, _animationClip.length, _triggerName);
            
            return animationScreenDeactivatorPresenter;
        }
    }
}