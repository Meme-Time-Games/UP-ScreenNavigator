using System.Collections;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class AnimationScreenDeactivatorPresenter : MonoBehaviour, IScreenDeactivatorPresenter
    {
        private GameObject _gameObjectToClose;
        private WaitForSeconds _waitForSeconds;
        
        public void Install(GameObject gameObjectToClose, float animationTime)
        {
            _gameObjectToClose = gameObjectToClose;
            _waitForSeconds = new WaitForSeconds(animationTime);
        }
        
        public void Close()
        {
            StartCoroutine(CloseAfterTime());
        }

        public IEnumerator CloseAfterTime()
        {
            yield return _waitForSeconds;
            _gameObjectToClose.SetActive(false);
        }
    }
}