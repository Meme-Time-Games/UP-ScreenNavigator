using Commands.Core;

namespace ScreenNavigators.Core
{
    public class ClosePresenterScreenCommand : ICommand
    {
        private readonly IScreenPresenter _screenPresenter;

        public ClosePresenterScreenCommand(IScreenPresenter screenPresenter)
        {
            _screenPresenter = screenPresenter;
        }

        public void Execute()
        {
            _screenPresenter.Close();
        }
    }
}