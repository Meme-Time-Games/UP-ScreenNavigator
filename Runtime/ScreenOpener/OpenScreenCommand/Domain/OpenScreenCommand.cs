using Commands.Core;

namespace ScreenNavigators.Core
{
    public class OpenScreenCommand : ICommand
    {
        private readonly IScreenNavigator _screenNavigator;
        private readonly string _screenId;

        public OpenScreenCommand(IScreenNavigator screenNavigator, string screenId)
        {
            _screenNavigator = screenNavigator;
            _screenId = screenId;
        }

        public void Execute()
        {
            _screenNavigator.OpenScreen(_screenId);
        }
    }
}