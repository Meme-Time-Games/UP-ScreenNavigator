using System;
using System.Collections;
using MVVM.Core;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class AnimationScreenDeactivatorPresenter : MonoBehaviour, IScreenDeactivatorPresenter, IDisposable
    {
        private GameObject _gameObjectToClose;
        private Animator _animator;
        private float _animationDuration;
        private string _triggerName;
        private IEventViewModel _onScreenOpenedEventViewModel;
        
        private WaitForSeconds _waitForSeconds;
        private bool _isClosing;

        public void Install(GameObject gameObjectToClose, Animator animator, float animationDuration, string triggerName,
            IEventViewModel onScreenOpenedEventViewModel)
        {
            _gameObjectToClose = gameObjectToClose;
            _animator = animator;
            
            _waitForSeconds = new WaitForSeconds(animationDuration);
            _triggerName = triggerName;
            
            _onScreenOpenedEventViewModel = onScreenOpenedEventViewModel;
            _onScreenOpenedEventViewModel.OnEventRaised += StopClose;
        }

        private void StopClose()
        {
            if(_isClosing) 
                StopAllCoroutines();
            
            _isClosing = false;
        }

        public void Close()
        {
            StopAllCoroutines();
            
            _isClosing = true;
            
            _animator.SetTrigger(_triggerName);
            StartCoroutine(CloseAfterTime());
        }

        private IEnumerator CloseAfterTime()
        {
            yield return _waitForSeconds;
            
            if(!_isClosing)
                yield break;
            
            _gameObjectToClose.SetActive(false);
            _isClosing = false;
        }

        public void Dispose()
        {
            _onScreenOpenedEventViewModel.OnEventRaised -= StopClose;
        }
    }
}