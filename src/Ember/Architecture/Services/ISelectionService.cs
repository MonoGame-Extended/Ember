using System.Collections.Generic;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages selection state for particle emitters, modifiers, and interpolators.
/// </summary>
public interface ISelectionService
{
    /// <summary>
    /// Gets the currently selected particle emitter
    /// </summary>
    ParticleEmitter SelectedEmitter { get; }

    /// <summary>
    /// Gets the index of the currently selected particle emitter
    /// </summary>
    int SelectedEmitterIndex { get; }

    /// <summary>
    /// Gets the currently selected modifier
    /// </summary>
    Modifier SelectedModifier { get; }

    /// <summary>
    /// Gets the index of the currently selected modifier
    /// </summary>
    int SelectedModifierIndex { get; }

    /// <summary>
    /// Gets the currently selected interpolator
    /// </summary>
    Interpolator SelectedInterpolator { get; }

    /// <summary>
    /// Gets the index of the currently selected interpolator
    /// </summary>
    int SelectedInterpolatorIndex { get; }

    /// <summary>
    /// Gets the list of interpolators from the currently selected modifier
    /// </summary>
    /// <returns>
    /// A list of interpolators if the selected modifier supports them; otherwise <see langword="null"/>.
    /// </returns>
    List<Interpolator> GetCurrentInterpolators();

    /// <summary>
    /// Selects a particle emitter by index
    /// </summary>
    /// <param name="emitter">The emitter to select or <see langword="null"/> to clear selection</param>
    /// <param name="index">The index of the emitter</param>
    void SelectEmitter(ParticleEmitter emitter, int index);

    /// <summary>
    /// Selects a modifier by index
    /// </summary>
    /// <param name="modifier">The modifier to select, or <see langword="null"/> to clear selection</param>
    /// <param name="index">The index of the modifier</param>
    void SelectModifier(Modifier modifier, int index);

    /// <summary>
    /// Selects an interpolator by index
    /// </summary>
    /// <param name="interpolator">The interpolator to select, or <see langword="null"/> to clear selection</param>
    /// <param name="index">The index of the interpolator</param>
    void SelectInterpolator(Interpolator interpolator, int index);

    /// <summary>
    /// Clears all selections
    /// </summary>
    void ClearSelections();

}
