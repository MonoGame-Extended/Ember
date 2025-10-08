using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Data;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.Architecture;

public sealed class EditorContext : IDisposable
{
    private readonly Dictionary<object, bool> _locks = [];
    private readonly Dictionary<string, Texture2D> _textureCache = [];
    private readonly Game _game;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ContentManager _contentManager;

    private float _baseFontSize = 16.0f;
    private float _fontScaleMain = 1.0f;

    /// <summary>
    /// Gets the current particle effect being edited.
    /// </summary>
    public ParticleEffect ParticleEffect { get; private set; }

    /// <summary>
    /// Gets the currently selected particle emitter.
    /// </summary>
    public ParticleEmitter SelectedEmitter { get; private set; }

    /// <summary>
    /// Gets the index of the currently selected particle emitter
    /// </summary>
    public int SelectedEmitterIndex { get; private set; } = -1;

    /// <summary>
    /// Gets the currently selected modifier.
    /// </summary>
    public Modifier SelectedModifier { get; private set; }

    /// <summary>
    /// Gets the index of the currently selected modifier.
    /// </summary>
    public int SelectedModifierIndex { get; private set; } = -1;

    /// <summary>
    /// Gets the currently selected interpolator.
    /// </summary>
    public Interpolator SelectedInterpolator { get; private set; }

    /// <summary>
    /// Gets the index of the currently selected interpolator.
    /// </summary>
    public int SelectedInterpolatorIndex { get; private set; } = -1;

    /// <summary>
    /// Gets the name of the current project.
    /// </summary>
    public string ProjectName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the directory path of the current project.
    /// </summary>
    public string ProjectDirectory { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the file path of the current project.
    /// </summary>
    public string ProjectFilePath { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the project has unsaved changes.
    /// </summary>
    public bool HasUnsavedChanges { get; set; }

    /// <summary>
    /// Gets a value indicating whether a project is currently open.
    /// </summary>
    public bool IsProjectOpen => ParticleEffect != null;

    /// <summary>
    /// Gets a value indicating whether the project playback is currently paused.
    /// </summary>
    public bool IsProjectPaused { get; private set; } = false;

    public float BaseFontSize
    {
        get => _baseFontSize;
        set
        {
            if (Math.Abs(_baseFontSize - value) > 0.0001f)
            {
                _baseFontSize = Math.Clamp(value, 8.0f, 72.0f);
                ApplyFontSettings();
            }
        }
    }

    public float FontScaleMain
    {
        get => _fontScaleMain;
        set
        {
            if (Math.Abs(_fontScaleMain - value) > 0.0001f)
            {
                _fontScaleMain = Math.Clamp(value, 0.5f, 4.0f);
                ApplyFontSettings();
            }
        }
    }

    public float EffectiveFontSize => _baseFontSize * _fontScaleMain;

    public XnaColor ClearColor { get; set; } = XnaColor.Black;

    public ColorTheme Theme { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="EditorContext"/> has been disposed of.
    /// </summary>
    public bool IsDisposed { get; private set; }

    public EditorContext(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        _game = game;
        _graphicsDevice = game.GraphicsDevice;
        _contentManager = game.Content;
        ApplyFontSettings();
    }

    ~EditorContext() => Dispose(false);

    // public void CreateParticleEffect(string name)
    // {
    //     ArgumentException.ThrowIfNullOrEmpty(name);
    //     CloseProject();
    //     ParticleEffect = new ParticleEffect(name);
    // }

    public void CenterParticleEffect()
    {
        if (ParticleEffect == null)
        {
            return;
        }

        ParticleEffect.Position = _graphicsDevice.Viewport.Bounds.Center.ToVector2();
    }

    public void AddEmitter()
    {
        if (ParticleEffect == null)
        {
            return;
        }

        ParticleEmitter emitter = new(1000);
        emitter.Name = nameof(ParticleEmitter);

        int index = ParticleEffect.Emitters.Count;
        ParticleEffect.Emitters.Add(emitter);
        TrackLock(emitter);
        SelectEmitter(index);
        HasUnsavedChanges = true;
    }

    public void SelectEmitter(int index)
    {
        if (ParticleEffect == null || index < 0 || index >= ParticleEffect.Emitters.Count)
        {
            SelectedEmitter = null;
            SelectedEmitterIndex = -1;
        }
        else
        {
            SelectedEmitter = ParticleEffect.Emitters[index];
            SelectedEmitterIndex = index;
        }

        // Select the first modifier of this emitter (if there is one)
        SelectModifier(0);
    }

    public void RemoveEmitter(int index)
    {
        if (ParticleEffect == null || index < 0 || index >= ParticleEffect.Emitters.Count)
        {
            return;
        }

        ParticleEmitter emitter = ParticleEffect.Emitters[index];
        ParticleEffect.Emitters.RemoveAt(index);
        UntrackLock(emitter);

        // Update selection if we removed the selected emitter
        if (emitter == SelectedEmitter)
        {
            int newIndex = Math.Max(0, index - 1);
            SelectEmitter(newIndex);
        }

        HasUnsavedChanges = true;
    }

    public void ReorderEmitters(int fromIndex, int toIndex)
    {
        if (ParticleEffect == null || fromIndex < 0 || fromIndex >= ParticleEffect.Emitters.Count || toIndex < 0 || toIndex >= ParticleEffect.Emitters.Count)
        {
            return;
        }

        ParticleEmitter moving = ParticleEffect.Emitters[fromIndex];
        ParticleEffect.Emitters.RemoveAt(fromIndex);
        ParticleEffect.Emitters.Insert(toIndex, moving);

        // Maintain current selection by re-selecting at the potentially new index
        if (SelectedEmitter != null)
        {
            int currentIndex = ParticleEffect.Emitters.IndexOf(SelectedEmitter);
            SelectEmitter(currentIndex);
        }

        HasUnsavedChanges = true;
    }

    public void AddModifier(Type modifierType)
    {
        if (SelectedEmitter == null)
        {
            return;
        }

        Modifier modifier = CreateModifier(modifierType);
        int index = SelectedEmitter.Modifiers.Count;
        SelectedEmitter.Modifiers.Add(modifier);
        TrackLock(modifier);
        SelectModifier(index);
        HasUnsavedChanges = true;
    }

    public void SelectModifier(int index)
    {
        if (SelectedEmitter == null || index < 0 || index >= SelectedEmitter.Modifiers.Count)
        {
            SelectedModifier = null;
            SelectedModifierIndex = -1;
        }
        else
        {
            SelectedModifier = SelectedEmitter.Modifiers[index];
            SelectedModifierIndex = index;
        }

        // Select the first interpolator of this modifier (if there is one)
        SelectInterpolator(0);
    }

    public void RemoveModifier(int index)
    {
        if (SelectedEmitter == null || index < 0 || index >= SelectedEmitter.Modifiers.Count)
        {
            return;
        }

        Modifier modifier = SelectedEmitter.Modifiers[index];
        SelectedEmitter.Modifiers.RemoveAt(index);
        UntrackLock(modifier);

        // Update selection if we removed the selected modifier
        if (modifier == SelectedModifier)
        {
            int newIndex = Math.Max(0, index - 1);
            SelectModifier(newIndex);
        }

        HasUnsavedChanges = true;
    }

    public void ReorderModifiers(int fromIndex, int toIndex)
    {
        if (SelectedEmitter == null || fromIndex < 0 || fromIndex >= SelectedEmitter.Modifiers.Count || toIndex < 0 || toIndex >= SelectedEmitter.Modifiers.Count)
        {
            return;
        }

        Modifier moving = SelectedEmitter.Modifiers[fromIndex];
        SelectedEmitter.Modifiers.RemoveAt(fromIndex);
        SelectedEmitter.Modifiers.Insert(toIndex, moving);

        // Maintain current selection by re-selecting at the potentially new index
        if (SelectedModifier != null)
        {
            int currentIndex = SelectedEmitter.Modifiers.IndexOf(SelectedModifier);
            SelectModifier(currentIndex);
        }

        HasUnsavedChanges = true;
    }

    public bool SupportsInterpolators(Modifier modifier) => modifier is AgeModifier || modifier is VelocityModifier;

    public List<Interpolator> GetCurrentInterpolators()
    {
        return SelectedModifier switch
        {
            AgeModifier age => age.Interpolators,
            VelocityModifier velocity => velocity.Interpolators,
            _ => null
        };
    }


    private Modifier CreateModifier(Type modifierType)
    {
        if (modifierType == typeof(RectangleLoopContainerModifier))
        {
            return new RectangleLoopContainerModifier() { Width = 100, Height = 100 };
        }

        if (modifierType == typeof(RectangleContainerModifier))
        {
            return new RectangleContainerModifier { Width = 100, Height = 100 };
        }

        if (modifierType == typeof(LinearGravityModifier))
        {
            return new LinearGravityModifier() { Direction = Vector2.UnitY, Strength = 100.0f };
        }

        if (modifierType == typeof(VortexModifier))
        {
            return new VortexModifier() { };
        }

        if (modifierType == typeof(OpacityFastFadeModifier))
        {
            return new OpacityFastFadeModifier();
        }

        if (modifierType == typeof(AgeModifier))
        {
            return new AgeModifier() { Interpolators = new List<Interpolator>() { new ScaleInterpolator() { StartValue = Vector2.Zero, EndValue = Vector2.One } } };
        }

        if (modifierType == typeof(CircleContainerModifier))
        {
            return new CircleContainerModifier() { Radius = 100.0f };
        }

        if (modifierType == typeof(DragModifier))
        {
            return new DragModifier();
        }

        if (modifierType == typeof(RotationModifier))
        {
            return new RotationModifier() { RotationRate = MathF.PI / 4.0f };
        }

        if (modifierType == typeof(VelocityColorModifier))
        {
            return new VelocityColorModifier() { VelocityThreshold = 100.0f, StationaryColor = new HslColor(0, 0, 1.0f), VelocityColor = new HslColor(0, 1.0f, 0.5f) };
        }

        if (modifierType == typeof(VelocityModifier))
        {
            return new VelocityModifier() { VelocityThreshold = 100.0f, Interpolators = new List<Interpolator>() { new ScaleInterpolator() { StartValue = Vector2.Zero, EndValue = Vector2.One } } };
        }

        throw new InvalidOperationException($"Unknown modifier type '{modifierType.Name}'");
    }

    public void AddInterpolator(Type interpolatorType)
    {
        List<Interpolator> interpolators = GetCurrentInterpolators();
        if (interpolators == null)
        {
            return;
        }

        Interpolator interpolator = CreateInterpolator(interpolatorType);
        int index = interpolators.Count;
        interpolators.Add(interpolator);
        TrackLock(interpolator);
        SelectInterpolator(index);
        HasUnsavedChanges = true;
    }

    public void SelectInterpolator(int index)
    {
        List<Interpolator> interpolators = GetCurrentInterpolators();
        if (interpolators == null || index < 0 || index >= interpolators.Count)
        {
            SelectedInterpolator = null;
            SelectedInterpolatorIndex = -1;
        }
        else
        {
            SelectedInterpolator = interpolators[index];
            SelectedInterpolatorIndex = index;
        }
    }

    public void RemoveInterpolator(int index)
    {
        List<Interpolator> interpolators = GetCurrentInterpolators();
        if (interpolators == null || index < 0 || index >= interpolators.Count)
        {
            return;
        }

        Interpolator interpolator = interpolators[index];
        interpolators.RemoveAt(index);
        UntrackLock(interpolator);

        // Update selection if we removed the selected interpolator
        if (interpolator == SelectedInterpolator)
        {
            int newIndex = Math.Max(0, index - 1);
            SelectInterpolator(newIndex);
        }

        HasUnsavedChanges = true;
    }

    public void ReorderInterpolators(int fromIndex, int toIndex)
    {
        List<Interpolator> interpolators = GetCurrentInterpolators();
        if (interpolators == null || fromIndex < 0 || fromIndex >= interpolators.Count || toIndex < 0 || toIndex >= interpolators.Count)
        {
            return;
        }

        Interpolator moving = interpolators[fromIndex];
        interpolators.RemoveAt(fromIndex);
        interpolators.Insert(toIndex, moving);

        // Maintain current selection by re-selecting at the potentially new index
        if (SelectedInterpolator != null)
        {
            int currentIndex = interpolators.IndexOf(SelectedInterpolator);
            SelectInterpolator(currentIndex);
        }

        HasUnsavedChanges = true;
    }

    private static Interpolator CreateInterpolator(Type interpolatorType)
    {
        if (interpolatorType == typeof(ColorInterpolator))
        {
            return new ColorInterpolator() { StartValue = new HslColor(0.0f, 0.0f, 0.0f), EndValue = new HslColor(0.0f, 0.0f, 1.0f) };
        }

        if (interpolatorType == typeof(HueInterpolator))
        {
            return new HueInterpolator() { StartValue = 0.0f, EndValue = 1.0f };
        }

        if (interpolatorType == typeof(OpacityInterpolator))
        {
            return new OpacityInterpolator() { StartValue = 0.0f, EndValue = 1.0f };
        }

        if (interpolatorType == typeof(RotationInterpolator))
        {
            return new RotationInterpolator() { StartValue = 0.0f, EndValue = MathF.PI / 2.0f };
        }

        if (interpolatorType == typeof(ScaleInterpolator))
        {
            return new ScaleInterpolator() { StartValue = Vector2.One, EndValue = Vector2.Zero };
        }

        if (interpolatorType == typeof(VelocityInterpolator))
        {
            return new VelocityInterpolator() { StartValue = Vector2.Zero, EndValue = Vector2.One };
        }

        throw new InvalidOperationException($"Unknown interpolator type '{interpolatorType.Name}'");
    }
    public void ClearSelection()
    {
        SelectEmitter(-1);
    }

    public bool IsLocked(ParticleEmitter emitter) => _locks.GetValueOrDefault(emitter, false);
    public bool IsLocked(Modifier modifier) => _locks.GetValueOrDefault(modifier, false);
    public bool IsLocked(Interpolator interpolator) => _locks.GetValueOrDefault(interpolator, false);
    public bool ToggleLock(ParticleEmitter emitter) => _locks[emitter] = !IsLocked(emitter);
    public bool ToggleLock(Modifier modifier) => _locks[modifier] = !IsLocked(modifier);
    public bool ToggleLock(Interpolator interpolator) => _locks[interpolator] = !IsLocked(interpolator);
    public void TrackLock(ParticleEmitter emitter) => _locks[emitter] = false;
    public void TrackLock(Modifier modifier) => _locks[modifier] = false;
    public void TrackLock(Interpolator interpolator) => _locks[interpolator] = false;
    public void UntrackLock(Modifier emitter) => _locks.Remove(emitter);
    public void UntrackLock(ParticleEmitter modifier) => _locks.Remove(modifier);
    public void UntrackLock(Interpolator parameter) => _locks.Remove(parameter);
    public void ClearLocks() => _locks.Clear();

    public string GetWorkingDirectory() => Directory.GetCurrentDirectory();
    public void SetWorkingDirectory(string directory) => Directory.SetCurrentDirectory(directory);

    public string CopyTo(string filePath, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        string sourceName = Path.GetFileName(filePath);
        string destination = Path.Combine(GetWorkingDirectory(), sourceName);
        File.Copy(filePath, destination, overwrite);
        return destination;
    }

    public string GetRelativePath(string filePath) => Path.GetRelativePath(GetWorkingDirectory(), filePath);

    public bool IsRelativeTo(string filePath)
    {
        string relativePath = GetRelativePath(filePath);
        return !relativePath.StartsWith('.');
    }

    public bool TextureExists(string fileName) => _textureCache.ContainsKey(fileName);

    public void AddTexture(string filePath, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        string fileName = Path.GetFileName(filePath);

        // If already in project directory, just load it
        if (IsRelativeTo(filePath))
        {
            if (overwrite || !TextureExists(fileName))
            {
                LoadTexture(fileName);
            }
            return;
        }

        string destinationPath = Path.Combine(GetWorkingDirectory(), fileName);

        // Check if file exists and whether we should overwrite
        if (File.Exists(destinationPath) && !overwrite)
        {
            // File exists but we're not overwriting, so just ensure it's loaded
            if (!TextureExists(fileName))
            {
                LoadTexture(fileName);
            }
            return;
        }

        // Copy and load texture
        CopyTo(filePath, overwrite: true);
        LoadTexture(fileName);
    }

    public Texture2D LoadTexture(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        // If already cached, unload and reload
        if (_textureCache.TryGetValue(fileName, out Texture2D cached))
        {
            cached.Dispose();
            _textureCache.Remove(fileName);
        }

        Texture2D texture = Texture2D.FromFile(_graphicsDevice, fileName);
        texture.Name = fileName;
        _textureCache[fileName] = texture;

        return texture;
    }

    public Texture2D GetTexture(string fileName)
    {
        if (_textureCache.TryGetValue(fileName, out Texture2D texture))
        {
            return texture;
        }

        return null;
    }

    public IEnumerable<string> GetTextureNames() => _textureCache.Keys;

    public void ClearTextures()
    {
        foreach (var kvp in _textureCache)
        {
            kvp.Value.Dispose();
        }

        _textureCache.Clear();
    }

    public void CreateProject(string projectName, string projectDirectory, bool createProjectDirectory)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);
        ArgumentException.ThrowIfNullOrEmpty(projectDirectory);

        CloseProject();

        ProjectName = projectName;
        ProjectDirectory = projectDirectory;

        if (createProjectDirectory)
        {
            ProjectDirectory = Path.Combine(ProjectDirectory, ProjectName);
        }

        ProjectFilePath = Path.Combine(ProjectDirectory, ProjectName);
        ProjectFilePath = Path.ChangeExtension(ProjectFilePath, ".ember");

        Directory.CreateDirectory(ProjectDirectory);

        SetWorkingDirectory(ProjectDirectory);
        _contentManager.RootDirectory = ProjectDirectory;

        ParticleEffect = new ParticleEffect(ProjectName);

        CenterParticleEffect();

        HasUnsavedChanges = true;
        SaveProject();
    }

    public void OpenProject(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        CloseProject();

        ProjectName = Path.GetFileNameWithoutExtension(filePath);
        ProjectDirectory = Path.GetDirectoryName(filePath);
        ProjectFilePath = filePath;

        SetWorkingDirectory(ProjectDirectory);
        _contentManager.RootDirectory = ProjectDirectory;

        using ParticleEffectReader reader = new(ProjectFilePath, _contentManager);
        ParticleEffect = reader.ReadParticleEffect();
        CenterParticleEffect();

        HasUnsavedChanges = false;
    }

    public void SaveProject()
    {
        if (ParticleEffect == null)
        {
            return;
        }

        using ParticleEffectWriter writer = new(ProjectFilePath);
        writer.WriteParticleEffect(ParticleEffect);

        HasUnsavedChanges = false;
    }

    public void CloseProject()
    {
        if (ParticleEffect != null)
        {
            ParticleEffect.Dispose();
            ParticleEffect = null;
        }

        _contentManager.Unload();
        ClearTextures();
        ClearSelection();
        ClearLocks();

        ProjectName = string.Empty;
        ProjectDirectory = string.Empty;
        ProjectFilePath = string.Empty;
        HasUnsavedChanges = false;

        GC.Collect();
    }

    public void PauseProject(bool pause) => IsProjectPaused = pause;

    private void ApplyFontSettings()
    {
        ImGuiStylePtr stylePtr = ImGui.GetStyle();
        stylePtr.FontSizeBase = _baseFontSize;
        stylePtr.FontScaleMain = _fontScaleMain;
        stylePtr.FontScaleDpi = 1.0f;

        // Temporary hack from ImGui demo until font atlas rebuilding is finalized
        stylePtr.NextFrameFontSizeBase = _baseFontSize;
    }

    public void ApplyTheme(ColorTheme theme)
    {
        if(Theme == theme)
        {
            return;
        }

        Theme = theme;

        // Themes are based on Catppuccin Latte (Light) and Frappe (Dark)
        // https://catppuccin.com/

        ImGuiStylePtr stylePtr = ImGui.GetStyle();

        SysVec4 @base = theme == ColorTheme.Light ? new(0.937f, 0.945f, 0.961f, 1.0f) : new(0.192f, 0.200f, 0.271f, 1.0f);
        SysVec4 mantle = theme == ColorTheme.Light ? new(0.902f, 0.914f, 0.937f, 1.0f) : new(0.161f, 0.169f, 0.235f, 1.0f);
        SysVec4 crust = theme == ColorTheme.Light ? new(0.863f, 0.878f, 0.910f, 1.0f) : new(0.137f, 0.145f, 0.200f, 1.0f);
        SysVec4 surface0 = theme == ColorTheme.Light ? new(0.800f, 0.816f, 0.855f, 1.0f) : new(0.247f, 0.267f, 0.349f, 1.0f);
        SysVec4 surface1 = theme == ColorTheme.Light ? new(0.737f, 0.753f, 0.800f, 1.0f) : new(0.314f, 0.337f, 0.427f, 1.0f);
        SysVec4 surface2 = theme == ColorTheme.Light ? new(0.675f, 0.694f, 0.745f, 1.0f) : new(0.380f, 0.408f, 0.502f, 1.0f);
        SysVec4 overlay0 = theme == ColorTheme.Light ? new(0.612f, 0.627f, 0.690f, 1.0f) : new(0.447f, 0.478f, 0.576f, 1.0f);
        SysVec4 overlay1 = theme == ColorTheme.Light ? new(0.549f, 0.561f, 0.631f, 1.0f) : new(0.514f, 0.549f, 0.651f, 1.0f);
        SysVec4 overlay2 = theme == ColorTheme.Light ? new(0.486f, 0.498f, 0.576f, 1.0f) : new(0.580f, 0.616f, 0.725f, 1.0f);
        SysVec4 text = theme == ColorTheme.Light ? new(0.298f, 0.310f, 0.412f, 1.0f) : new(0.780f, 0.816f, 0.961f, 1.0f);
        SysVec4 subtext1 = theme == ColorTheme.Light ? new(0.361f, 0.373f, 0.467f, 1.0f) : new(0.714f, 0.753f, 0.894f, 1.0f);
        SysVec4 subtext0 = theme == ColorTheme.Light ? new(0.424f, 0.435f, 0.522f, 1.0f) : new(0.647f, 0.682f, 0.816f, 1.0f);
        SysVec4 rosewater = theme == ColorTheme.Light ? new(0.863f, 0.537f, 0.467f, 1.0f) : new(0.949f, 0.835f, 0.812f, 1.0f);
        SysVec4 blue = theme == ColorTheme.Light ? new(0.118f, 0.400f, 0.961f, 1.0f) : new(0.549f, 0.671f, 0.929f, 1.0f);
        SysVec4 lavender = theme == ColorTheme.Light ? new(0.451f, 0.529f, 0.992f, 1.0f) : new(0.729f, 0.729f, 0.949f, 1.0f);
        SysVec4 sapphire = theme == ColorTheme.Light ? new(0.129f, 0.624f, 0.710f, 1.0f) : new(0.518f, 0.761f, 0.863f, 1.0f);
        SysVec4 sky = theme == ColorTheme.Light ? new(0.016f, 0.647f, 0.898f, 1.0f) : new(0.600f, 0.812f, 0.863f, 1.0f);
        SysVec4 teal = theme == ColorTheme.Light ? new(0.094f, 0.573f, 0.600f, 1.0f) : new(0.510f, 0.780f, 0.753f, 1.0f);
        SysVec4 green = theme == ColorTheme.Light ? new(0.251f, 0.627f, 0.169f, 1.0f) : new(0.651f, 0.812f, 0.537f, 1.0f);
        SysVec4 yellow = theme == ColorTheme.Light ? new(0.875f, 0.557f, 0.114f, 1.0f) : new(0.898f, 0.784f, 0.561f, 1.0f);
        SysVec4 peach = theme == ColorTheme.Light ? new(0.996f, 0.392f, 0.043f, 1.0f) : new(0.937f, 0.624f, 0.463f, 1.0f);
        SysVec4 maroon = theme == ColorTheme.Light ? new(0.902f, 0.271f, 0.325f, 1.0f) : new(0.918f, 0.600f, 0.612f, 1.0f);
        SysVec4 red = theme == ColorTheme.Light ? new(0.820f, 0.059f, 0.224f, 1.0f) : new(0.906f, 0.514f, 0.518f, 1.0f);
        SysVec4 mauve = theme == ColorTheme.Light ? new(0.533f, 0.224f, 0.937f, 1.0f) : new(0.792f, 0.624f, 0.902f, 1.0f);
        SysVec4 pink = theme == ColorTheme.Light ? new(0.918f, 0.463f, 0.796f, 1.0f) : new(0.961f, 0.722f, 0.894f, 1.0f);

        // Selection color uses blue for light theme and mauve for dark themes with 30% opacity
        SysVec4 selection = (theme == ColorTheme.Light ? blue : mauve) with { Z = 0.3f };

        // Background hierarchy: Base (primary) -> Mantle (secondary) -> Surface0 (tertiary)
        stylePtr.Colors[(int)ImGuiCol.WindowBg] = @base;
        stylePtr.Colors[(int)ImGuiCol.ChildBg] = mantle;
        stylePtr.Colors[(int)ImGuiCol.PopupBg] = surface0;
        stylePtr.Colors[(int)ImGuiCol.MenuBarBg] = mantle;

        // Text hierarchy and selection
        stylePtr.Colors[(int)ImGuiCol.Text] = text;
        stylePtr.Colors[(int)ImGuiCol.TextDisabled] = subtext0;
        stylePtr.Colors[(int)ImGuiCol.TextSelectedBg] = selection;

        // Frame backgrounds need sufficient contrast from container backgrounds
        stylePtr.Colors[(int)ImGuiCol.FrameBg] = surface1;
        stylePtr.Colors[(int)ImGuiCol.FrameBgHovered] = surface2;
        stylePtr.Colors[(int)ImGuiCol.FrameBgActive] = overlay0;

        // Primary interactive elements use blue as the main accent color
        stylePtr.Colors[(int)ImGuiCol.Button] = blue with { Z = 0.3f };
        stylePtr.Colors[(int)ImGuiCol.ButtonHovered] = blue with { Z = 0.5f };
        stylePtr.Colors[(int)ImGuiCol.ButtonActive] = blue with { Z = 0.7f };

        // Secondary interactive elements use lavender for distinction
        stylePtr.Colors[(int)ImGuiCol.Header] = lavender with { Z = 0.3f };
        stylePtr.Colors[(int)ImGuiCol.HeaderHovered] = lavender with { Z = 0.5f };
        stylePtr.Colors[(int)ImGuiCol.HeaderActive] = lavender with { Z = 0.7f };

        // Semantic feedback colors
        stylePtr.Colors[(int)ImGuiCol.CheckMark] = green;

        // Title bar styling using the deepest background colors
        stylePtr.Colors[(int)ImGuiCol.TitleBg] = crust;
        stylePtr.Colors[(int)ImGuiCol.TitleBgActive] = mantle;
        stylePtr.Colors[(int)ImGuiCol.TitleBgCollapsed] = crust;

        // Borders and separators
        stylePtr.Colors[(int)ImGuiCol.Border] = overlay0;
        stylePtr.Colors[(int)ImGuiCol.BorderShadow] = @base with { Z = 0.0f };
        stylePtr.Colors[(int)ImGuiCol.Separator] = overlay0;
        stylePtr.Colors[(int)ImGuiCol.SeparatorHovered] = lavender;
        stylePtr.Colors[(int)ImGuiCol.SeparatorActive] = lavender;

        // Scrollbar components following the surface hierarchy
        stylePtr.Colors[(int)ImGuiCol.ScrollbarBg] = @base;
        stylePtr.Colors[(int)ImGuiCol.ScrollbarGrab] = surface0;
        stylePtr.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = surface1;
        stylePtr.Colors[(int)ImGuiCol.ScrollbarGrabActive] = surface2;

        // Sliders using primary accent colors
        stylePtr.Colors[(int)ImGuiCol.SliderGrab] = blue;
        stylePtr.Colors[(int)ImGuiCol.SliderGrabActive] = sapphire;

        // Tab styling using lavender as the accent color for consistency with headers
        stylePtr.Colors[(int)ImGuiCol.Tab] = surface0;
        stylePtr.Colors[(int)ImGuiCol.TabHovered] = lavender with { Z = 0.4f };
        stylePtr.Colors[(int)ImGuiCol.TabSelected] = lavender with { Z = 0.6f };
        stylePtr.Colors[(int)ImGuiCol.TabSelectedOverline] = lavender;
        stylePtr.Colors[(int)ImGuiCol.TabDimmed] = surface0;
        stylePtr.Colors[(int)ImGuiCol.TabDimmedSelected] = lavender with { Z = 0.3f };
        stylePtr.Colors[(int)ImGuiCol.TabDimmedSelectedOverline] = lavender with { Z = 0.6f };

        // Resize grips using overlay colors with blue accent when active
        stylePtr.Colors[(int)ImGuiCol.ResizeGrip] = overlay2 with { Z = 0.5f };
        stylePtr.Colors[(int)ImGuiCol.ResizeGripHovered] = blue with { Z = 0.7f };
        stylePtr.Colors[(int)ImGuiCol.ResizeGripActive] = blue with { Z = 0.9f };

        // Plot colors using complementary theme colors
        stylePtr.Colors[(int)ImGuiCol.PlotLines] = blue;
        stylePtr.Colors[(int)ImGuiCol.PlotLinesHovered] = sky;
        stylePtr.Colors[(int)ImGuiCol.PlotHistogram] = teal;
        stylePtr.Colors[(int)ImGuiCol.PlotHistogramHovered] = green;

        // Table styling following background and overlay hierarchy
        stylePtr.Colors[(int)ImGuiCol.TableHeaderBg] = surface0;
        stylePtr.Colors[(int)ImGuiCol.TableBorderStrong] = overlay1;
        stylePtr.Colors[(int)ImGuiCol.TableBorderLight] = overlay0;
        stylePtr.Colors[(int)ImGuiCol.TableRowBg] = @base with { Z = 0.0f };
        stylePtr.Colors[(int)ImGuiCol.TableRowBgAlt] = surface0 with { Z = 0.3f };

        // Docking system colors
        stylePtr.Colors[(int)ImGuiCol.DockingPreview] = blue with { Z = 0.7f };
        stylePtr.Colors[(int)ImGuiCol.DockingEmptyBg] = @base;

        // Drag and drop visual feedback
        stylePtr.Colors[(int)ImGuiCol.DragDropTarget] = yellow with { Z = 0.9f };

        // Navigation and modal overlay colors
        stylePtr.Colors[(int)ImGuiCol.NavWindowingHighlight] = text with { Z = 0.7f };
        stylePtr.Colors[(int)ImGuiCol.NavWindowingDimBg] = @base with { Z = 0.2f };
        stylePtr.Colors[(int)ImGuiCol.ModalWindowDimBg] = @base with { Z = 0.35f };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            ClearTextures();
        }

        IsDisposed = true;
    }
}
