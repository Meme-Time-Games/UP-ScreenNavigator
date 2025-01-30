using System;

namespace ScreenNavigators.Core
{
    public abstract class ScreenActionNotifier : IDisposable
    {
        private readonly IScreenNavigator _screenNavigator;
        private readonly string _screenIdToOpen;

        protected ScreenActionNotifier(IScreenNavigator screenNavigator, string screenIdToOpen)
        {
            _screenNavigator = screenNavigator;
            _screenIdToOpen = screenIdToOpen;
            SubscribeToScreenNavigator(_screenNavigator, Execute);
        }

        private void Execute(string screenId)
        {
            if(_screenIdToOpen != screenId)
                return;

            ExecuteAction();
        }

        protected abstract void ExecuteAction();

        protected abstract void SubscribeToScreenNavigator(IScreenNavigator screenNavigator, Action<string> executeAction);

        public void Dispose()
        {
            UnsubscribeToScreenNavigator(_screenNavigator, Execute);
        }

        protected abstract void UnsubscribeToScreenNavigator(IScreenNavigator screenNavigator, Action<string> executeAction);
    }
}