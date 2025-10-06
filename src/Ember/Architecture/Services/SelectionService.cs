using System.Collections.Generic;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages selection state for particle emitters, modifiers, and interpolators.
/// </summary>
public sealed class SelectionService : ISelectionService
{
    /// <summary>
    /// Gets the currently selected particle emitter.
    /// </summary>
    public ParticleEmitter SelectedEmitter { get; private set; }

    /// <summary>
    /// Gets the index of the currently selected particle emitter.
    /// </summary>
    public int SelectedEmitterIndex { get; private set; } = -1;

    /// <summary>
    /// Gets the currently selected modifier.
    /// </summary>
    public Modifier SelectedModifier { get; private set; }

    /// <summary>
    /// Gets the index of the currently selected modifier.
    /// </summary>
    public int SelectedModifierIndex { get; private set; } = -1;

    /// <summary>
    /// Gets the currently selected interpolator.
    /// </summary>
    public Interpolator SelectedInterpolator { get; private set; }

    /// <summary>
    /// Gets the index of the currently selected interpolator.
    /// </summary>
    public int SelectedInterpolatorIndex { get; private set; } = -1;

    /// <summary>
    /// Gets the list of interpolators from the currently selected modifier.
    /// </summary>
    /// <returns>
    /// A list of interpolators if the selected modifier supports them; otherwise, <see langword="null"/>.
    /// </returns>
    public List<Interpolator> GetCurrentInterpolators()
    {
        return SelectedModifier switch
        {
            AgeModifier age => age.Interpolators,
            VelocityModifier velocity => velocity.Interpolators,
            _ => null
        };
    }

    /// <summary>
    /// Selects a particle emitter by index.
    /// </summary>
    /// <param name="emitter">The emitter to select, or <see langword="null"/> to clear selection.</param>
    /// <param name="index">The index of the emitter.</param>
    public void SelectEmitter(ParticleEmitter emitter, int index)
    {
        SelectedEmitter = emitter;
        SelectedEmitterIndex = index;

        // Clear modifier and interpolator selections when emitter changes
        SelectModifier(null, -1);
    }

    /// <summary>
    /// Selects a modifier by index.
    /// </summary>
    /// <param name="modifier">The modifier to select, or <see langword="null"/> to clear selection.</param>
    /// <param name="index">The index of the modifier.</param>
    public void SelectModifier(Modifier modifier, int index)
    {
        SelectedModifier = modifier;
        SelectedModifierIndex = index;

        // Clear interpolator selection when modifier changes
        SelectInterpolator(null, -1);
    }

    /// <summary>
    /// Selects an interpolator by index.
    /// </summary>
    /// <param name="interpolator">The interpolator to select, or <see langword="null"/> to clear selection.</param>
    /// <param name="index">The index of the interpolator.</param>
    public void SelectInterpolator(Interpolator interpolator, int index)
    {
        SelectedInterpolator = interpolator;
        SelectedInterpolatorIndex = index;
    }

    /// <summary>
    /// Clears all selections.
    /// </summary>
    public void ClearSelections()
    {
        SelectEmitter(null, -1);
    }
}
