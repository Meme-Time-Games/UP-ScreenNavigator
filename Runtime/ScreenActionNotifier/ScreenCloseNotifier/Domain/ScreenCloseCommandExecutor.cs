using System;
using Commands.Core;

namespace ScreenNavigators.Core
{
    public class ScreenCloseCommandExecutor : ScreenActionNotifier 
    {
        private readonly ICommand _command;
        
        public ScreenCloseCommandExecutor(IScreenNavigator screenNavigator, string screenId, ICommand command) : base(screenNavigator, screenId)
        {
            _command = command;
        }

        protected override void ExecuteAction()
        {
            _command.Execute();
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