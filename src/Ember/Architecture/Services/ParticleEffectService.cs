using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Particles;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages the lifecycle and state of the current particle effect.
/// </summary>
public sealed class ParticleEffectService : IParticleEffectService
{
    /// <summary>
    /// Gets the current particle effect.
    /// </summary>
    public ParticleEffect Current { get; private set; }

    /// <summary>
    /// Creates a new particle effect with the specified name.
    /// </summary>
    /// <param name="name">The name of the particle effect.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/> or consists only of white-space characters.
    /// </exception>
    public void Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Clear();
        Current = new ParticleEffect(name);
    }

    /// <summary>
    /// Sets the current particle effect.
    /// </summary>
    /// <param name="effect">The particle effect to set as current.</param>
    public void SetCurrent(ParticleEffect effect)
    {
        Clear();
        Current = effect;
    }

    /// <summary>
    /// Centers the particle effect at the specified position.
    /// </summary>
    /// <param name="position">The position to center the effect at.</param>
    public void Center(Vector2 position)
    {
        if (Current != null)
        {
            Current.Position = position;
        }
    }

    /// <summary>
    /// Clears the current particle effect, disposing it if necessary.
    /// </summary>
    public void Clear()
    {
        if (Current != null)
        {
            Current.Dispose();
            Current = null;
        }
    }
}
