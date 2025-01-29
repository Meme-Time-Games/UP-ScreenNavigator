using Commands.Core;

namespace ScreenNavigators.Core
{
    public class CloseScreenCommand : ICommand
    {
        private readonly IScreenNavigator _screenNavigator;
        private readonly string _screenId;

        public CloseScreenCommand(IScreenNavigator screenNavigator, string screenId)
        {
            _screenNavigator = screenNavigator;
            _screenId = screenId;
        }

        public void Execute()
        {
            _screenNavigator.CloseScreen(_screenId);
        }
    }
}