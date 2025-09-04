// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Ember.Graphics;
using Ember.UI;
using Ember.UI.Modals;
using Ember.UI.Styling;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Primitives;


namespace Ember;

public class EmberEditor : Game
{
    private static EmberEditor s_instance;
    private static readonly CompositeFormat s_windowTitle = CompositeFormat.Parse("Ember: {0} | {1:F3} ms/Frame | {2:F1} FPS");
    private static readonly string s_version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    private static float s_frameRate;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Input
    private static MouseState s_previousMouseState;
    private static MouseState s_currentMouseState;

    public EmberEditor()
    {
        s_instance = this;
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        EmberContext.CenterParticleEffect();
    }

    protected override unsafe void Initialize()
    {
        base.Initialize();


        // Initialize ImGui
        ImGuiRenderer.Initialize(this);

        // Enable docking
        ImGuiIOPtr ioPtr = ImGui.GetIO();
        ioPtr.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        // Load Fonts
        Fonts.Load();

        // Set the styling and theme for ImGui
        CatppuccinTheme.Apply(CatppuccinVariant.Mocha);

        // Initialize the editor context
        EmberContext.Initialize(this);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        s_previousMouseState = s_currentMouseState;
        s_currentMouseState = Mouse.GetState();

        // Emit particles on click
        if (EmberContext.ParticleEffect is ParticleEffect particleEffect)
        {
            ImGuiIOPtr ioPtr = ImGui.GetIO();

            // Only emit if the click happens somewhere that ImGui is not
            // actively capturing mouse inputs
            if (!ioPtr.WantCaptureMouse && s_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                XnaVec2 start = s_previousMouseState.Position.ToVector2();
                XnaVec2 end = s_currentMouseState.Position.ToVector2();
                LineSegment lineSegment = new(start, end);
                particleEffect.Trigger(lineSegment, 0.0f);
            }

            // Update the particle effect
            particleEffect.Update(gameTime);
        }
    }

    protected override unsafe void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(EmberContext.ClearColor);

        // Draw the particle effect
        if (EmberContext.ParticleEffect is ParticleEffect particleEffect)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointWrap, blendState: BlendState.AlphaBlend);
            _spriteBatch.Draw(particleEffect);
            _spriteBatch.End();
        }

        // Draw the user interface
        ImGuiRenderer.BeforeLayout(gameTime);

        MainMenuBar.Draw();
        PreferencesWindow.Draw();
        DockSpace.Draw();

        if (EmberContext.ParticleEffect != null)
        {
            PropertiesWindow.Draw();
            ModifiersWindow.Draw();
        }

        CreateNewProjectModal.Draw();
        OpenProjectModal.Draw();
        SelectTextureModal.Draw();
        OverwriteExistingFileModal.Draw();
        ChooseModifierModal.Draw();
        ChooseInterpolatorModal.Draw();
        UnsavedChangesModal.Draw();

        // End the use interface draw
        ImGuiRenderer.AfterLayout();

        s_frameRate = ImGui.GetIO().Framerate;

        Window.Title = string.Format(CultureInfo.InvariantCulture, s_windowTitle, s_version, 1000.0f / s_frameRate, s_frameRate);
    }
}
