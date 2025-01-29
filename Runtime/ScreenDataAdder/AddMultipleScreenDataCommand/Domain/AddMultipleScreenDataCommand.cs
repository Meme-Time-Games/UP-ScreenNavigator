using Commands.Core;

namespace ScreenNavigators.Core
{
    public class AddMultipleScreenDataCommand : ICommand
    {
        private readonly IScreenNavigator _screenNavigator;
        private readonly ScreenData[] _screenData;

        public AddMultipleScreenDataCommand(IScreenNavigator screenNavigator, ScreenData[] screenData)
        {
            _screenNavigator = screenNavigator;
            _screenData = screenData;
        }

        public void Execute()
        {
            foreach (var screenData in _screenData)
            {
                _screenNavigator.AddScreen(screenData);
            }
        }
    }
}