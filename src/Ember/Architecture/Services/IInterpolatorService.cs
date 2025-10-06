using System;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages particle interpolator operations including creation, removal, and reordering.
/// </summary>
public interface IInterpolatorService
{
    /// <summary>
    /// Adds a new interpolator of the specified type to the currently selected modifier.
    /// </summary>
    /// <param name="interpolatorType">The type of interpolator to create.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="interpolatorType"/> is not a recognized interpolator type.
    /// </exception>
    void Add(Type interpolatorType);

    /// <summary>
    /// Removes the interpolator at the specified index from the currently selected modifier.
    /// </summary>
    /// <param name="index">The index of the interpolator to remove.</param>
    void Remove(int index);

    /// <summary>
    /// Reorders an interpolator from one index to another within the currently selected modifier.
    /// </summary>
    /// <param name="fromIndex">The current index of the interpolator.</param>
    /// <param name="toIndex">The target index for the interpolator.</param>
    void Reorder(int fromIndex, int toIndex);

    /// <summary>
    /// Selects the interpolator at the specified index within the currently selected modifier.
    /// </summary>
    /// <param name="index">The index of the interpolator to select.</param>
    void Select(int index);
}
