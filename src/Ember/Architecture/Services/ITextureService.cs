using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages texture loading, caching, and assignment for particle emitters.
/// </summary>
public interface ITextureService : IDisposable
{
    /// <summary>
    /// Adds a texture file to the project, copying it if necessary.
    /// </summary>
    /// <param name="filePath">The absolute or relative path to the texture file.</param>
    /// <param name="onOverwriteConfirm">Callback invoked when a file would be overwritten, allowing user confirmation.</param>
    void AddTexture(string filePath, Action<string, Action> onOverwriteConfirm = null);

    /// <summary>
    /// Loads a texture from the project directory into the cache.
    /// </summary>
    /// <param name="fileName">The file name of the texture to load.</param>
    /// <returns>The loaded <see cref="Texture2D"/>.</returns>
    Texture2D LoadTexture(string fileName);

    /// <summary>
    /// Gets a cached texture by file name.
    /// </summary>
    /// <param name="fileName">The file name of the texture.</param>
    /// <returns>The cached <see cref="Texture2D"/>, or <see langword="null"/> if not found.</returns>
    Texture2D GetTexture(string fileName);

    /// <summary>
    /// Gets all cached texture file names.
    /// </summary>
    /// <returns>A collection of texture file names currently cached.</returns>
    IEnumerable<string> GetTextureNames();

    /// <summary>
    /// Clears all cached textures and unloads them from the content manager.
    /// </summary>
    void Clear();
}
