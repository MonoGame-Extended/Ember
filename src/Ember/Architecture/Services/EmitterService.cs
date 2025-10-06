using System;
using MonoGame.Extended.Particles;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages particle emitter operations including creation, removal, and reordering.
/// </summary>
public sealed class EmitterService : IEmitterService
{
    private readonly IParticleEffectService _particleEffectService;
    private readonly ISelectionService _selectionService;
    private readonly ILockService _lockService;
    private readonly IProjectService _projectService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmitterService"/> class.
    /// </summary>
    /// <param name="services">The service provider used to resolve dependencies.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public EmitterService(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _particleEffectService = services.GetService(typeof(IParticleEffectService)) as IParticleEffectService;
        _selectionService = services.GetService(typeof(ISelectionService)) as ISelectionService;
        _lockService = services.GetService(typeof(ILockService)) as ILockService;
        _projectService = services.GetService(typeof(IProjectService)) as IProjectService;
    }

    /// <summary>
    /// Adds a new particle emitter to the current particle effect.
    /// </summary>
    public void Add()
    {
        ParticleEffect effect = _particleEffectService.Current;
        if (effect == null)
        {
            return;
        }

        ParticleEmitter emitter = new(1000)
        {
            Name = nameof(ParticleEmitter)
        };

        int index = effect.Emitters.Count;
        effect.Emitters.Add(emitter);
        _lockService.Track(emitter);
        Select(index);
        _projectService.HasUnsavedChanges = true;
    }

    /// <summary>
    /// Removes the particle emitter at the specified index.
    /// </summary>
    /// <param name="index">The index of the emitter to remove.</param>
    public void Remove(int index)
    {
        ParticleEffect effect = _particleEffectService.Current;
        if (effect == null || index < 0 || index >= effect.Emitters.Count)
        {
            return;
        }

        ParticleEmitter emitter = effect.Emitters[index];
        effect.Emitters.RemoveAt(index);
        _lockService.Untrack(emitter);

        // Update selection if we removed the selected emitter
        if (emitter == _selectionService.SelectedEmitter)
        {
            int newIndex = Math.Max(0, index - 1);

            if (effect.Emitters.Count > 0)
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
    /// Reorders a particle emitter from one index to another.
    /// </summary>
    /// <param name="fromIndex">The current index of the emitter.</param>
    /// <param name="toIndex">The target index for the emitter.</param>
    public void Reorder(int fromIndex, int toIndex)
    {
        ParticleEffect effect = _particleEffectService.Current;
        if (effect == null || fromIndex < 0 || fromIndex >= effect.Emitters.Count || toIndex < 0 || toIndex >= effect.Emitters.Count)
        {
            return;
        }

        ParticleEmitter moving = effect.Emitters[fromIndex];
        effect.Emitters.RemoveAt(fromIndex);
        effect.Emitters.Insert(toIndex, moving);

        // Maintain current selection by re-selecting at the potentially new index
        if (_selectionService.SelectedEmitter != null)
        {
            int currentIndex = effect.Emitters.IndexOf(_selectionService.SelectedEmitter);
            Select(currentIndex);
        }

        _projectService.HasUnsavedChanges = true;
    }

    /// <summary>
    /// Selects the particle emitter at the specified index.
    /// </summary>
    /// <param name="index">The index of the emitter to select.</param>
    public void Select(int index)
    {
        ParticleEffect effect = _particleEffectService.Current;
        if (effect == null || index < 0 || index >= effect.Emitters.Count)
        {
            _selectionService.SelectEmitter(null, -1);
        }
        else
        {
            _selectionService.SelectEmitter(effect.Emitters[index], index);
        }
    }
}
