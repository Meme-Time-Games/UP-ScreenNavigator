using System;
using System.Collections.Generic;
using CrudRepository.Core;

namespace ScreenNavigators.Core
{
    public class ScreenNavigator : IScreenNavigator
    {
        private readonly ICrudRepository<string, ScreenData> _screensRepository;

        private Dictionary<string, ScreenData> _openedScreens;
        
        public Action<string> OnScreenOpened { get; set; }
        public Action<string> OnScreenClosed { get; set; }

        public ScreenNavigator(ICrudRepository<string, ScreenData> screensRepository)
        {
            _screensRepository = screensRepository;
            _openedScreens  = new Dictionary<string, ScreenData>();
        }
        
        public void AddScreen(ScreenData screenData)
        {
            string screenId = screenData.ScreenId;
            
            if (_screensRepository.IsThisContainedById(screenId))
            {
                return;
            }
            
            _screensRepository.Insert(screenId, screenData);
        }

        public void OpenScreen(string screenId)
        {
            ScreenData screenData = _screensRepository.GetById(screenId);

            if (_openedScreens.ContainsKey(screenData.ScreenId))
            {
                return;
            }
            
            if (screenData.HasToCloseAllScreensOnOpen)
                CloseAllScreens();
            
            _openedScreens.Add(screenId, screenData);
            
            OpenNestedScreens(screenData.NestedScreenIds);
            CloseScreens(screenData.ToCloseScreenIds);
            
            OnScreenOpened?.Invoke(screenData.ScreenId);
        }

        private void OpenNestedScreens(string[] nestedScreens)
        {
            foreach (var screenId in nestedScreens)
            {
                OpenScreen(screenId);
            }
        }
        
        private void CloseScreens(string[] closeScreens)
        {
            foreach (var screenId in closeScreens)
            {
                CloseScreen(screenId);
            }
        }
        
        private void CloseAllScreens()
        {
            IEnumerable<KeyValuePair<string, ScreenData>> allOpenedScreens = new List<KeyValuePair<string, ScreenData>>(_openedScreens);
            foreach (KeyValuePair<string, ScreenData> screenDataKeyValuePair in allOpenedScreens)
            {
                CloseScreen(screenDataKeyValuePair.Key);
            }
        }

        public void CloseScreen(string screenId)
        {
            ScreenData screenData = _screensRepository.GetById(screenId);
            if (!_openedScreens.ContainsKey(screenData.ScreenId))
                return;
            
            CloseNestedScreens(screenData.NestedScreenIds);
            _openedScreens.Remove(screenId);
            
            OnScreenClosed?.Invoke(screenId);
        }

        private void CloseNestedScreens(string[] nestedScreens)
        {
            foreach (string screenId in nestedScreens)
            {
                if (!_openedScreens.ContainsKey(screenId))
                    continue;
                
                ScreenData screenData = _screensRepository.GetById(screenId);
                if (screenData.IsLocked)
                    continue;
                
                CloseScreen(screenId);
            }
        }
        
        public bool IsScreenOpen(string screenId)
        {
            bool isOpen = _openedScreens.ContainsKey(screenId);
            return isOpen;
        }

#if UNITY_EDITOR
        public string[] GetOpenScreenIds()
        {
            List<KeyValuePair<string, ScreenData>> allOpenedScreens = new List<KeyValuePair<string, ScreenData>>(_openedScreens);
            string[] openedScreens = new string[allOpenedScreens.Count];
            int index = 0;
            foreach (var openedScreen in allOpenedScreens)
            {
                openedScreens[index] = openedScreen.Key;
                index++;
            }
            
            return openedScreens;
        }
#endif
    }
}