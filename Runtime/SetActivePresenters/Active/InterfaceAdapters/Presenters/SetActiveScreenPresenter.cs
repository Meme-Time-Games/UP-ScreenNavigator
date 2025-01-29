using UnityEngine;

namespace ScreenNavigators.Core
{
    public class SetActiveScreenPresenter : IActiveScreenPresenter
    {
        private readonly GameObject _gameObject;

        public SetActiveScreenPresenter(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public void Active()
        {
            _gameObject.SetActive(true);
        }
    }
}