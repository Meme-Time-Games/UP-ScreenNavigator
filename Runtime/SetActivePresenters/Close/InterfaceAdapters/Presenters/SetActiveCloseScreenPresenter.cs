using UnityEngine;

namespace ScreenNavigators.Core
{
    public class SetActiveCloseScreenPresenter : ICloseScreenPresenter
    {
        private readonly GameObject _gameObject;

        public SetActiveCloseScreenPresenter(GameObject gameObject)
        {
            _gameObject = gameObject;
        }
        
        public void Close()
        {
            _gameObject.SetActive(false);
        }
    }
}