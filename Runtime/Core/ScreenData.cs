using System.Collections.Generic;
using CrudRepository.Core;

namespace ScreenNavigators.Core
{
    public class DefaultScreenData : ScreenData
    {
        public DefaultScreenData(string screenName, ScreenData[] nestedScreens) : base(screenName, nestedScreens)
        {
        }
    }
    
    public abstract class ScreenData
    {
        private readonly string _screenName;
        private readonly ScreenData[] _nestedScreens;
        
        public string ScreenName => _screenName;
        public ScreenData[] NestedScreens => _nestedScreens;

        protected ScreenData(string screenName, ScreenData[] nestedScreens)
        {
            _screenName = screenName;
            _nestedScreens = nestedScreens;
        }
    }

    public interface IScreen
    {
        ScreenData ScreenData { get; }

        void Enable();
        void Disable();

        void Select();
        void Deselect();

        void Dispose();
    }

    public class NullScreen : IScreen
    {
        private readonly ScreenData _screenData;
        
        public ScreenData ScreenData => _screenData;
        
        public NullScreen(ScreenData screenData)
        {
            _screenData = screenData;
        }

        public void Enable() { }
        public void Disable() { }
        public void Select() { }
        public void Deselect() { }
        public void Dispose() { }
    }

    public interface IScreenNavigator
    {
        void OpenScreen(ScreenData screenData);
        void CloseScreen(string screenName);
        
        void GoToPreviousScreen();
        
        void AddScreen(string screenName, IScreen screen);
        void RemoveScreen(string screenName);
        
        void SelectScreen(string screenName);
        void DeselectScreen(string screenName);
        void DeselectCurrentScreen();
    }

    public class ScreenNavigator : IScreenNavigator
    {
        private readonly ICrudRepository<string, IScreen> _screensRepository;
        
        private IScreen _currentScreen;
        private Queue<ScreenData> _screensOpenedQueue;

        public ScreenNavigator(ICrudRepository<string, IScreen> screensRepository)
        {
            _screensRepository = screensRepository;

            _currentScreen = new NullScreen(new DefaultScreenData("NullScreen", null));
            _screensOpenedQueue = new Queue<ScreenData>();
        }

        public void OpenScreen(ScreenData screenData)
        {
            OpenScreen(screenData, false);
        }
        
        private void OpenScreen(ScreenData screenData, bool isPreviousScreen = false)
        {
            //TODO: Add transition system
            _currentScreen.Disable();
            if(!isPreviousScreen)
                _screensOpenedQueue.Enqueue(_currentScreen.ScreenData);
         
            //TODO: Check if the screen exists
            //TODO: Add the option of spawning a screen
            IScreen screen = _screensRepository.GetById(screenData.ScreenName);
            screen.Enable();
            
            _currentScreen = screen;
        }

        public void CloseScreen(string screenName)
        {
            IScreen screen = _screensRepository.GetById(screenName);
            screen.Disable();
        }

        public void GoToPreviousScreen()
        {
            ScreenData previousScreenName = _screensOpenedQueue.Dequeue();
            OpenScreen(previousScreenName, true);
        }

        public void AddScreen(string screenName, IScreen screen)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveScreen(string screenName)
        {
            throw new System.NotImplementedException();
        }

        public void SelectScreen(string screenName)
        {
            throw new System.NotImplementedException();
        }

        public void DeselectScreen(string screenName)
        {
            throw new System.NotImplementedException();
        }

        public void DeselectCurrentScreen()
        {
            throw new System.NotImplementedException();
        }
    }
}