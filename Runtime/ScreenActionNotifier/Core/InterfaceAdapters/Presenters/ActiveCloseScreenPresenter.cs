namespace ScreenNavigators.Core
{
    public class ActiveCloseScreenPresenter : IScreenPresenter
    {
        private readonly IActiveScreenPresenter _activeScreenPresenter;
        private readonly ICloseScreenPresenter _closeScreenPresenter;
        private readonly IScreenPresenter _screenPresenter;

        public ActiveCloseScreenPresenter(IActiveScreenPresenter activeScreenPresenter, ICloseScreenPresenter closeScreenPresenter, IScreenPresenter screenPresenter)
        {
            _activeScreenPresenter = activeScreenPresenter;
            _closeScreenPresenter = closeScreenPresenter;
            _screenPresenter = screenPresenter;
        }

        public void Present()
        {
            _activeScreenPresenter.Active();
            _screenPresenter.Present();
        }

        public void Close()
        {
            _closeScreenPresenter.Close();
        }
    }
}