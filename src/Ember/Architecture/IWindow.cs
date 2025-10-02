namespace Ember.Architecture;

public interface IWindow : IView
{
    public string WindowTitle { get; }
    public bool IsDocked { get; set; }
}
