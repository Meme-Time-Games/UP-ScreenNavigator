namespace ScreenNavigators.Editors
{
    public interface IScreenMapTab
    {
        string Title { get; }
        void Draw(ScreenMapContext context);
    }
}
