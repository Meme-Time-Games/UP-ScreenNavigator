using UnityEngine;

namespace ScreenNavigators.Core
{
    public class SetScreenActivatorPresenter : IScreenActivatorPresenter
    {
        private readonly GameObject _gameObject;

        public SetScreenActivatorPresenter(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public void Activate()
        {
            _gameObject.SetActive(true);
        }
    }
}