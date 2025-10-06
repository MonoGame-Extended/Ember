using System;
using MonoGame.Extended.Particles;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages particle emitter operations including creation, removal, and reordering.
/// </summary>
public interface IEmitterService
{
    /// <summary>
    /// Adds a new particle emitter to the current particle effect.
    /// </summary>
    void Add();

    /// <summary>
    /// Removes the particle emitter at the specified index.
    /// </summary>
    /// <param name="index">The index of the emitter to remove.</param>
    void Remove(int index);

    /// <summary>
    /// Reorders a particle emitter from one index to another.
    /// </summary>
    /// <param name="fromIndex">The current index of the emitter.</param>
    /// <param name="toIndex">The target index for the emitter.</param>
    void Reorder(int fromIndex, int toIndex);

    /// <summary>
    /// Selects the particle emitter at the specified index.
    /// </summary>
    /// <param name="index">The index of the emitter to select.</param>
    void Select(int index);
}
