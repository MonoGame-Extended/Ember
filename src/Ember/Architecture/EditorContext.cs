using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Ember.Architecture.Style;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
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
    private readonly string _version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    private float _baseFontSize = 16.0f;
    private float _fontScaleMain = 1.0f;

    private bool _shouldExit;
    private PendingAction _pendingAction = PendingAction.None;
    private string _pendingProjectName;
    private string _pendingProjectDirectory;
    private bool _pendingCreateProjectDirectory;
    private string _pendingProjectFilePath;

    public ParticleEffect ParticleEffect { get; private set; }
    public ParticleEmitter SelectedEmitter { get; private set; }
    public int SelectedEmitterIndex { get; private set; } = -1;

    public Modifier SelectedModifier { get; private set; }
    public int SelectedModifierIndex { get; private set; } = -1;

    public Interpolator SelectedInterpolator { get; private set; }
    public int SelectedInterpolatorIndex { get; private set; } = -1;

    public string ProjectName { get; private set; } = string.Empty;
    public string ProjectDirectory { get; private set; } = string.Empty;
    public string ProjectFilePath { get; private set; } = string.Empty;

    public string LastUsedTextureDirectory { get; set; } = string.Empty;
    public string LastUsedProjectDirectory { get; set; } = string.Empty;

    public bool HasUnsavedChanges { get; set; }
    public bool IsProjectOpen => ParticleEffect != null;
    public bool IsProjectPaused { get; private set; } = false;
    public bool IsSavePromptPending => _pendingAction != PendingAction.None;

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

    public ITheme CurrentTheme { get; set; }

    public string Version => _version;

    public Game Game => _game;

    public bool IsDisposed { get; private set; }

    public EditorContext(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        _game = game;
        _graphicsDevice = game.GraphicsDevice;
        _contentManager = game.Content;
        _game.Exiting += OnExiting;
        ApplyTheme<CatppuccinFrappeTheme>();
        ApplyFontSettings();
    }

    ~EditorContext() => Dispose(false);

    private void OnExiting(object sender, EventArgs e)
    {
        if (!_shouldExit && HasUnsavedChanges)
        {
            _pendingAction = PendingAction.Exit;
            ((ExitingEventArgs)e).Cancel = true;
        }
    }

    public void RequestExit()
    {
        if (HasUnsavedChanges)
        {
            _pendingAction = PendingAction.Exit;
        }
        else
        {
            _game.Exit();
        }
    }

    public void RequestCreateProject(string projectName, string projectDirectory, bool createProjectDirectory)
    {
        if (HasUnsavedChanges)
        {
            _pendingAction = PendingAction.CreateProject;
            _pendingProjectName = projectName;
            _pendingProjectDirectory = projectDirectory;
            _pendingCreateProjectDirectory = createProjectDirectory;
        }
        else
        {
            CreateProject(projectName, projectDirectory, createProjectDirectory);
        }
    }

    public void RequestOpenProject(string filePath)
    {
        if (HasUnsavedChanges)
        {
            _pendingAction = PendingAction.OpenProject;
            _pendingProjectFilePath = filePath;
        }
        else
        {
            OpenProject(filePath);
        }
    }

    public void ConfirmPendingAction(bool save)
    {
        if (save)
        {
            SaveProject();
        }

        ExecutePendingAction();
    }

    public void CancelPendingAction()
    {
        _pendingAction = PendingAction.None;
        _pendingProjectName = null;
        _pendingProjectDirectory = null;
        _pendingCreateProjectDirectory = false;
        _pendingProjectFilePath = null;
    }

    private void ExecutePendingAction()
    {
        switch (_pendingAction)
        {
            case PendingAction.Exit:
                _shouldExit = true;
                _game.Exit();
                break;
            case PendingAction.CreateProject:
                CreateProject(_pendingProjectName, _pendingProjectDirectory, _pendingCreateProjectDirectory);
                break;
            case PendingAction.OpenProject:
                OpenProject(_pendingProjectFilePath);
                break;
        }

        CancelPendingAction();
    }

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

        LastUsedTextureDirectory = ProjectDirectory;
        LastUsedProjectDirectory = ProjectDirectory;

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

        LastUsedTextureDirectory = ProjectDirectory;
        LastUsedProjectDirectory = ProjectDirectory;

        ParticleEffect = ParticleEffectSerializer.Deserialize(ProjectFilePath, _contentManager);
        CenterParticleEffect();

        HasUnsavedChanges = false;
    }

    public void SaveProject()
    {
        if (ParticleEffect == null)
        {
            return;
        }

        ParticleEffectSerializer.Serialize(ProjectFilePath, ParticleEffect);

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

    public void ApplyTheme<T>() where T : ITheme
    {
        CurrentTheme = Activator.CreateInstance(typeof(T)) as ITheme;
        Theme.Apply(CurrentTheme);
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
