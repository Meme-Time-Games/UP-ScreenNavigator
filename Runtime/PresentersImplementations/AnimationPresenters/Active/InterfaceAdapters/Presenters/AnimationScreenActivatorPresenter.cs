using UnityEngine;

namespace ScreenNavigators.Core
{
    public class AnimationScreenActivatorPresenter : IScreenActivatorPresenter
    {
        private readonly GameObject _gameObjectToClose;
        private readonly Animator _animator;
        private readonly string _triggerName;

        public AnimationScreenActivatorPresenter(GameObject gameObjectToClose, Animator animator, string triggerName)
        {
            _gameObjectToClose = gameObjectToClose;
            _animator = animator;
            _triggerName = triggerName;
        }

        public void Activate()
        {
            _animator.SetTrigger(_triggerName);
            _gameObjectToClose.SetActive(true);
        }
    }
}