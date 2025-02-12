using Commands.Core;

namespace ScreenNavigators.Core
{
    public class PresentScreenCommand : ICommand
    {
        private readonly IScreenPresenter _screenPresenter;

        public PresentScreenCommand(IScreenPresenter screenPresenter)
        {
            _screenPresenter = screenPresenter;
        }

        public void Execute()
        {
            _screenPresenter.Present();
        }
    }
}