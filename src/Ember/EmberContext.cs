// // Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// // Licensed under the MIT license.
// // See LICENSE file in the project root for full license information.

// using System;
// using System.Collections.Generic;
// using System.IO;
// using Hexa.NET.ImGui;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Content;
// using Microsoft.Xna.Framework.Graphics;
// using MonoGame.Extended;
// using MonoGame.Extended.Particles;
// using MonoGame.Extended.Particles.Modifiers;
// using MonoGame.Extended.Particles.Modifiers.Containers;
// using MonoGame.Extended.Particles.Modifiers.Interpolators;

// namespace Ember;

// public static class EmberContext
// {
//     private static Game s_game;
//     private static GraphicsDevice s_graphicsDevice;
//     private static ContentManager s_contentManager;
//     private static float s_baseFontSize = 16.0f;
//     private static float s_fontScaleMain = 1.0f;
//     public static XnaColor ClearColor { get; set; }
//     private static readonly Dictionary<ParticleEmitter, bool> s_emitterLocks = [];
//     private static readonly Dictionary<Modifier, bool> s_modifierLocks = [];
//     private static readonly Dictionary<Interpolator, bool> s_interpolatorLocks = [];
//     private static readonly Dictionary<string, Texture2D> s_textureCache = [];

//     public static string ProjectName { get; private set; } = string.Empty;
//     public static string ProjectDirectory { get; private set; } = string.Empty;
//     public static string ProjectFilePath { get; private set; } = string.Empty;
//     public static bool HasUnsavedChanges { get; set; }

//     public static ParticleEffect ParticleEffect { get; private set; }

//     public static ParticleEmitter SelectedParticleEmitter { get; private set; }
//     public static int SelectedParticleEmitterIndex { get; private set; }

//     public static Modifier SelectedModifier { get; private set; }
//     public static int SelectedModifierIndex { get; private set; }

//     public static Interpolator SelectedInterpolator { get; private set; }
//     public static List<Interpolator> CurrentInterpolators { get; private set; }
//     public static int SelectedInterpolatorIndex { get; private set; }

//     public static float BaseFontSize
//     {
//         get => s_baseFontSize;
//         set
//         {
//             if (Math.Abs(s_baseFontSize - value) > 0.01f)
//             {
//                 s_baseFontSize = Math.Clamp(value, 8.0f, 72.0f);
//                 ApplyFontScaling();
//             }
//         }
//     }

//     public static float FontScaleMain
//     {
//         get => s_fontScaleMain;
//         set
//         {
//             if (Math.Abs(s_fontScaleMain - value) > 0.01f)
//             {
//                 s_fontScaleMain = Math.Clamp(value, 0.5f, 4.0f);
//                 ApplyFontScaling();
//             }
//         }
//     }

//     public static float EffectiveFontSize => s_baseFontSize * s_fontScaleMain;

//     public static void Initialize(Game game, float initialBaseFontSize = 16.0f, XnaColor? initialClearColor = null)
//     {
//         ArgumentNullException.ThrowIfNull(game);

//         s_game = game;
//         s_graphicsDevice = game.GraphicsDevice;
//         s_contentManager = game.Content;
//         s_baseFontSize = initialBaseFontSize;
//         s_fontScaleMain = 1.0f;
//         ClearColor = initialClearColor ?? XnaColor.Black;

//         ApplyFontScaling();
//     }

//     private static void ApplyFontScaling()
//     {
//         ImGuiStylePtr stylePtr = ImGui.GetStyle();

//         stylePtr.FontSizeBase = s_baseFontSize;
//         stylePtr.FontScaleMain = s_fontScaleMain;
//         stylePtr.FontScaleDpi = 1.0f;

//         // Temporary hack from ImGui demo until font atlas rebuilding is finalized
//         stylePtr.NextFrameFontSizeBase = s_baseFontSize;
//     }

//     public static void CreateProject(string projectName, string projectDirectory, bool createProjectDirectory)
//     {
//         ArgumentException.ThrowIfNullOrWhiteSpace(projectName);
//         ArgumentException.ThrowIfNullOrWhiteSpace(projectDirectory);

//         ClearContext();

//         ProjectName = projectName;
//         ProjectDirectory = projectDirectory;

//         if (createProjectDirectory)
//         {
//             ProjectDirectory = Path.Combine(ProjectDirectory, ProjectName);
//         }

//         ProjectFilePath = Path.Combine(ProjectDirectory, ProjectName);
//         ProjectFilePath = Path.ChangeExtension(ProjectFilePath, ".ember");

//         Directory.CreateDirectory(ProjectDirectory);

//         s_contentManager.RootDirectory = ProjectDirectory;

//         ParticleEffect = new(ProjectName);
//         CenterParticleEffect();
//         HasUnsavedChanges = true;
//         SaveProject();
//     }

//     public static void OpenProject(string filePath)
//     {
//         ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

//         ClearContext();

//         ProjectName = Path.GetFileNameWithoutExtension(filePath);
//         ProjectDirectory = Path.GetDirectoryName(filePath);
//         ProjectFilePath = filePath;

//         s_contentManager.RootDirectory = ProjectDirectory;

//         using ParticleEffectReader reader = new(ProjectFilePath, s_contentManager);
//         ParticleEffect = reader.ReadParticleEffect();
//         CenterParticleEffect();
//     }

//     public static void SaveProject()
//     {
//         if (ParticleEffect == null)
//         {
//             return;
//         }

//         using ParticleEffectWriter writer = new(ProjectFilePath);
//         writer.WriteParticleEffect(ParticleEffect);

//         HasUnsavedChanges = false;
//     }

//     private static void ClearContext()
//     {
//         if (ParticleEffect != null)
//         {
//             ParticleEffect.Dispose();
//             ParticleEffect = null;
//         }

//         s_contentManager.Unload();

//         SelectedParticleEmitter = null;
//         SelectedParticleEmitterIndex = -1;
//         SelectedModifier = null;
//         SelectedModifierIndex = -1;
//         SelectedInterpolator = null;
//         SelectedInterpolatorIndex = -1;
//         CurrentInterpolators = null;
//         s_emitterLocks.Clear();
//         s_modifierLocks.Clear();
//         s_interpolatorLocks.Clear();
//         s_textureCache.Clear();

//         GC.Collect();
//     }

//     public static void Exit()
//     {
//         if (HasUnsavedChanges)
//         {
//             UnsavedChangesModal.Open((result) =>
//             {
//                 if (result.Status == ModalResult.Success)
//                 {
//                     SaveProject();
//                     s_game.Exit();
//                 }
//                 else if (result.Status == ModalResult.Cancel)
//                 {
//                     // They canceled, don't save, but don't exit
//                     return;
//                 }
//                 else if (result.Status == ModalResult.Error)
//                 {
//                     // Those choose to not save, we warned them
//                     s_game.Exit();
//                 }
//             });
//             return;
//         }

//         s_game.Exit();
//     }

//     public static void CenterParticleEffect()
//     {
//         if (ParticleEffect == null)
//         {
//             return;
//         }

//         ParticleEffect.Position = s_graphicsDevice.Viewport.Bounds.Center.ToVector2();
//     }

//     public static void AddEmitter()
//     {
//         if (ParticleEffect == null)
//         {
//             return;
//         }

//         ParticleEmitter emitter = new(1000);
//         emitter.Name = nameof(ParticleEmitter);
//         int index = ParticleEffect.Emitters.Count;
//         ParticleEffect.Emitters.Add(emitter);
//         TrackObject(emitter);
//         SelectEmitter(index);
//         HasUnsavedChanges = true;
//     }

//     public static void RemoveEmitter(int index)
//     {
//         if (ParticleEffect == null || index < 0 || index >= ParticleEffect.Emitters.Count)
//         {
//             return;
//         }

//         ParticleEmitter emitter = ParticleEffect.Emitters[index];
//         ParticleEffect.Emitters.RemoveAt(index);
//         UntrackObject(emitter);

//         if (emitter == SelectedParticleEmitter)
//         {
//             index = Math.Max(0, index - 1);

//             if (ParticleEffect.Emitters.Count > 0)
//             {
//                 SelectEmitter(index);
//             }
//             else
//             {
//                 SelectEmitter(-1);
//             }
//         }

//         HasUnsavedChanges = true;
//     }

//     public static void SelectEmitter(int index)
//     {
//         if (ParticleEffect == null || index < 0 || index >= ParticleEffect.Emitters.Count)
//         {
//             SelectedParticleEmitter = null;
//             SelectedParticleEmitterIndex = -1;
//         }
//         else
//         {
//             SelectedParticleEmitter = ParticleEffect.Emitters[index];
//             SelectedParticleEmitterIndex = index;
//         }
//     }

//     public static void ReorderEmitter(int from, int to)
//     {
//         if (ParticleEffect == null || from < 0 || from >= ParticleEffect.Emitters.Count || to < 0 || to >= ParticleEffect.Emitters.Count)
//         {
//             return;
//         }

//         ParticleEmitter moving = ParticleEffect.Emitters[from];
//         ParticleEffect.Emitters.RemoveAt(from);
//         ParticleEffect.Emitters.Insert(to, moving);
//         SelectEmitter(SelectedParticleEmitterIndex);
//         HasUnsavedChanges = true;
//     }

//     public static void AddModifier(Type modifierType)
//     {
//         if (SelectedParticleEmitter == null)
//         {
//             return;
//         }

//         Modifier modifier = CreateModifier(modifierType);
//         int index = SelectedParticleEmitter.Modifiers.Count;
//         SelectedParticleEmitter.Modifiers.Add(modifier);
//         TrackObject(modifier);
//         SelectModifier(index);
//         HasUnsavedChanges = true;
//     }

//     public static void RemoveModifier(int index)
//     {
//         if (SelectedParticleEmitter == null || index < 0 || index >= SelectedParticleEmitter.Modifiers.Count)
//         {
//             return;
//         }

//         Modifier modifier = SelectedParticleEmitter.Modifiers[index];
//         SelectedParticleEmitter.Modifiers.RemoveAt(index);
//         UntrackObject(modifier);

//         if (modifier == SelectedModifier)
//         {
//             index = Math.Max(0, index - 1);
//             if (SelectedParticleEmitter.Modifiers.Count > 0)
//             {
//                 SelectModifier(index);
//             }
//             else
//             {
//                 SelectModifier(-1);
//             }
//         }

//         HasUnsavedChanges = true;
//     }

//     public static void SelectModifier(int index)
//     {
//         if (SelectedParticleEmitter == null || index < 0 || index >= SelectedParticleEmitter.Modifiers.Count)
//         {
//             SelectedModifier = null;
//             SelectedModifierIndex = -1;
//         }
//         else
//         {
//             SelectedModifier = SelectedParticleEmitter.Modifiers[index];
//             SelectedModifierIndex = index;

//             CurrentInterpolators = SelectedModifier switch
//             {
//                 AgeModifier age => age.Interpolators,
//                 VelocityModifier velocity => velocity.Interpolators,
//                 _ => null
//             };
//         }
//     }

//     public static void ReorderModifier(int from, int to)
//     {
//         if (SelectedParticleEmitter == null || from < 0 || from >= SelectedParticleEmitter.Modifiers.Count || to < 0 || to >= SelectedParticleEmitter.Modifiers.Count)
//         {
//             return;
//         }

//         Modifier moving = SelectedParticleEmitter.Modifiers[from];
//         SelectedParticleEmitter.Modifiers.RemoveAt(from);
//         SelectedParticleEmitter.Modifiers.Insert(to, moving);
//         SelectModifier(SelectedModifierIndex);
//         HasUnsavedChanges = true;
//     }

//     private static Modifier CreateModifier(Type modifierType)
//     {
//         if (modifierType == typeof(RectangleLoopContainerModifier)) return new RectangleLoopContainerModifier() { Width = 100, Height = 100 };
//         if (modifierType == typeof(RectangleContainerModifier)) return new RectangleContainerModifier { Width = 100, Height = 100 };
//         if (modifierType == typeof(LinearGravityModifier)) return new LinearGravityModifier() { Direction = Vector2.UnitY, Strength = 100.0f };
//         if (modifierType == typeof(VortexModifier)) return new VortexModifier() {  };
//         if (modifierType == typeof(OpacityFastFadeModifier)) return new OpacityFastFadeModifier();
//         if (modifierType == typeof(AgeModifier)) return new AgeModifier() { Interpolators = new List<Interpolator>() { new ScaleInterpolator() { StartValue = Vector2.Zero, EndValue = Vector2.One } } };
//         if (modifierType == typeof(CircleContainerModifier)) return new CircleContainerModifier() { Radius = 100.0f };
//         if (modifierType == typeof(DragModifier)) return new DragModifier();
//         if (modifierType == typeof(RotationModifier)) return new RotationModifier() { RotationRate = MathF.PI / 4.0f };
//         if (modifierType == typeof(VelocityColorModifier)) return new VelocityColorModifier() { VelocityThreshold = 100.0f, StationaryColor = new HslColor(0, 0, 1.0f), VelocityColor = new HslColor(0, 1.0f, 0.5f) };
//         if (modifierType == typeof(VelocityModifier)) return new VelocityModifier() { VelocityThreshold = 100.0f, Interpolators = new List<Interpolator>() { new ScaleInterpolator() { StartValue = Vector2.Zero, EndValue = Vector2.One } } };

//         throw new InvalidOperationException($"Unknown modifier type '{modifierType.Name}'");
//     }

//     public static bool ModifierSupportsInterpolators(Modifier modifier) => modifier is AgeModifier || modifier is VelocityModifier;

//     public static void AddInterpolator(Type interpolatorType)
//     {
//         if (CurrentInterpolators == null)
//         {
//             return;
//         }

//         Interpolator interpolator = CreateInterpolator(interpolatorType);
//         int index = CurrentInterpolators.Count;
//         CurrentInterpolators.Add(interpolator);
//         TrackObject(interpolator);
//         SelectInterpolator(index);
//         HasUnsavedChanges = true;
//     }

//     public static void RemoveInterpolator(int index)
//     {
//         if (CurrentInterpolators == null || index < 0 || index >= CurrentInterpolators.Count)
//         {
//             return;
//         }

//         Interpolator interpolator = CurrentInterpolators[index];
//         CurrentInterpolators.RemoveAt(index);
//         UntrackObject(interpolator);

//         if (interpolator == SelectedInterpolator)
//         {
//             index = Math.Max(0, index - 1);
//             if (CurrentInterpolators.Count > 0)
//             {
//                 SelectInterpolator(index);
//             }
//             else
//             {
//                 SelectInterpolator(-1);
//             }
//         }

//         HasUnsavedChanges = true;
//     }

//     public static void SelectInterpolator(int index)
//     {
//         if (CurrentInterpolators == null || index < 0 || index >= CurrentInterpolators.Count)
//         {
//             SelectedInterpolator = null;
//             SelectedInterpolatorIndex = -1;
//         }
//         else
//         {
//             SelectedInterpolator = CurrentInterpolators[index];
//             SelectedInterpolatorIndex = index;
//         }
//     }

//     public static void ReorderInterpolator(int from, int to)
//     {
//         if (CurrentInterpolators == null || from < 0 || from >= CurrentInterpolators.Count || to < 0 || to >= CurrentInterpolators.Count)
//         {
//             return;
//         }

//         Interpolator moving = CurrentInterpolators[from];
//         CurrentInterpolators.RemoveAt(from);
//         CurrentInterpolators.Insert(to, moving);
//         SelectInterpolator(SelectedInterpolatorIndex);
//         HasUnsavedChanges = true;
//     }

//     private static Interpolator CreateInterpolator(Type interpolatorType)
//     {
//         if (interpolatorType == typeof(ColorInterpolator)) return new ColorInterpolator() { StartValue = new HslColor(0.0f, 0.0f, 0.0f), EndValue = new HslColor(0.0f, 0.0f, 1.0f) };
//         if (interpolatorType == typeof(HueInterpolator)) return new HueInterpolator() { StartValue = 0.0f, EndValue = 1.0f };
//         if (interpolatorType == typeof(OpacityInterpolator)) return new OpacityInterpolator() { StartValue = 0.0f, EndValue = 1.0f };
//         if (interpolatorType == typeof(RotationInterpolator)) return new RotationInterpolator() { StartValue = 0.0f, EndValue = MathF.PI / 2.0f };
//         if (interpolatorType == typeof(ScaleInterpolator)) return new ScaleInterpolator() { StartValue = Vector2.One, EndValue = Vector2.Zero };
//         if (interpolatorType == typeof(VelocityInterpolator)) return new VelocityInterpolator() { StartValue = Vector2.Zero, EndValue = Vector2.One };

//         throw new InvalidOperationException($"Unknown interpolator type '{interpolatorType.Name}'");
//     }

//     public static bool IsLocked(ParticleEmitter emitter) => s_emitterLocks.GetValueOrDefault(emitter, false);
//     public static bool IsLocked(Modifier modifier) => s_modifierLocks.GetValueOrDefault(modifier, false);
//     public static bool IsLocked(Interpolator interpolator) => s_interpolatorLocks.GetValueOrDefault(interpolator, false);

//     public static bool ToggleLock(ParticleEmitter emitter) => s_emitterLocks[emitter] = !IsLocked(emitter);
//     public static bool ToggleLock(Modifier modifier) => s_modifierLocks[modifier] = !IsLocked(modifier);
//     public static bool ToggleLock(Interpolator interpolator) => s_interpolatorLocks[interpolator] = !IsLocked(interpolator);

//     public static void TrackObject(ParticleEmitter emitter) => s_emitterLocks[emitter] = false;
//     public static void TrackObject(Modifier modifier) => s_modifierLocks[modifier] = false;
//     public static void TrackObject(Interpolator interpolator) => s_interpolatorLocks[interpolator] = false;

//     public static void UntrackObject(ParticleEmitter emitter) => s_emitterLocks.Remove(emitter);
//     public static void UntrackObject(Modifier modifier) => s_modifierLocks.Remove(modifier);
//     public static void UntrackObject(Interpolator interpolator) => s_interpolatorLocks.Remove(interpolator);

//     public static void AddTexture(string filePath)
//     {
//         ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

//         string fileName = Path.GetFileName(filePath);
//         string sourceDirectory = Path.GetDirectoryName(filePath);

//         // If the file is already in the project directory, just load it
//         if (string.Equals(sourceDirectory, ProjectDirectory, StringComparison.OrdinalIgnoreCase))
//         {
//             ProcessTextureFile(fileName);
//             return;
//         }

//         string destinationPath = Path.Combine(ProjectDirectory, fileName);

//         // Check if it would be overwritten
//         if (File.Exists(destinationPath))
//         {
//             OverwriteExistingFileModal.Open(fileName, (result) =>
//             {
//                 if (result.Status == ModalResult.Success)
//                 {
//                     CopyAndProcessTexture(filePath, destinationPath, fileName);
//                 }
//             });
//         }
//         else
//         {
//             CopyAndProcessTexture(filePath, destinationPath, fileName);
//         }
//     }

//     private static void CopyAndProcessTexture(string sourcePath, string destinationPath, string fileName)
//     {
//         try
//         {
//             File.Copy(sourcePath, destinationPath, overwrite: true);
//             ProcessTextureFile(fileName);
//         }
//         catch
//         {
//             throw;
//         }
//     }

//     private static void ProcessTextureFile(string fileName)
//     {
//         LoadTexture(fileName);
//         SetTexture(fileName);
//     }

//     public static void LoadTexture(string fileName)
//     {
//         // If it's already been loaded, remove it and unload it first.
//         if (s_textureCache.ContainsKey(fileName))
//         {
//             s_contentManager.UnloadAsset(fileName);
//             s_textureCache.Remove(fileName);
//         }

//         // Add it to the cache
//         Texture2D texture = s_contentManager.Load<Texture2D>(fileName);
//         texture.Name = fileName;
//         s_textureCache[fileName] = texture;

//         // Find any existing particle emitters that were using that texture
//         // and update it
//         foreach (ParticleEmitter emitter in ParticleEffect?.Emitters)
//         {
//             if (emitter.TextureRegion != null)
//             {
//                 if (emitter.TextureRegion.Name == fileName)
//                 {
//                     emitter.TextureRegion = new(texture, emitter.TextureRegion.Bounds);
//                 }
//             }
//         }
//     }

//     public static void SetTexture(string key)
//     {
//         if (SelectedParticleEmitter == null || !s_textureCache.ContainsKey(key))
//         {
//             return;
//         }

//         Texture2D texture = s_textureCache[key];
//         SelectedParticleEmitter.TextureRegion = new(texture);
//     }
// }
