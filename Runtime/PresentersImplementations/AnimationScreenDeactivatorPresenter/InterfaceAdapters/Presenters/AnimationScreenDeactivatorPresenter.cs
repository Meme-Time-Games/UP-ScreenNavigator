using System.Collections;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class AnimationScreenDeactivatorPresenter : MonoBehaviour, IScreenDeactivatorPresenter
    {
        private GameObject _gameObjectToClose;
        private Animator _animator;
        private WaitForSeconds _waitForSeconds;
        private AnimationClip _animationClip;

        public void Install(GameObject gameObjectToClose, Animator animator, AnimationClip animationClip)
        {
            _gameObjectToClose = gameObjectToClose;
            _animator = animator;
            _animationClip = animationClip;
            
            _waitForSeconds = new WaitForSeconds(animationClip.length);
        }
        
        public void Close()
        {
            _animator.Play(_animationClip.name);
            StartCoroutine(CloseAfterTime());
        }

        public IEnumerator CloseAfterTime()
        {
            yield return _waitForSeconds;
            _gameObjectToClose.SetActive(false);
        }
    }
}