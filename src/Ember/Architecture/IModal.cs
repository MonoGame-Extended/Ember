namespace Ember.Architecture;

public interface IModal : IView
{
    bool IsOpen { get; }
    void Open();
    void Close();
}
