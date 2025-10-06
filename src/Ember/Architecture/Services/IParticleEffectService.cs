using Microsoft.Xna.Framework;
using MonoGame.Extended.Particles;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages the lifecycle and state of the current particle effect.
/// </summary>
public interface IParticleEffectService
{
    /// <summary>
    /// Gets the current particle effect.
    /// </summary>
    ParticleEffect Current { get; }

    /// <summary>
    /// Creates a new particle effect with the specified name.
    /// </summary>
    /// <param name="name">The name of the particle effect.</param>
    void Create(string name);

    /// <summary>
    /// Sets the current particle effect.
    /// </summary>
    /// <param name="effect">The particle effect to set as current.</param>
    void SetCurrent(ParticleEffect effect);

    /// <summary>
    /// Centers the particle effect at the specified position.
    /// </summary>
    /// <param name="position">The position to center the effect at.</param>
    void Center(Vector2 position);

    /// <summary>
    /// Clears the current particle effect, disposing it if necessary.
    /// </summary>
    void Clear();
}
