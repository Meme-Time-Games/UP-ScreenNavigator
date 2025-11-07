using UnityEngine;

namespace ScreenNavigators.Core
{
    [CreateAssetMenu(fileName = "-ScreenDataSO", menuName = "ScriptableObjects/ScreenNavigator/Data/ScreenDataSO")]
    public class ScreenDataSO : ScriptableObject
    {
        [Header("Config")]
        [SerializeField] private string _screenId;
        [SerializeField] private bool _isLocked;
        [SerializeField] private bool _hasToCloseAllScreensOnOpen;

        [Header("References")]
        [SerializeField] private ScreenDataSO[] _nestedScreens;
        [SerializeField] private ScreenDataSO[] _toCloseScreens;

        public string ScreenId => _screenId;

        public ScreenData GetScreenData()
        {
            string[] nestedScreenIds = new string[_nestedScreens.Length];
            for (int i = 0; i < _nestedScreens.Length; i++)
            {
                nestedScreenIds[i] = _nestedScreens[i].ScreenId;
            }
            
            string[] closeScreenIds = new string[_toCloseScreens.Length];
            for (int i = 0; i < _toCloseScreens.Length; i++)
            {
                closeScreenIds[i] = _toCloseScreens[i].ScreenId;
            }
            
            return new ScreenData(_screenId, nestedScreenIds, closeScreenIds, _isLocked, _hasToCloseAllScreensOnOpen);
        }
    }
}