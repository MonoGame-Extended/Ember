using System;
using System.Diagnostics;
using System.IO;
using EmberExample.Graphics;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Primitives;

namespace EmberExample;

public record SampleEffect(string Name, string Path);

public class Game1 : Game
{

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private ParticleEffect _particleEffect;
    private bool _emitOnClick = true;
    private MouseState _prevMouseState;
    private MouseState _curMouseState;
    private readonly Stopwatch _updateTimer = new();
    private readonly Stopwatch _drawTimer = new();

    private readonly string _contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
    private SampleEffect[] _sampleEffects;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

        Content.RootDirectory = "Content";
        InactiveSleepTime = TimeSpan.Zero;
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        ImGuiRenderer.Initialize(this);

        // Load smoke effect by default
        string fileName = Path.Combine(_contentPath, "smoke", "smoke.ember");
        LoadParticleEffect(fileName);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _sampleEffects =
        [
            new SampleEffect("Smoke", Path.Combine(_contentPath, "smoke", "smoke.ember")),
            new SampleEffect("Spark", Path.Combine(_contentPath, "spark", "spark.ember")),
            new SampleEffect("Ring", Path.Combine(_contentPath, "ring", "ring.ember")),
            new SampleEffect("Load Test", Path.Combine(_contentPath, "loadtest", "loadtest.ember")),

            new SampleEffect("Fireball", Path.Combine(_contentPath, "fireball", "fireball.ember")),
        ];
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _prevMouseState = _curMouseState;
        _curMouseState = Mouse.GetState();

        _updateTimer.Restart();
        if (_particleEffect != null)
        {
            if (_emitOnClick && _curMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 start = _prevMouseState.Position.ToVector2();
                Vector2 end = _curMouseState.Position.ToVector2();
                LineSegment line = new(start, end);
                _particleEffect.Trigger(line, 0.0f);
            }

            _particleEffect.Update(gameTime);
        }
        _updateTimer.Stop();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _drawTimer.Restart();
        if (_particleEffect != null)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);
            _spriteBatch.Draw(_particleEffect);
            _spriteBatch.End();
        }
        _drawTimer.Stop();

        DrawDebugWindow(gameTime);
    }

    private void DrawDebugWindow(GameTime gameTime)
    {
        ImGuiRenderer.BeforeLayout(gameTime);

        ImGui.SetNextWindowPos(System.Numerics.Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowSize(new(450, 200), ImGuiCond.Always);

        if (ImGui.Begin("Debug"u8, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar))
        {
            ImGuiStylePtr stylePtr = ImGui.GetStyle();
            System.Numerics.Vector2 buttonSize = new(120, 0);
            float windowVisibleX2 = ImGui.GetCursorScreenPos().X + ImGui.GetContentRegionAvail().X;
            for (int i = 0; i < _sampleEffects.Length; i++)
            {
                SampleEffect sampleEffect = _sampleEffects[i];

                ImGui.PushID(i);

                if (ImGui.Button(sampleEffect.Name, buttonSize))
                {
                    LoadParticleEffect(sampleEffect.Path);
                }

                float lastButtonX2 = ImGui.GetItemRectMax().X;
                float nextButtonX2 = lastButtonX2 + stylePtr.ItemSpacing.X + buttonSize.X;

                if (i + 1 < _sampleEffects.Length && nextButtonX2 < windowVisibleX2)
                {
                    ImGui.SameLine();
                }

                ImGui.PopID();
            }

            ImGui.Spacing();

            // Options and info
            if (ImGui.BeginTable("##debug_table"u8, 2, ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn("##label_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                ImGui.TableSetupColumn("##value_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.BeginDisabled(_particleEffect == null);
                ImGui.Text("Auto Trigger: ");
                ImGui.TableNextColumn();
                ImGui.Checkbox("##auto_trigger"u8, ref _particleEffect.AutoTrigger);
                ImGui.EndDisabled();

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Emit on click");
                ImGui.TableNextColumn();
                ImGui.Checkbox("##emit_on_click"u8, ref _emitOnClick);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Particles: ");
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text(string.Format("{0:n0}", _particleEffect.ActiveParticles));

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text("FPS: ");
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                float frameRate = ImGui.GetIO().Framerate;
                ImGui.Text(string.Format("{0:n2} ", frameRate));

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Update: ");
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                float updateTime = (float)_updateTimer.Elapsed.TotalSeconds;
                ImGui.Text(string.Format("{0:n4}s ({1,8:P2}%)", updateTime, updateTime / 0.01666666f));

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Draw: ");
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                float drawTime = (float)_drawTimer.Elapsed.TotalSeconds;
                ImGui.Text(string.Format("{0:n4}s ({1,8:P2}%)", drawTime, drawTime / 0.01666666f));

                ImGui.EndTable();
            }
        }

        ImGui.End();

        ImGuiRenderer.AfterLayout();
    }

    private void LoadParticleEffect(string fileName)
    {
        if (_particleEffect != null)
        {
            _particleEffect.Dispose();
            _particleEffect = null;
        }

        using ParticleEffectReader reader = new(fileName, Content);
        _particleEffect = reader.ReadParticleEffect();
        _particleEffect.Position = GraphicsDevice.Viewport.Bounds.Center.ToVector2();
        _particleEffect.AutoTrigger = false;
    }
}
