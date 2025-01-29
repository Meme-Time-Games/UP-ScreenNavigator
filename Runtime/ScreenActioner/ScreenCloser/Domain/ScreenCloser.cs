using System;

namespace ScreenNavigators.Core
{
    public class ScreenCloser : ScreenActioner 
    {
        private readonly IScreenPresenter _screenPresenter;
        
        public ScreenCloser(IScreenNavigator screenNavigator, string screenIdToOpen, IScreenPresenter screenPresenter) : base(screenNavigator, screenIdToOpen)
        {
            _screenPresenter = screenPresenter;
        }

        protected override void ExecuteAction()
        {
            _screenPresenter.Close();
        }

        protected override void SubscribeToScreenNavigator(IScreenNavigator screenNavigator, Action<string> executeAction)
        {
            screenNavigator.OnScreenClosed += executeAction;
        }

        protected override void UnsubscribeToScreenNavigator(IScreenNavigator screenNavigator, Action<string> executeAction)
        {
            screenNavigator.OnScreenClosed -= executeAction;
        }
    }
}