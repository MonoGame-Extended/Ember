using System.Collections.Generic;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages locked state for particle emitters, modifiers, and interpolators.
/// </summary>
public sealed class LockService : ILockService
{
    private readonly Dictionary<ParticleEmitter, bool> _emitterLocks = [];
    private readonly Dictionary<Modifier, bool> _modifierLocks = [];
    private readonly Dictionary<Interpolator, bool> _interpolatorLocks = [];

    /// <summary>
    /// Determines whether the specified emitter is locked.
    /// </summary>
    /// <param name="emitter">The emitter to check.</param>
    /// <returns>
    /// <see langword="true"/> if the emitter is locked; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsLocked(ParticleEmitter emitter)
    {
        return _emitterLocks.GetValueOrDefault(emitter, false);
    }

    /// <summary>
    /// Determines whether the specified modifier is locked.
    /// </summary>
    /// <param name="modifier">The modifier to check.</param>
    /// <returns>
    /// <see langword="true"/> if the modifier is locked; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsLocked(Modifier modifier)
    {
        return _modifierLocks.GetValueOrDefault(modifier, false);
    }

    /// <summary>
    /// Determines whether the specified interpolator is locked.
    /// </summary>
    /// <param name="interpolator">The interpolator to check.</param>
    /// <returns>
    /// <see langword="true"/> if the interpolator is locked; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsLocked(Interpolator interpolator)
    {
        return _interpolatorLocks.GetValueOrDefault(interpolator, false);
    }

    /// <summary>
    /// Toggles the locked state of the specified emitter.
    /// </summary>
    /// <param name="emitter">The emitter to toggle.</param>
    /// <returns>
    /// The new locked state after toggling.
    /// </returns>
    public bool ToggleLock(ParticleEmitter emitter)
    {
        return _emitterLocks[emitter] = !IsLocked(emitter);
    }

    /// <summary>
    /// Toggles the locked state of the specified modifier.
    /// </summary>
    /// <param name="modifier">The modifier to toggle.</param>
    /// <returns>
    /// The new locked state after toggling.
    /// </returns>
    public bool ToggleLock(Modifier modifier)
    {
        return _modifierLocks[modifier] = !IsLocked(modifier);
    }

    /// <summary>
    /// Toggles the locked state of the specified interpolator.
    /// </summary>
    /// <param name="interpolator">The interpolator to toggle.</param>
    /// <returns>
    /// The new locked state after toggling.
    /// </returns>
    public bool ToggleLock(Interpolator interpolator)
    {
        return _interpolatorLocks[interpolator] = !IsLocked(interpolator);
    }

    /// <summary>
    /// Begins tracking the specified emitter with an unlocked state.
    /// </summary>
    /// <param name="emitter">The emitter to track.</param>
    public void Track(ParticleEmitter emitter)
    {
        _emitterLocks[emitter] = false;
    }

    /// <summary>
    /// Begins tracking the specified modifier with an unlocked state.
    /// </summary>
    /// <param name="modifier">The modifier to track.</param>
    public void Track(Modifier modifier)
    {
        _modifierLocks[modifier] = false;
    }

    /// <summary>
    /// Begins tracking the specified interpolator with an unlocked state.
    /// </summary>
    /// <param name="interpolator">The interpolator to track.</param>
    public void Track(Interpolator interpolator)
    {
        _interpolatorLocks[interpolator] = false;
    }

    /// <summary>
    /// Stops tracking the specified emitter and removes its lock state.
    /// </summary>
    /// <param name="emitter">The emitter to untrack.</param>
    public void Untrack(ParticleEmitter emitter)
    {
        _emitterLocks.Remove(emitter);
    }

    /// <summary>
    /// Stops tracking the specified modifier and removes its lock state.
    /// </summary>
    /// <param name="modifier">The modifier to untrack.</param>
    public void Untrack(Modifier modifier)
    {
        _modifierLocks.Remove(modifier);
    }

    /// <summary>
    /// Stops tracking the specified interpolator and removes its lock state.
    /// </summary>
    /// <param name="interpolator">The interpolator to untrack.</param>
    public void Untrack(Interpolator interpolator)
    {
        _interpolatorLocks.Remove(interpolator);
    }

    /// <summary>
    /// Clears all tracked objects and their lock states.
    /// </summary>
    public void Clear()
    {
        _emitterLocks.Clear();
        _modifierLocks.Clear();
        _interpolatorLocks.Clear();
    }
}
