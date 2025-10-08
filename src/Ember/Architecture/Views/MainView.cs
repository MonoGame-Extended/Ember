using System;

namespace Ember.Architecture.Views;

public sealed class MainView
{
    private readonly MainMenuBarView _mainMenuBarView;
    private readonly ToolbarView _toolbarView;
    private readonly DockSpaceView _dockSpaceView;
    private readonly ParticleEffectView _particleEffectView;

    public MainView(EditorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _mainMenuBarView = new(context);
        _toolbarView = new(context);
        _dockSpaceView = new();
        _particleEffectView = new(context);
    }

    public void Draw()
    {
        _mainMenuBarView.Draw();
        _toolbarView.Draw();
        _dockSpaceView.Draw();
        _particleEffectView.Draw();
    }
}
