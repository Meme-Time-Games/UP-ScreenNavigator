using System;
using Commands.Core;

namespace ScreenNavigators.Core
{
    public class AddMultipleScreenDataCommand : ICommand, IDisposable
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
            foreach (ScreenData screenData in _screenData)
            {
                _screenNavigator.AddScreen(screenData);
            }
        }

        public void Dispose()
        {
            foreach (ScreenData screenData in _screenData)
            {
                _screenNavigator.CloseScreen(screenData.ScreenId);
            } 
        }
    }
}