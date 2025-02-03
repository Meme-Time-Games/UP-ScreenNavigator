using System;

namespace ScreenNavigators.Core
{
    public abstract class ScreenActionNotifier : IDisposable
    {
        private readonly IScreenNavigator _screenNavigator;
        private readonly string _screenId;

        protected ScreenActionNotifier(IScreenNavigator screenNavigator, string screenId)
        {
            _screenNavigator = screenNavigator;
            _screenId = screenId;
            SubscribeToScreenNavigator(_screenNavigator, Execute);
        }
        
        protected abstract void SubscribeToScreenNavigator(IScreenNavigator screenNavigator, Action<string> executeAction);

        private void Execute(string screenId)
        {
            if(_screenId != screenId)
                return;

            ExecuteAction();
        }

        protected abstract void ExecuteAction();

        public void Dispose()
        {
            UnsubscribeToScreenNavigator(_screenNavigator, Execute);
        }

        protected abstract void UnsubscribeToScreenNavigator(IScreenNavigator screenNavigator, Action<string> executeAction);
    }
}