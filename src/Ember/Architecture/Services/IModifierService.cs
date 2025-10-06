using System;
using MonoGame.Extended.Particles.Modifiers;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages particle modifier operations including creation, removal, and reordering.
/// </summary>
public interface IModifierService
{
    /// <summary>
    /// Adds a new modifier of the specified type to the currently selected emitter.
    /// </summary>
    /// <param name="modifierType">The type of modifier to create.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="modifierType"/> is not a recognized modifier type.
    /// </exception>
    void Add(Type modifierType);

    /// <summary>
    /// Removes the modifier at the specified index from the currently selected emitter.
    /// </summary>
    /// <param name="index">The index of the modifier to remove.</param>
    void Remove(int index);

    /// <summary>
    /// Reorders a modifier from one index to another within the currently selected emitter.
    /// </summary>
    /// <param name="fromIndex">The current index of the modifier.</param>
    /// <param name="toIndex">The target index for the modifier.</param>
    void Reorder(int fromIndex, int toIndex);

    /// <summary>
    /// Selects the modifier at the specified index within the currently selected emitter.
    /// </summary>
    /// <param name="index">The index of the modifier to select.</param>
    void Select(int index);

    /// <summary>
    /// Determines whether the specified modifier supports interpolators.
    /// </summary>
    /// <param name="modifier">The modifier to check.</param>
    /// <returns>
    /// <see langword="true"/> if the modifier supports interpolators; otherwise, <see langword="false"/>.
    /// </returns>
    bool SupportsInterpolators(Modifier modifier);
}
