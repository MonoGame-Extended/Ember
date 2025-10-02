using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.Architecture;

public sealed class EditorContext
{
    private readonly Dictionary<object, bool> _lockStates = [];
    private readonly Dictionary<string, Texture2D> _textureCache = [];

    // Core services
    public Game Game { get; private set; }
    public GraphicsDevice GraphicsDevice { get; private set; }
    public ContentManager ContentManager { get; private set; }

    // Project State
    public string ProjectName { get; private set; } = string.Empty;
    public string ProjectDirectory { get; private set; } = string.Empty;
    public string ProjectFilePath { get; private set; } = string.Empty;
    public bool HasUnsavedChanged { get; set; }

    // Particle System State
    public ParticleEffect ParticleEffect { get; private set; }
    public ParticleEmitter SelectedParticleEmitter { get; private set; }
    public int SelectedParticleEmitterIndex { get; private set; }
    public Modifier SelectedModifier { get; private set; }
    public int SelectedModifierIndex { get; private set; }
    public Interpolator SelectedInterpolator { get; private set; }
    public List<Interpolator> CurrentInterpolators { get; private set; } = [];
    public int SelectedInterpolatorIndex { get; private set; } = -1;

    // Display Settings
    public XnaColor ClearColor { get; set; } = XnaColor.CornflowerBlue;

    public EditorContext(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        Game = game;
        GraphicsDevice = game.GraphicsDevice;
        ContentManager = game.Content;
    }







}
