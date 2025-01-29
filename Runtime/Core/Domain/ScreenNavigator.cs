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
            
            OpenNestedScreens(screenData.NestedScreenIds);
            CloseScreens(screenData.CloseScreenIds);
            
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
            foreach (var screenDataKeyValuePair in allOpenedScreens)
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
            foreach (var screenId in nestedScreens)
            {
                if (!_openedScreens.ContainsKey(screenId))
                    continue;
                
                ScreenData screenData = _screensRepository.GetById(screenId);
                if (screenData.IsLocked)
                    continue;
                
                CloseScreen(screenId);
            }
        }
    }
}