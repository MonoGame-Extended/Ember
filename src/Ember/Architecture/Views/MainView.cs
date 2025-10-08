using System;

namespace Ember.Architecture.Views;

public sealed class MainView
{
    private readonly EditorContext _context;
    private readonly MainMenuBarView _mainMenuBarView;
    private readonly ToolbarView _toolbarView;
    private readonly DockSpaceView _dockSpaceView;
    private readonly ParticleEffectView _particleEffectView;
    private readonly ModifiersView _modifiersView;

    public MainView(EditorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _mainMenuBarView = new(_context);
        _toolbarView = new(_context);
        _dockSpaceView = new();
        _particleEffectView = new(_context);
        _modifiersView = new(_context);
    }

    public void Draw()
    {
        _mainMenuBarView.Draw();

        if (_context.ParticleEffect != null)
        {
            _toolbarView.Draw();
            _dockSpaceView.Draw();
            _particleEffectView.Draw();
            _modifiersView.Draw();
        }
    }
}
