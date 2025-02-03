namespace ScreenNavigators.Core
{
    public class ActiveCloseScreenPresenter : IScreenPresenter
    {
        private readonly IScreenActivatorPresenter _screenActivatorPresenter;
        private readonly IScreenDeactivatorPresenter _screenDeactivatorPresenter;
        private readonly IScreenPresenter _screenPresenter;

        public ActiveCloseScreenPresenter(IScreenActivatorPresenter screenActivatorPresenter, IScreenDeactivatorPresenter screenDeactivatorPresenter, IScreenPresenter screenPresenter)
        {
            _screenActivatorPresenter = screenActivatorPresenter;
            _screenDeactivatorPresenter = screenDeactivatorPresenter;
            _screenPresenter = screenPresenter;
        }

        public void Present()
        {
            _screenActivatorPresenter.Activate();
            _screenPresenter.Present();
        }

        public void Close()
        {
            _screenDeactivatorPresenter.Close();
        }
    }
}