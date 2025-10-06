using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages particle interpolator operations including creation, removal, and reordering.
/// </summary>
public sealed class InterpolatorService : IInterpolatorService
{
    private readonly ISelectionService _selectionService;
    private readonly ILockService _lockService;
    private readonly IProjectService _projectService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterpolatorService"/> class.
    /// </summary>
    /// <param name="services">The service provider used to resolve dependencies.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public InterpolatorService(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _selectionService = services.GetService(typeof(ISelectionService)) as ISelectionService;
        _lockService = services.GetService(typeof(ILockService)) as ILockService;
        _projectService = services.GetService(typeof(IProjectService)) as IProjectService;
    }

    /// <summary>
    /// Adds a new interpolator of the specified type to the currently selected modifier.
    /// </summary>
    /// <param name="interpolatorType">The type of interpolator to create.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="interpolatorType"/> is not a recognized interpolator type.
    /// </exception>
    public void Add(Type interpolatorType)
    {
        List<Interpolator> interpolators = _selectionService.GetCurrentInterpolators();
        if (interpolators == null)
        {
            return;
        }

        Interpolator interpolator = CreateInterpolator(interpolatorType);
        int index = interpolators.Count;
        interpolators.Add(interpolator);
        _lockService.Track(interpolator);
        Select(index);
        _projectService.HasUnsavedChanges = true;
    }

    /// <summary>
    /// Removes the interpolator at the specified index from the currently selected modifier.
    /// </summary>
    /// <param name="index">The index of the interpolator to remove.</param>
    public void Remove(int index)
    {
        List<Interpolator> interpolators = _selectionService.GetCurrentInterpolators();
        if (interpolators == null || index < 0 || index >= interpolators.Count)
        {
            return;
        }

        Interpolator interpolator = interpolators[index];
        interpolators.RemoveAt(index);
        _lockService.Untrack(interpolator);

        // Update selection if we removed the selected interpolator
        if (interpolator == _selectionService.SelectedInterpolator)
        {
            int newIndex = Math.Max(0, index - 1);

            if (interpolators.Count > 0)
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
    /// Reorders an interpolator from one index to another within the currently selected modifier.
    /// </summary>
    /// <param name="fromIndex">The current index of the interpolator.</param>
    /// <param name="toIndex">The target index for the interpolator.</param>
    public void Reorder(int fromIndex, int toIndex)
    {
        List<Interpolator> interpolators = _selectionService.GetCurrentInterpolators();
        if (interpolators == null || fromIndex < 0 || fromIndex >= interpolators.Count || toIndex < 0 || toIndex >= interpolators.Count)
        {
            return;
        }

        Interpolator moving = interpolators[fromIndex];
        interpolators.RemoveAt(fromIndex);
        interpolators.Insert(toIndex, moving);

        // Maintain current selection by re-selecting at the potentially new index
        if (_selectionService.SelectedInterpolator != null)
        {
            int currentIndex = interpolators.IndexOf(_selectionService.SelectedInterpolator);
            Select(currentIndex);
        }

        _projectService.HasUnsavedChanges = true;
    }

    /// <summary>
    /// Selects the interpolator at the specified index within the currently selected modifier.
    /// </summary>
    /// <param name="index">The index of the interpolator to select.</param>
    public void Select(int index)
    {
        List<Interpolator> interpolators = _selectionService.GetCurrentInterpolators();
        if (interpolators == null || index < 0 || index >= interpolators.Count)
        {
            _selectionService.SelectInterpolator(null, -1);
        }
        else
        {
            _selectionService.SelectInterpolator(interpolators[index], index);
        }
    }

    private static Interpolator CreateInterpolator(Type interpolatorType)
    {
        if (interpolatorType == typeof(ColorInterpolator))
            return new ColorInterpolator() { StartValue = new HslColor(0.0f, 0.0f, 0.0f), EndValue = new HslColor(0.0f, 0.0f, 1.0f) };

        if (interpolatorType == typeof(HueInterpolator))
            return new HueInterpolator() { StartValue = 0.0f, EndValue = 1.0f };

        if (interpolatorType == typeof(OpacityInterpolator))
            return new OpacityInterpolator() { StartValue = 0.0f, EndValue = 1.0f };

        if (interpolatorType == typeof(RotationInterpolator))
            return new RotationInterpolator() { StartValue = 0.0f, EndValue = MathF.PI / 2.0f };

        if (interpolatorType == typeof(ScaleInterpolator))
            return new ScaleInterpolator() { StartValue = Vector2.One, EndValue = Vector2.Zero };

        if (interpolatorType == typeof(VelocityInterpolator))
            return new VelocityInterpolator() { StartValue = Vector2.Zero, EndValue = Vector2.One };

        throw new InvalidOperationException($"Unknown interpolator type '{interpolatorType.Name}'");
    }
}
