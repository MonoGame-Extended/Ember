using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages particle modifier operations including creation, removal, and reordering.
/// </summary>
public sealed class ModifierService : IModifierService
{
    private readonly ISelectionService _selectionService;
    private readonly ILockService _lockService;
    private readonly IProjectService _projectService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifierService"/> class.
    /// </summary>
    /// <param name="services">The service provider used to resolve dependencies.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public ModifierService(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _selectionService = services.GetService(typeof(ISelectionService)) as ISelectionService;
        _lockService = services.GetService(typeof(ILockService)) as ILockService;
        _projectService = services.GetService(typeof(IProjectService)) as IProjectService;
    }

    /// <summary>
    /// Adds a new modifier of the specified type to the currently selected emitter.
    /// </summary>
    /// <param name="modifierType">The type of modifier to create.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="modifierType"/> is not a recognized modifier type.
    /// </exception>
    public void Add(Type modifierType)
    {
        ParticleEmitter emitter = _selectionService.SelectedEmitter;
        if (emitter == null)
        {
            return;
        }

        Modifier modifier = CreateModifier(modifierType);
        int index = emitter.Modifiers.Count;
        emitter.Modifiers.Add(modifier);
        _lockService.Track(modifier);
        Select(index);
        _projectService.HasUnsavedChanges = true;
    }

    /// <summary>
    /// Removes the modifier at the specified index from the currently selected emitter.
    /// </summary>
    /// <param name="index">The index of the modifier to remove.</param>
    public void Remove(int index)
    {
        ParticleEmitter emitter = _selectionService.SelectedEmitter;
        if (emitter == null || index < 0 || index >= emitter.Modifiers.Count)
        {
            return;
        }

        Modifier modifier = emitter.Modifiers[index];
        emitter.Modifiers.RemoveAt(index);
        _lockService.Untrack(modifier);

        // Update selection if we removed the selected modifier
        if (modifier == _selectionService.SelectedModifier)
        {
            int newIndex = Math.Max(0, index - 1);

            if (emitter.Modifiers.Count > 0)
            {
                Select(newIndex);
            }
            else
            {
                Select(-1);
            }
        }

        _projectService.HasUnsavedChanges = true;
    }

    /// <summary>
    /// Reorders a modifier from one index to another within the currently selected emitter.
    /// </summary>
    /// <param name="fromIndex">The current index of the modifier.</param>
    /// <param name="toIndex">The target index for the modifier.</param>
    public void Reorder(int fromIndex, int toIndex)
    {
        ParticleEmitter emitter = _selectionService.SelectedEmitter;
        if (emitter == null || fromIndex < 0 || fromIndex >= emitter.Modifiers.Count || toIndex < 0 || toIndex >= emitter.Modifiers.Count)
        {
            return;
        }

        Modifier moving = emitter.Modifiers[fromIndex];
        emitter.Modifiers.RemoveAt(fromIndex);
        emitter.Modifiers.Insert(toIndex, moving);

        // Maintain current selection by re-selecting at the potentially new index
        if (_selectionService.SelectedModifier != null)
        {
            int currentIndex = emitter.Modifiers.IndexOf(_selectionService.SelectedModifier);
            Select(currentIndex);
        }

        _projectService.HasUnsavedChanges = true;
    }

    /// <summary>
    /// Selects the modifier at the specified index within the currently selected emitter.
    /// </summary>
    /// <param name="index">The index of the modifier to select.</param>
    public void Select(int index)
    {
        ParticleEmitter emitter = _selectionService.SelectedEmitter;
        if (emitter == null || index < 0 || index >= emitter.Modifiers.Count)
        {
            _selectionService.SelectModifier(null, -1);
        }
        else
        {
            _selectionService.SelectModifier(emitter.Modifiers[index], index);
        }
    }

    /// <summary>
    /// Determines whether the specified modifier supports interpolators.
    /// </summary>
    /// <param name="modifier">The modifier to check.</param>
    /// <returns>
    /// <see langword="true"/> if the modifier supports interpolators; otherwise, <see langword="false"/>.
    /// </returns>
    public bool SupportsInterpolators(Modifier modifier)
    {
        return modifier is AgeModifier || modifier is VelocityModifier;
    }

    private static Modifier CreateModifier(Type modifierType)
    {
        if (modifierType == typeof(RectangleLoopContainerModifier))
            return new RectangleLoopContainerModifier() { Width = 100, Height = 100 };

        if (modifierType == typeof(RectangleContainerModifier))
            return new RectangleContainerModifier { Width = 100, Height = 100 };

        if (modifierType == typeof(LinearGravityModifier))
            return new LinearGravityModifier() { Direction = Vector2.UnitY, Strength = 100.0f };

        if (modifierType == typeof(VortexModifier))
            return new VortexModifier() { };

        if (modifierType == typeof(OpacityFastFadeModifier))
            return new OpacityFastFadeModifier();

        if (modifierType == typeof(AgeModifier))
            return new AgeModifier() { Interpolators = new List<Interpolator>() { new ScaleInterpolator() { StartValue = Vector2.Zero, EndValue = Vector2.One } } };

        if (modifierType == typeof(CircleContainerModifier))
            return new CircleContainerModifier() { Radius = 100.0f };

        if (modifierType == typeof(DragModifier))
            return new DragModifier();

        if (modifierType == typeof(RotationModifier))
            return new RotationModifier() { RotationRate = MathF.PI / 4.0f };

        if (modifierType == typeof(VelocityColorModifier))
            return new VelocityColorModifier() { VelocityThreshold = 100.0f, StationaryColor = new HslColor(0, 0, 1.0f), VelocityColor = new HslColor(0, 1.0f, 0.5f) };

        if (modifierType == typeof(VelocityModifier))
            return new VelocityModifier() { VelocityThreshold = 100.0f, Interpolators = new List<Interpolator>() { new ScaleInterpolator() { StartValue = Vector2.Zero, EndValue = Vector2.One } } };

        throw new InvalidOperationException($"Unknown modifier type '{modifierType.Name}'");
    }
}
