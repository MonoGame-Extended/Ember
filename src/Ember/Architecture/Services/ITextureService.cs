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
    /// Checks whether a texture with the specified file name exists in the cache.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns><see langword="true"/> if the texture exists in the cache; otherwise, <see langword="false"/>.</returns>
    bool TextureExists(string fileName);

    /// <summary>
    /// Adds a texture file to the project, copying it to the working directory if necessary and loading it into the cache.
    /// </summary>
    /// <param name="filePath">The absolute or relative path to the texture file to add.</param>
    /// <param name="overwrite">Whether to overwrite an existing texture with the same file name.</param>
    void AddTexture(string filePath, bool overwrite = false);

    /// <summary>
    /// Loads a texture from the working directory into the cache.
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
    /// Clears all cached textures and disposes their resources.
    /// </summary>
    void Clear();
}
