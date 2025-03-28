using System.Collections;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class AnimationScreenDeactivatorPresenter : MonoBehaviour, IScreenDeactivatorPresenter
    {
        private GameObject _gameObjectToClose;
        private Animator _animator;
        private float _animationDuration;
        private string _triggerName;
        
        private WaitForSeconds _waitForSeconds;

        public void Install(GameObject gameObjectToClose, Animator animator, float animationDuration, string triggerName)
        {
            _gameObjectToClose = gameObjectToClose;
            _animator = animator;
            
            _waitForSeconds = new WaitForSeconds(animationDuration);
            _triggerName = triggerName;
        }
        
        public void Close()
        {
            _animator.SetTrigger(_triggerName);
            StartCoroutine(CloseAfterTime());
        }

        private IEnumerator CloseAfterTime()
        {
            yield return _waitForSeconds;
            _gameObjectToClose.SetActive(false);
        }
    }
}