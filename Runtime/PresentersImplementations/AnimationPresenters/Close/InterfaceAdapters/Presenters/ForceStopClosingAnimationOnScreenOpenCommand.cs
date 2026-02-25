using System;

namespace ScreenNavigators.Core
{
    public class ForceStopClosingAnimationOnScreenOpenCommand : IDisposable
    {
        private readonly IScreenNavigator _screenNavigator;
        private readonly string _screenName;
        private readonly AnimationScreenDeactivatorPresenter _animationScreenDeactivatorPresenter;

        public ForceStopClosingAnimationOnScreenOpenCommand(IScreenNavigator screenNavigator, string screenName, AnimationScreenDeactivatorPresenter animationScreenDeactivatorPresenter)
        {
            _screenNavigator = screenNavigator;
            _screenName = screenName;
            _animationScreenDeactivatorPresenter = animationScreenDeactivatorPresenter;

            _screenNavigator.OnScreenOpened += ForceStop;
        }

        private void ForceStop(string screenName)
        {
            if (screenName != _screenName)
                return;
            
            _animationScreenDeactivatorPresenter.StopClose();
        }

        public void Dispose()
        {
            _screenNavigator.OnScreenOpened -= ForceStop;
        }
    }
}