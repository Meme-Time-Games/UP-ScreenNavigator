using System;
using System.Collections.Generic;
using CrudRepository.Core;

namespace ScreenNavigators.Core
{
    public class ScreenNavigator : IScreenNavigator
    {
        private readonly ICrudRepository<string, ScreenData> _screensRepository;
        
        private Dictionary<string, ScreenData> _openedScreens = new Dictionary<string, ScreenData>();
        private List<ScreenData> _lockedScreens = new List<ScreenData>();
        
        public Action<string> OnScreenOpened { get; set; }
        public Action<string> OnScreenClosed { get; set; }

        public ScreenNavigator(ICrudRepository<string, ScreenData> screensRepository)
        {
            _screensRepository = screensRepository;
        }
        
        public void AddScreen(ScreenData screenData)
        {
            string screenId = screenData.ScreenId;
            if(_screensRepository.IsThisContainedById(screenId))
                throw new Exception("Screen with id " + screenId + " is already added.");
            
            _screensRepository.Insert(screenId, screenData);
        }

        public void OpenScreen(string screenId)
        {
            ScreenData screenData = _screensRepository.GetById(screenId);
            
            if (screenData.HasToCloseAllScreensOnOpen)
                CloseAllScreens();
            
            _openedScreens.Add(screenId, screenData);
            
            OpenNestedScreens(screenData.NestedScreens);
            CloseScreens(screenData.CloseScreens);
            
            OnScreenOpened?.Invoke(screenData.ScreenId);
        }

        private void OpenNestedScreens(ScreenData[] nestedScreens)
        {
            foreach (var screenData in nestedScreens)
            {
                OpenScreen(screenData.ScreenId);
            }
        }
        
        private void CloseScreens(ScreenData[] closeScreens)
        {
            foreach (var screenData in closeScreens)
            {
                CloseScreen(screenData.ScreenId);
            }
        }
        
        private void CloseAllScreens()
        {
            IEnumerable<KeyValuePair<string, ScreenData>> allOpenedScreens = _openedScreens;
            foreach (var screenDataKeyValuePair in allOpenedScreens)
            {
                CloseScreen(screenDataKeyValuePair.Key);
            }
        }

        public void CloseScreen(string screenId)
        {
            ScreenData screenData = _screensRepository.GetById(screenId);
            if (!_openedScreens.ContainsKey(screenData.ScreenId))
                throw new Exception("Screen with id " + screenId + " is not opened.");
            
            CloseNestedScreens(screenData.NestedScreens);
            _openedScreens.Remove(screenData.ScreenId);
        }

        private void CloseNestedScreens(ScreenData[] nestedScreens)
        {
            foreach (var screenData in nestedScreens)
            {
                if (_openedScreens.ContainsKey(screenData.ScreenId) && !screenData.IsLocked)
                    CloseScreen(screenData.ScreenId);
            }
        }
    }
}