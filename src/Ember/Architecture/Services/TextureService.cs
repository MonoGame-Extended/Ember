using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages texture loading, caching, and assignment for particle emitters.
/// </summary>
public sealed class TextureService : ITextureService
{
    private readonly IPathService _pathService;
    private readonly IGraphicsDeviceService _graphicsDeviceService;
    private readonly Dictionary<string, Texture2D> _textureCache = [];

    /// <summary>
    /// Gets a value indicating whether this <see cref="TextureService"/> has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureService"/> class.
    /// </summary>
    /// <param name="services">
    /// The service provider used to resolve <see cref="IPathService"/> and <see cref="IGraphicsDeviceService"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public TextureService(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _pathService = services.GetService(typeof(IPathService)) as IPathService;
        _graphicsDeviceService = services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="TextureService"/> class.
    /// </summary>
    ~TextureService() => Dispose(false);

    /// <summary>
    /// Checks whether a texture with the specified file name exists in the cache.
    /// </summary>
    /// <param name="fileName">
    /// The file name to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the texture exists in the cache; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TextureExists(string fileName)
    {
        return _textureCache.ContainsKey(fileName);
    }

    /// <summary>
    /// Adds a texture file to the project, copying it to the working directory if necessary and loading it into the cache.
    /// </summary>
    /// <param name="filePath">
    /// The absolute or relative path to the texture file to add.
    /// </param>
    /// <param name="overwrite">
    /// Whether to overwrite an existing texture with the same file name. If <see langword="false"/> and a texture
    /// with the same name exists, the existing texture is kept.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="filePath"/> is <see langword="null"/>, empty, or contains invalid path characters.
    /// </exception>
    /// <exception cref="IOException">
    /// An I/O error occurred during the file copy operation.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission to read the source file or write to the destination.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// The source file specified by <paramref name="filePath"/> was not found.
    /// </exception>
    /// <exception cref="PathTooLongException">
    /// The specified path exceeds the system-defined maximum length.
    /// </exception>
    public void AddTexture(string filePath, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        string fileName = Path.GetFileName(filePath);

        // If already in project directory, just load it
        if (_pathService.IsRelativeTo(filePath))
        {
            if (overwrite || !TextureExists(fileName))
            {
                LoadTexture(fileName);
            }
            return;
        }

        string destinationPath = Path.Combine(_pathService.GetWorkingDirectory(), fileName);

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

        // Copy and load the texture
        _pathService.CopyTo(filePath, overwrite: true);
        LoadTexture(fileName);
    }

    /// <summary>
    /// Loads a texture from the working directory into the cache, replacing any previously cached texture with the same name.
    /// </summary>
    /// <param name="fileName">
    /// The file name of the texture to load, relative to the working directory.
    /// </param>
    /// <returns>
    /// The loaded <see cref="Texture2D"/> instance.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="fileName"/> is <see langword="null"/>, empty, or contains invalid path characters.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// The texture file was not found in the working directory.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The graphics device is not available or has been disposed.
    /// </exception>
    /// <exception cref="IOException">
    /// An I/O error occurred while reading the texture file.
    /// </exception>
    /// <remarks>
    /// If a texture with the same file name is already cached, it is disposed and replaced with the newly loaded texture.
    /// </remarks>
    public Texture2D LoadTexture(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        // If already cached, unload and reload
        if (_textureCache.TryGetValue(fileName, out Texture2D cached))
        {
            cached.Dispose();
            _textureCache.Remove(fileName);
        }

        Texture2D texture = Texture2D.FromFile(_graphicsDeviceService.GraphicsDevice, fileName);
        texture.Name = fileName;
        _textureCache[fileName] = texture;

        return texture;
    }

    /// <summary>
    /// Gets a cached texture by file name.
    /// </summary>
    /// <param name="fileName">
    /// The file name of the texture to retrieve.
    /// </param>
    /// <returns>
    /// The cached <see cref="Texture2D"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    public Texture2D GetTexture(string fileName)
    {
        if (_textureCache.TryGetValue(fileName, out Texture2D texture))
        {
            return texture;
        }

        return null;
    }

    /// <summary>
    /// Gets the file names of all currently cached textures.
    /// </summary>
    /// <returns>
    /// A collection of file names for textures currently in the cache.
    /// </returns>
    public IEnumerable<string> GetTextureNames()
    {
        return _textureCache.Keys;
    }

    /// <summary>
    /// Clears all cached textures, disposing of their resources.
    /// </summary>
    /// <remarks>
    /// After calling this method, the texture cache will be empty and all previously cached textures will be disposed.
    /// </remarks>
    public void Clear()
    {
        foreach (var kvp in _textureCache)
        {
            kvp.Value.Dispose();
        }

        _textureCache.Clear();
    }

    /// <summary>
    /// Releases all resources used by this <see cref="TextureService"/>.
    /// </summary>
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
            Clear();
        }

        IsDisposed = true;
    }
}
