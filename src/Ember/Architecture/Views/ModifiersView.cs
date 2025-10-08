// using System;
// using System.IO;
// using Ember.Architecture.PopupModals;
// using Ember.Architecture.Services;
// using Ember.Graphics;
// using Hexa.NET.ImGui;
// using MonoGame.Extended;
// using MonoGame.Extended.Graphics;
// using MonoGame.Extended.Particles;
// using MonoGame.Extended.Particles.Modifiers;
// using MonoGame.Extended.Particles.Modifiers.Containers;
// using MonoGame.Extended.Particles.Profiles;
// using static Hexa.NET.ImGui.ImGui;

// namespace Ember.Architecture.Views;

// public sealed class ModifiersView
// {
//     public const string ViewName = "Modifiers";

//     private readonly IProjectService _projectService;
//     private readonly IParticleEffectService _particleEffectService;
//     private readonly IModifierService _modifierService;
//     private readonly ISelectionService _selectionService;
//     private bool _chooseModifier;

//     public ModifiersView(IServiceProvider services)
//     {
//         ArgumentNullException.ThrowIfNull(services);

//         _modifierService = services.GetService(typeof(IModifierService)) as IModifierService;
//     }

//     public void Draw()
//     {
//         if (Begin(ViewName))
//         {
//             DrawModifierList();
//         }
//         End();
//     }

//     private unsafe void DrawModifierList()
//     {
//         if (CollapsingHeader("Modifiers"u8, ImGuiTreeNodeFlags.DefaultOpen))
//         {
//             ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
//                                          | ImGuiChildFlags.AutoResizeY;

//             SysVec2 childWindowSize = new SysVec2(0.0f, 300.0f);

//             if (Button("Add Modifier"u8, new SysVec2(-1, 0)))
//             {
//                 _chooseModifier = true;
//             }

//             // If there are no emitters, no emitters selected, or no modifiers
//             // display text stating so and return back
//             ReadOnlySpan<byte> disabledText = _particleEffectService.Current.Emitters.Count == 0 ? "No particle emitters added"u8
//                                               : _selectionService.SelectedEmitter == null ? "No particle emitter selected"u8
//                                               : _selectionService.SelectedEmitter.Modifiers.Count == 0 ? "No modifiers added"u8
//                                               : null;

//             if (disabledText != null)
//             {
//                 if (BeginChild("##modifier-list-child-window"u8, childWindowSize, childFlags))
//                 {
//                     TextDisabled("No modifiers emitters added"u8);
//                 }
//                 EndChild();
//                 return;
//             }

//             if (BeginChild("##modifier-list-child-window"u8, childWindowSize, childFlags))
//             {
//                 float iconColumnWidth = 20.0f;
//                 ImGuiTableFlags tableFlags = ImGuiTableFlags.ScrollY
//                                              | ImGuiTableFlags.RowBg
//                                              | ImGuiTableFlags.SizingStretchProp;

//                 if (BeginTable("##modifier-list"u8, columns: 4, tableFlags))
//                 {
//                     TableSetupColumn("##modifier-list-name-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
//                     TableSetupColumn("##modifier-list-lock-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
//                     TableSetupColumn("##modifier-list-visibility-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
//                     TableSetupColumn("##modifier-list-delete-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);

//                     for (int i = 0; i < _selectionService.SelectedEmitter.Modifiers.Count; i++)
//                     {
//                         TableNextRow();
//                         PushID(i);

//                         Modifier modifier = _modifierService.Select(i);
//                         ParticleEmitter emitter = _particleEffectService.Current.Emitters[i];
//                         bool isLocked = _lockService.IsLocked(emitter);
//                         bool isSelected = emitter == _selectionService.SelectedEmitter;

//                         // Name Column
//                         TableNextColumn();
//                         SysVec2 nameButtonSize = new SysVec2(-1, GetFrameHeight());
//                         PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new SysVec2(0.0f, 0.5f));
//                         if (Button(emitter.Name, nameButtonSize))
//                         {
//                             _selectionService.SelectEmitter(emitter, i);
//                         }

//                         if (BeginDragDropSource(ImGuiDragDropFlags.None))
//                         {
//                             int* indexPtr = &i;
//                             SetDragDropPayload("emitter-reorder-payload"u8, &i, sizeof(int));
//                             Text($"Moving: {emitter.Name}");
//                             EndDragDropSource();
//                         }

//                         if (BeginDragDropTarget())
//                         {
//                             ImGuiPayloadPtr payloadPtr = AcceptDragDropPayload("emitter-reorder-payload"u8);
//                             if (!payloadPtr.IsNull)
//                             {
//                                 _emitterDragFromIndex = *(int*)payloadPtr.Data;
//                                 _emitterDragToIndex = i;
//                             }

//                             EndDragDropTarget();
//                         }

//                         PopStyleVar();

//                         // Lock column
//                         TableNextColumn();
//                         Text(isLocked ? Fonts.LockIcon : Fonts.UnlockedIcon);
//                         if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
//                         {
//                             _lockService.ToggleLock(emitter);
//                         }

//                         // Visibility Column
//                         TableNextColumn();
//                         BeginDisabled(isLocked);
//                         Text(emitter.Visible ? Fonts.VisibleIcon : Fonts.NotVisibleIcon);
//                         if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
//                         {
//                             emitter.Visible = !emitter.Visible;
//                             _projectService.HasUnsavedChanges = true;
//                         }
//                         EndDisabled();

//                         // Delete column
//                         TableNextColumn();
//                         BeginDisabled(isLocked);
//                         Text(Fonts.DeleteIcon);
//                         if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
//                         {
//                             _emitterService.Remove(i);
//                         }
//                         EndDisabled();

//                         // Reorder emitters if a drag/drop occured
//                         if (_emitterDragFromIndex != -1 && _emitterDragToIndex != -1 && _emitterDragFromIndex != _emitterDragToIndex)
//                         {
//                             _emitterService.Reorder(_emitterDragFromIndex, _emitterDragToIndex);
//                             _emitterDragFromIndex = -1;
//                             _emitterDragToIndex = -1;
//                         }

//                         PopID();
//                     }
//                     EndTable();
//                 }
//             }
//             EndChild();
//         }
//     }

//     private Type _modifierTypeToAdd;

//     private void DrawChooseModifierPopup()
//     {
//         if (_chooseModifier)
//         {
//             OpenPopup("Choose Modifier"u8);
//             _chooseModifier = false;
//             _modifierTypeToAdd = null;
//         }


//         ImGuiViewportPtr viewportPtr = GetMainViewport();
//         SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

//         SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
//         SetNextWindowSizeConstraints(new SysVec2(400, 0), viewportPtr.WorkSize);

//         ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
//                                       | ImGuiWindowFlags.AlwaysAutoResize
//                                       | ImGuiWindowFlags.NoResize
//                                       | ImGuiWindowFlags.NoCollapse
//                                       | ImGuiWindowFlags.NoMove;

//         if (BeginPopupModal("Choose Modifier"u8, modalFlags))
//         {
//             if (BeginChild("##choose-modifier-list"u8, ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY))
//             {
//                 AddModifierChoice(typeof(AgeModifier), "Age Modifier"u8, "Applies interpolators to particles based on their age over their lifetime"u8);
//                 AddModifierChoice(typeof(CircleContainerModifier), "Circle Container Modifier"u8, "Constrains particles within or outside a circular boundary with bouncing effects"u8);
//                 AddModifierChoice(typeof(DragModifier), "Drag Modifier"u8, "Applies fluid resistance to particles, gradually reducing their velocity over time"u8);
//                 AddModifierChoice(typeof(LinearGravityModifier), "Linear Gravity Modifier"u8, "Applies a constant directional force to particles, simulating gravity or wind"u8);
//                 AddModifierChoice(typeof(OpacityFastFadeModifier), "Opacity Fast Fade Modifier"u8, "Rapidly decreases particle opacity based on their age, creating a linear fade-out effect"u8);
//                 AddModifierChoice(typeof(RectangleContainerModifier), "Rectangle Container Modifier"u8, "Constrains particles within a rectangular boundary with bouncing effects"u8);
//                 AddModifierChoice(typeof(RectangleLoopContainerModifier), "Rectangle Loop Container Modifier"u8, "Wraps particles to the opposite side when they exit a rectangular boundary"u8);
//                 AddModifierChoice(typeof(RotationModifier), "Rotation Modifier"u8, ""u8);
//                 AddModifierChoice(typeof(VelocityColorModifier), "Velocity Color Modifier"u8, "Changes particle colors dynamically based on their movement speed"u8);
//                 AddModifierChoice(typeof(VelocityModifier), "Velocity Modifier"u8, "Applies interpolators to particles based on their velocity magnitude"u8);
//                 AddModifierChoice(typeof(VortexModifier), "Vortex Modifier"u8, "Creates a gravitational vortex effect, pulling particles toward a central point"u8);
//             }
//             EndChild();

//             Separator();

//             BeginDisabled(_modifierTypeToAdd == null);
//             if (Button("Select"u8))
//             {
//                 _modifierService.Add(_modifierTypeToAdd);
//                 _modifierTypeToAdd = null;
//                 CloseCurrentPopup();
//             }
//             EndDisabled();

//             SameLine();
//             if (Button("Cancel"u8))
//             {
//                 _modifierTypeToAdd = null;
//                 CloseCurrentPopup();
//             }

//             EndPopup();
//         }
//     }

//     private void AddModifierChoice(Type modifierType, ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip)
//     {
//         bool isSelected = _modifierTypeToAdd == modifierType;
//         if (Selectable(label, isSelected))
//         {
//             _modifierTypeToAdd = modifierType;
//         }
//         if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
//         {
//             SetTooltip(tooltip);
//         }
//         if (IsItemHovered() && IsMouseDoubleClicked(ImGuiMouseButton.Left))
//         {
//             _modifierService.Add(modifierType);
//             _modifierTypeToAdd = null;
//             CloseCurrentPopup();
//         }
//     }

// }
