using Commands.Core;

namespace ScreenNavigators.Core
{
    public class AddMultipleScreenDataCommand : ICommand
    {
        private readonly IScreenNavigator _screenNavigator;
        private readonly ScreenData[] _screenData;
        private readonly bool _addOnInit;

        public AddMultipleScreenDataCommand(IScreenNavigator screenNavigator, ScreenData[] screenData, bool addOnInit)
        {
            _screenNavigator = screenNavigator;
            _screenData = screenData;
            _addOnInit = addOnInit;

            if(_addOnInit)
                Execute();
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