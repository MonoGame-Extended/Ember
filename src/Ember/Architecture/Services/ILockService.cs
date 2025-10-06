using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages locked state for particle emitters, modifiers, and interpolators.
/// </summary>
public interface ILockService
{
    /// <summary>
    /// Determines whether the specified emitter is locked.
    /// </summary>
    /// <param name="emitter">The emitter to check.</param>
    /// <returns>
    /// <see langword="true"/> if the emitter is locked; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsLocked(ParticleEmitter emitter);

    /// <summary>
    /// Determines whether the specified modifier is locked.
    /// </summary>
    /// <param name="modifier">The modifier to check.</param>
    /// <returns>
    /// <see langword="true"/> if the modifier is locked; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsLocked(Modifier modifier);

    /// <summary>
    /// Determines whether the specified interpolator is locked.
    /// </summary>
    /// <param name="interpolator">The interpolator to check.</param>
    /// <returns>
    /// <see langword="true"/> if the interpolator is locked; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsLocked(Interpolator interpolator);

    /// <summary>
    /// Toggles the locked state of the specified emitter.
    /// </summary>
    /// <param name="emitter">The emitter to toggle.</param>
    /// <returns>
    /// The new locked state after toggling.
    /// </returns>
    bool ToggleLock(ParticleEmitter emitter);

    /// <summary>
    /// Toggles the locked state of the specified modifier.
    /// </summary>
    /// <param name="modifier">The modifier to toggle.</param>
    /// <returns>
    /// The new locked state after toggling.
    /// </returns>
    bool ToggleLock(Modifier modifier);

    /// <summary>
    /// Toggles the locked state of the specified interpolator.
    /// </summary>
    /// <param name="interpolator">The interpolator to toggle.</param>
    /// <returns>
    /// The new locked state after toggling.
    /// </returns>
    bool ToggleLock(Interpolator interpolator);

    /// <summary>
    /// Begins tracking the specified emitter with an unlocked state.
    /// </summary>
    /// <param name="emitter">The emitter to track.</param>
    void Track(ParticleEmitter emitter);

    /// <summary>
    /// Begins tracking the specified modifier with an unlocked state.
    /// </summary>
    /// <param name="modifier">The modifier to track.</param>
    void Track(Modifier modifier);

    /// <summary>
    /// Begins tracking the specified interpolator with an unlocked state.
    /// </summary>
    /// <param name="interpolator">The interpolator to track.</param>
    void Track(Interpolator interpolator);

    /// <summary>
    /// Stops tracking the specified emitter and removes its lock state.
    /// </summary>
    /// <param name="emitter">The emitter to untrack.</param>
    void Untrack(ParticleEmitter emitter);

    /// <summary>
    /// Stops tracking the specified modifier and removes its lock state.
    /// </summary>
    /// <param name="modifier">The modifier to untrack.</param>
    void Untrack(Modifier modifier);

    /// <summary>
    /// Stops tracking the specified interpolator and removes its lock state.
    /// </summary>
    /// <param name="interpolator">The interpolator to untrack.</param>
    void Untrack(Interpolator interpolator);

    /// <summary>
    /// Clears all tracked objects and their lock states.
    /// </summary>
    void Clear();
}
