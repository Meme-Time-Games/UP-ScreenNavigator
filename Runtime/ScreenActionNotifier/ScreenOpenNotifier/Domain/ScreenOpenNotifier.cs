using System;

namespace ScreenNavigators.Core
{
    public class ScreenOpenNotifier : ScreenActionNotifier 
    {
        private readonly IScreenPresenter _screenPresenter;
        
        public ScreenOpenNotifier(IScreenNavigator screenNavigator, string screenIdToOpen, IScreenPresenter screenPresenter) : base(screenNavigator, screenIdToOpen)
        {
            _screenPresenter = screenPresenter;
        }

        protected override void ExecuteAction()
        {
            _screenPresenter.Present();
        }

        protected override void SubscribeToScreenNavigator(IScreenNavigator screenNavigator, Action<string> executeAction)
        {
            screenNavigator.OnScreenOpened += executeAction;
        }

        protected override void UnsubscribeToScreenNavigator(IScreenNavigator screenNavigator, Action<string> executeAction)
        {
            screenNavigator.OnScreenOpened -= executeAction;
        }
    }
}