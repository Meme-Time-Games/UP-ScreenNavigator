using UnityEngine;

namespace ScreenNavigators.Core
{
    public class SetActiveScreenDeactivatorPresenter : IScreenDeactivatorPresenter
    {
        private readonly GameObject _gameObject;

        public SetActiveScreenDeactivatorPresenter(GameObject gameObject)
        {
            _gameObject = gameObject;
        }
        
        public void Close()
        {
            _gameObject.SetActive(false);
        }
    }
}