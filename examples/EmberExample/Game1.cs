using System;
using System.IO;
using EmberExample.Graphics;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Primitives;

namespace EmberExample;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private ParticleEffect _particleEffect;
    private bool _emitOnClick = true;
    private MouseState _prevMouseState;
    private MouseState _curMouseState;

    private readonly string _contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

        Content.RootDirectory = "Content";
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
        InactiveSleepTime = TimeSpan.Zero;
        IsMouseVisible = true;
    }

    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        if (_particleEffect != null)
        {
            // Center the particle effect for the auto trigger
            _particleEffect.Position = GraphicsDevice.Viewport.Bounds.Center.ToVector2();
        }
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
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _prevMouseState = _curMouseState;
        _curMouseState = Mouse.GetState();

        if (_particleEffect != null)
        {
            _particleEffect.Update(gameTime);

            if (_emitOnClick && _curMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 start = _prevMouseState.Position.ToVector2();
                Vector2 end = _curMouseState.Position.ToVector2();
                LineSegment line = new(start, end);
                _particleEffect.Trigger(line, 0.0f);
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        if (_particleEffect != null)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_particleEffect);
            _spriteBatch.End();
        }

        DrawDebugWindow(gameTime);
    }

    private void DrawDebugWindow(GameTime gameTime)
    {
        ImGuiRenderer.BeforeLayout(gameTime);

        if (ImGui.Begin("Debug"u8))
        {
            // Buttons to choose different effect samples to show
            if (ImGui.BeginTable("##sample_buttons"u8, 4, ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn("##smoke_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                ImGui.TableSetupColumn("##spark_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                ImGui.TableSetupColumn("##ring_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                ImGui.TableSetupColumn("##loadtest_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                if (ImGui.Button("Smoke"u8, new System.Numerics.Vector2(-1, 0)))
                {
                    string fileName = Path.Combine(_contentPath, "smoke", "smoke.ember");
                    LoadParticleEffect(fileName);
                }

                ImGui.TableNextColumn();
                if (ImGui.Button("Spark"u8, new System.Numerics.Vector2(-1, 0)))
                {
                    string fileName = Path.Combine(_contentPath, "spark", "spark.ember");
                    LoadParticleEffect(fileName);
                }

                ImGui.TableNextColumn();
                if (ImGui.Button("Ring"u8, new System.Numerics.Vector2(-1, 0)))
                {
                    string fileName = Path.Combine(_contentPath, "ring", "ring.ember");
                    LoadParticleEffect(fileName);
                }

                ImGui.TableNextColumn();
                if (ImGui.Button("LoadTest"u8, new System.Numerics.Vector2(-1, 0)))
                {
                    string fileName = Path.Combine(_contentPath, "loadtest", "loadtest.ember");
                    LoadParticleEffect(fileName);
                }

                ImGui.EndTable();
            }

            ImGui.Spacing();

            if (ImGui.BeginTable("##debug_table"u8, 2, ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn("##label_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                ImGui.TableSetupColumn("##value_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.BeginDisabled(_particleEffect == null);
                ImGui.Text("Auto Trigger: ");
                ImGui.SameLine();
                ImGui.Checkbox("##auto_trigger"u8, ref _particleEffect.AutoTrigger);
                ImGui.EndDisabled();

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Emit on click");
                ImGui.SameLine();
                ImGui.Checkbox("##emit_on_click"u8, ref _emitOnClick);

                ImGui.EndTable();
            }
        }

        ImGui.End();


        // End the use interface draw
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
        _particleEffect.AutoTrigger = true;
    }
}
