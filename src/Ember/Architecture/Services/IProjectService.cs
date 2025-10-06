using System;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages particle effect project lifecycle, metadata, and persistence.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Gets the name of the current project.
    /// </summary>
    string ProjectName { get; }

    /// <summary>
    /// Gets the directory path of the current project.
    /// </summary>
    string ProjectDirectory { get; }

    /// <summary>
    /// Gets the file path of the current project.
    /// </summary>
    string ProjectFilePath { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the project has unsaved changes.
    /// </summary>
    bool HasUnsavedChanges { get; set; }

    /// <summary>
    /// Gets a value indicating whether a project is currently open.
    /// </summary>
    bool IsProjectOpen { get; }

    /// <summary>
    /// Gets a value indicating whether the project playback is currently paused.
    /// </summary>
    bool IsProjectPaused { get; }

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
    void CreateProject(string projectName, string projectDirectory, bool createProjectDirectory);

    /// <summary>
    /// Opens an existing project from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path of the project to open.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="filePath"/> is <see langword="null"/> or consists only of white-space characters.
    /// </exception>
    void OpenProject(string filePath);

    /// <summary>
    /// Saves the current project.
    /// </summary>
    void SaveProject();

    /// <summary>
    /// Closes the current project, clearing all associated data.
    /// </summary>
    void CloseProject();

    /// <summary>
    /// Sets the pause state of the project.
    /// </summary>
    /// <param name="pause"><see langword="true"/> if the project is paused; otherwise, <see langword="false"/>.</param>
    void PauseProject(bool pause);
}
