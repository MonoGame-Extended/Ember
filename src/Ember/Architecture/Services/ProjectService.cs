using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Particles;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages particle effect project lifecycle, metadata, and persistence.
/// </summary>
public sealed class ProjectService : IProjectService
{
    private readonly IPathService _pathService;
    private readonly IParticleEffectService _particleEffectService;
    private readonly ISelectionService _selectionService;
    private readonly ILockService _lockService;
    private readonly ITextureService _textureService;
    private readonly IGraphicsDeviceService _graphicsDeviceService;
    private readonly ContentManager _contentManager;

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
    public bool IsProjectOpen => !string.IsNullOrEmpty(ProjectFilePath);

    /// <summary>
    /// Gets a value indicating whether the project playback is currently paused.
    /// </summary>
    public bool IsProjectPaused { get; private set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectService"/> class.
    /// </summary>
    /// <param name="services">The service provider used to resolve dependencies.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public ProjectService(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _pathService = services.GetService(typeof(IPathService)) as IPathService;
        _particleEffectService = services.GetService(typeof(IParticleEffectService)) as IParticleEffectService;
        _selectionService = services.GetService(typeof(ISelectionService)) as ISelectionService;
        _lockService = services.GetService(typeof(ILockService)) as ILockService;
        _textureService = services.GetService(typeof(ITextureService)) as ITextureService;
        _contentManager = services.GetService(typeof(ContentManager)) as ContentManager;
        _graphicsDeviceService = services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
    }

    /// <summary>
    /// Creates a new project with the specified name and directory.
    /// </summary>
    /// <param name="projectName">The name of the project.</param>
    /// <param name="projectDirectory">The directory where the project will be created.</param>
    /// <param name="createProjectDirectory">
    /// If <see langword="true"/>, creates a subdirectory with the project name; otherwise, uses the specified directory directly.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="projectName"/> or <paramref name="projectDirectory"/> is <see langword="null"/> or consists only of white-space characters.
    /// </exception>
    public void CreateProject(string projectName, string projectDirectory, bool createProjectDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectDirectory);

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

        _pathService.SetWorkingDirectory(ProjectDirectory);
        _contentManager.RootDirectory = ProjectDirectory;

        _particleEffectService.Create(ProjectName);
        CenterParticleEffect();

        HasUnsavedChanges = true;
        SaveProject();
    }

    /// <summary>
    /// Opens an existing project from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path of the project to open.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="filePath"/> is <see langword="null"/> or consists only of white-space characters.
    /// </exception>
    public void OpenProject(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        CloseProject();

        ProjectName = Path.GetFileNameWithoutExtension(filePath);
        ProjectDirectory = Path.GetDirectoryName(filePath);
        ProjectFilePath = filePath;

        _pathService.SetWorkingDirectory(ProjectDirectory);
        _contentManager.RootDirectory = ProjectDirectory;

        using ParticleEffectReader reader = new(ProjectFilePath, _contentManager);
        ParticleEffect effect = reader.ReadParticleEffect();
        _particleEffectService.SetCurrent(effect);
        CenterParticleEffect();

        HasUnsavedChanges = false;
    }

    /// <summary>
    /// Saves the current project.
    /// </summary>
    public void SaveProject()
    {
        if (_particleEffectService.Current == null)
        {
            return;
        }

        using ParticleEffectWriter writer = new(ProjectFilePath);
        writer.WriteParticleEffect(_particleEffectService.Current);

        HasUnsavedChanges = false;
    }

    /// <summary>
    /// Closes the current project, clearing all associated data.
    /// </summary>
    public void CloseProject()
    {
        _particleEffectService.Clear();
        _contentManager.Unload();
        _textureService?.Clear();
        _selectionService.ClearSelections();
        _lockService.Clear();

        ProjectName = string.Empty;
        ProjectDirectory = string.Empty;
        ProjectFilePath = string.Empty;
        HasUnsavedChanges = false;

        GC.Collect();
    }

    /// <summary>
    /// Sets the pause state of the project.
    /// </summary>
    /// <param name="pause"><see langword="true"/> if the project is paused; otherwise, <see langword="false"/>.</param>
    public void PauseProject(bool pause)
    {
        IsProjectPaused = pause;
    }

    private void CenterParticleEffect()
    {
        Vector2 center = _graphicsDeviceService.GraphicsDevice.Viewport.Bounds.Center.ToVector2();
        _particleEffectService.Center(center);
    }
}
