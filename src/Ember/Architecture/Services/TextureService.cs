using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages texture loading, caching, and assignment for particle emitters using ContentManager.
/// </summary>
public sealed class TextureService : ITextureService
{
    private readonly IPathService _pathService;
    private readonly IGraphicsDeviceService _graphicsDeviceService;
    private readonly Dictionary<string, Texture2D> _textureCache = [];

    public bool IsDisposed { get; private set; }

    public TextureService(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _pathService = services.GetService(typeof(IPathService)) as IPathService;
        _graphicsDeviceService = services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
    }

    ~TextureService() => Dispose(false);

    public void AddTexture(string filePath, Action<string, Action> onOverwriteConfirm = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        string fileName = Path.GetFileName(filePath);

        // If already in project directory, just load it
        if (_pathService.IsRelativeTo(filePath))
        {
            LoadTexture(fileName);
            return;
        }

        string destinationPath = Path.Combine(_pathService.GetWorkingDirectory(), fileName);

        // Check if file would be overwritten
        if (File.Exists(destinationPath))
        {
            if (onOverwriteConfirm != null)
            {
                onOverwriteConfirm(fileName, () => CopyAndLoad(filePath, fileName));
            }
            else
            {
                CopyAndLoad(filePath, fileName);
            }
        }
        else
        {
            CopyAndLoad(filePath, fileName);
        }
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

        Texture2D texture = Texture2D.FromFile(_graphicsDeviceService.GraphicsDevice, fileName);
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

    public IEnumerable<string> GetTextureNames()
    {
        return _textureCache.Keys;
    }

    public void Clear()
    {
        foreach (var kvp in _textureCache)
        {
            kvp.Value.Dispose();
        }

        _textureCache.Clear();
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
            Clear();
        }

        IsDisposed = true;
    }

    private void CopyAndLoad(string sourcePath, string fileName)
    {
        _pathService.CopyTo(sourcePath, overwrite: true);
        LoadTexture(fileName);
    }
}
