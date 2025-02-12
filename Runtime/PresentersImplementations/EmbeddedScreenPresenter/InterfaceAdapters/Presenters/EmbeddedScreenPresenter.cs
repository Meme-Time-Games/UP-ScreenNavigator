namespace ScreenNavigators.Core
{
    public abstract class EmbeddedScreenPresenter<TScreenPresenter> : IScreenPresenter
    {
        private readonly TScreenPresenter _screenPresenter;

        public EmbeddedScreenPresenter(TScreenPresenter screenPresenter)
        {
            _screenPresenter = screenPresenter;
        }

        protected abstract void PresentScreen(TScreenPresenter screenPresenter);

        public void Present()
        {
            PresentScreen(_screenPresenter);
        }

        public void Close()
        {
            CloseScreen(_screenPresenter);
        }

        protected virtual void CloseScreen(TScreenPresenter screenPresenter)
        {
        }
    }
}