using System;
using Ember.Architecture.Components;
using Hexa.NET.ImGui;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using static Hexa.NET.ImGui.ImGui;

namespace Ember.Architecture.Views;

public sealed class ModifiersView
{
    public const string ViewName = "Modifiers";

    private readonly EditorContext _context;
    private int _modifierDragFromIndex = -1;
    private int _modifierDragToIndex = -1;
    private Type _modifierTypeToAdd;
    private bool _chooseModifier;

    public ModifiersView(EditorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public void Draw()
    {
        if (Begin(ViewName))
        {
            DrawModifierList();
        }
        End();

        DrawChooseModifierPopup();
    }

    private unsafe void DrawModifierList()
    {
        if (CollapsingHeader("Modifiers"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            SysVec2 childWindowSize = new SysVec2(0.0f, 300.0f);

            ReadOnlySpan<byte> disabledMessage = [];

            if (_context.ParticleEffect.Emitters.Count == 0)
            {
                disabledMessage = "No particle emitters added"u8;
            }
            else if (_context.SelectedEmitter == null)
            {
                disabledMessage = "No particle emitter selected"u8;
            }

            if (_context.SelectedEmitter != null)
            {
                if (Button("Add Modifier"u8, -SysVec2.UnitX))
                {
                    _chooseModifier = true;
                }

                if (_context.SelectedEmitter.Modifiers.Count == 0)
                {
                    disabledMessage = "No modifiers added"u8;
                }
            }

            if (BeginChild("##modifier-list-child-window"u8, childWindowSize, childFlags))
            {
                if (disabledMessage != ReadOnlySpan<byte>.Empty)
                {
                    TextDisabled(disabledMessage);
                }
                else
                {
                    float iconColumnWidth = 20.0f;
                    ImGuiTableFlags tableFlags = ImGuiTableFlags.ScrollY
                                                 | ImGuiTableFlags.RowBg
                                                 | ImGuiTableFlags.SizingStretchProp;

                    if (BeginTable("##modifier-list"u8, columns: 4, tableFlags))
                    {
                        TableSetupColumn("##modifier-list-name-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                        TableSetupColumn("##modifier-list-lock-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
                        TableSetupColumn("##modifier-list-visibility-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
                        TableSetupColumn("##modifier-list-delete-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);

                        for (int i = 0; i < _context.SelectedEmitter.Modifiers.Count; i++)
                        {
                            TableNextRow();
                            PushID(i);

                            Modifier modifier = _context.SelectedEmitter.Modifiers[i];
                            bool isLocked = _context.IsLocked(modifier);
                            bool isSelected = modifier == _context.SelectedModifier;

                            // Name Column
                            TableNextColumn();
                            SysVec2 nameButtonSize = new SysVec2(-1, GetFrameHeight());
                            PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new SysVec2(0.0f, 0.5f));
                            if (Button(modifier.Name, nameButtonSize))
                            {
                                _context.SelectModifier(i);
                            }

                            if (BeginDragDropSource(ImGuiDragDropFlags.None))
                            {
                                int* indexPtr = &i;
                                SetDragDropPayload("modifier-reorder-payload"u8, &i, sizeof(int));
                                Text($"Moving: {modifier.Name}");
                                EndDragDropSource();
                            }

                            if (BeginDragDropTarget())
                            {
                                ImGuiPayloadPtr payloadPtr = AcceptDragDropPayload("modifier-reorder-payload"u8);
                                if (!payloadPtr.IsNull)
                                {
                                    _modifierDragFromIndex = *(int*)payloadPtr.Data;
                                    _modifierDragToIndex = i;
                                }

                                EndDragDropTarget();
                            }

                            PopStyleVar();

                            // Lock column
                            TableNextColumn();
                            Text(isLocked ? Fonts.LockIcon : Fonts.UnlockedIcon);
                            if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                            {
                                _context.ToggleLock(modifier);
                            }

                            // Visibility Column
                            TableNextColumn();
                            BeginDisabled(isLocked);
                            Text(modifier.Enabled ? Fonts.EnabledIcon : Fonts.DisabledIcon);
                            if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                            {
                                modifier.Enabled = !modifier.Enabled;
                                _context.HasUnsavedChanges = true;
                            }
                            EndDisabled();

                            // Delete column
                            TableNextColumn();
                            BeginDisabled(isLocked);
                            Text(Fonts.DeleteIcon);
                            if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                            {
                                _context.RemoveModifier(i);
                            }
                            EndDisabled();

                            // Reorder emitters if a drag/drop occured
                            if (_modifierDragFromIndex != -1 && _modifierDragToIndex != -1 && _modifierDragFromIndex != _modifierDragToIndex)
                            {
                                _context.ReorderModifiers(_modifierDragFromIndex, _modifierDragToIndex);
                                _modifierDragFromIndex = -1;
                                _modifierDragToIndex = -1;
                            }

                            PopID();
                        }
                        EndTable();
                    }
                }
            }
            EndChild();
        }
    }

    private void DrawSelectedModifierProperties()
    {
        if (CollapsingHeader("Modifier Properties"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            if (_context.SelectedModifier is not Modifier modifier)
            {
                return;
            }

            if (BeginChild("##selected-modifier-properties-child-window"u8, SysVec2.Zero, childFlags))
            {
                BeginDisabled(_context.IsLocked(modifier));
                if (PropertyTable.BeginPropertyTable("##selected-modifier-properties-table"u8))
                {
                    // Name property
                    string modifierName = modifier.Name;
                    if (PropertyTable.TextProperty("Name"u8, "The display name of the selected modifier"u8, ref modifierName))
                    {
                        modifier.Name = modifierName;
                    }

                    // Frequency property
                    float modifierFrequency = modifier.Frequency;
                    if (PropertyTable.FloatProperty("Frequency"u8, "How often, in times per second, the modifier attempts to update the particle buffer"u8, ref modifierFrequency, 0.1f, 0.0f, float.MaxValue))
                    {
                        modifier.Frequency = modifierFrequency;
                    }

                    switch (modifier)
                    {
                        case AgeModifier: /* No Additional Properties */ break;

                        case CircleContainerModifier circleContainer:
                            bool circleContainerInside = circleContainer.Inside;
                            if (PropertyTable.BoolProperty("Inside"u8, ""u8, ref circleContainerInside))
                            {
                                circleContainer.Inside = circleContainerInside;
                            }

                            float circleContainerRadius = circleContainer.Radius;
                            if (PropertyTable.FloatProperty("Radius"u8, "The radius of the circular container"u8, ref circleContainerRadius, 0.1f, 0.0f, float.MaxValue))
                            {
                                circleContainer.Radius = circleContainerRadius;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            float circleContainerRestitutionCoefficient = circleContainer.RestitutionCoefficient;
                            if (PropertyTable.FloatProperty("Restitution Coefficient"u8, "The coefficient of restitution (bounciness) for particle collisions with the boundary"u8, ref circleContainerRestitutionCoefficient, 0.1f, 0.0f, float.MaxValue))
                            {
                                circleContainer.RestitutionCoefficient = circleContainerRestitutionCoefficient;
                                EmberContext.HasUnsavedChanges = true;
                            }
                            break;

                        case DragModifier drag:
                            float dragDensity = drag.Density;
                            if (PropertyTable.FloatProperty("Density"u8, SR.DragModifier_Property_Density_Description, ref dragDensity, 0.1f, 0.0f, float.MaxValue))
                            {
                                drag.Density = dragDensity;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            float dragDragCoefficient = drag.DragCoefficient;
                            if (PropertyTable.FloatProperty("Drag Coefficient"u8, SR.DragModifier_Property_DragCoefficient_Description, ref dragDragCoefficient, 0.1f, 0.0f, float.MaxValue))
                            {
                                drag.DragCoefficient = dragDragCoefficient;
                                EmberContext.HasUnsavedChanges = true;
                            }
                            break;

                        case LinearGravityModifier gravity:
                            XnaVec2 gravityDirection = gravity.Direction;
                            if (DrawVector2Property(SR.LinearGravityModifier_Property_Direction_Name, SR.LinearGravityModifier_Property_Direction_Description, ref gravityDirection, 0.1f, float.MinValue, float.MaxValue))
                            {
                                gravity.Direction = gravityDirection;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            float gravityStrength = gravity.Strength;
                            if (PropertyTable.FloatProperty("Strength"u8, SR.LinearGravityModifier_Property_Strength_Description, ref gravityStrength, 0.1f, 0.0f, float.MaxValue))
                            {
                                gravity.Strength = gravityStrength;
                                EmberContext.HasUnsavedChanges = true;
                            }
                            break;

                        case OpacityFastFadeModifier:
                            // No additional properties
                            break;

                        case RectangleContainerModifier rectContainer:
                            int rectContainerWidth = rectContainer.Width;
                            if (DrawIntProperty(SR.RectangleContainerModifier_Property_Width_Name, SR.RectangleContainerModifier_Property_Width_Description, ref rectContainerWidth, 1, 0, int.MaxValue))
                            {
                                rectContainer.Width = rectContainerWidth;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            int rectContainerHeight = rectContainer.Height;
                            if (DrawIntProperty(SR.RectangleContainerModifier_Property_Height_Name, SR.RectangleContainerModifier_Property_Height_Description, ref rectContainerHeight, 1, 0, int.MaxValue))
                            {
                                rectContainer.Height = rectContainerHeight;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            float rectContainerRestitutionCoefficient = rectContainer.RestitutionCoefficient;
                            if (PropertyTable.FloatProperty("Restitution Coefficient"u8, SR.RectangleContainerModifier_Property_RestitutionCoefficient_Description, ref rectContainerRestitutionCoefficient, 0.1f, 0.0f, float.MaxValue))
                            {
                                rectContainer.RestitutionCoefficient = rectContainerRestitutionCoefficient;
                                EmberContext.HasUnsavedChanges = true;
                            }
                            break;

                        case RectangleLoopContainerModifier rectLoop:
                            int rectLoopWidth = rectLoop.Width;
                            if (DrawIntProperty(SR.RectangleLoopContainerModifier_Property_Width_Name, SR.RectangleLoopContainerModifier_Property_Width_Description, ref rectLoopWidth, 1, 0, int.MaxValue))
                            {
                                rectLoop.Width = rectLoopWidth;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            int rectLoopHeight = rectLoop.Height;
                            if (DrawIntProperty(SR.RectangleLoopContainerModifier_Property_Height_Name, SR.RectangleLoopContainerModifier_Property_Height_Description, ref rectLoopHeight, 1, 0, int.MaxValue))
                            {
                                rectLoop.Height = rectLoopHeight;
                                EmberContext.HasUnsavedChanges = true;
                            }
                            break;

                        case RotationModifier rotation:
                            float rotationRotationRate = rotation.RotationRate;
                            if (PropertyTable.FloatProperty("Rotation Rate"u8, SR.RotationModifier_Property_RotationRate_Description, ref rotationRotationRate, 0.1f, float.MinValue, float.MaxValue))
                            {
                                rotation.RotationRate = rotationRotationRate;
                                EmberContext.HasUnsavedChanges = true;
                            }
                            break;

                        case VelocityColorModifier velocityColor:
                            HslColor velocityColorStationaryColor = velocityColor.StationaryColor;
                            if (DrawColorProperty(SR.VelocityColorModifier_Property_StationaryColor_Name, SR.VelocityColorModifier_Property_StationaryColor_Description, ref velocityColorStationaryColor))
                            {
                                velocityColor.StationaryColor = velocityColorStationaryColor;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            HslColor velocityColorVelocityColor = velocityColor.VelocityColor;
                            if (DrawColorProperty(SR.VelocityColorModifier_Property_VelocityColor_Name, SR.VelocityColorModifier_Property_VelocityColor_Description, ref velocityColorVelocityColor))
                            {
                                velocityColor.VelocityColor = velocityColorVelocityColor;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            float velocityColorVelocityThreshold = velocityColor.VelocityThreshold;
                            if (PropertyTable.FloatProperty("Velocity Threshold"u8, SR.VelocityColorModifier_Property_VelocityThreshold_Description, ref velocityColorVelocityThreshold, 0.1f, 0.0f, float.MaxValue))
                            {
                                velocityColor.VelocityThreshold = velocityColorVelocityThreshold;
                                EmberContext.HasUnsavedChanges = true;
                            }
                            break;

                        case VelocityModifier velocity:
                            float velocityVelocityThreshold = velocity.VelocityThreshold;
                            if (PropertyTable.FloatProperty("Velocity Threshold"u8, SR.VelocityModifier_Property_VelocityThreshold_Description, ref velocityVelocityThreshold, 0.1f, 0.0f, float.MaxValue))
                            {
                                velocity.VelocityThreshold = velocityVelocityThreshold;
                                EmberContext.HasUnsavedChanges = true;
                            }
                            break;

                        case VortexModifier vortex:
                            XnaVec2 vortexPosition = vortex.Position;
                            if (DrawVector2Property(SR.VortexModifier_Property_Position_Name, SR.VortexModifier_Property_Position_Description, ref vortexPosition, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.Position = vortexPosition;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            float vortexStrength = vortex.Strength;
                            if (DrawFloatProperty(SR.VortexModifier_Property_Strength_Name, SR.VortexModifier_Property_Strength_Description, ref vortexStrength, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.Strength = vortexStrength;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            float vortexOuterRadius = vortex.OuterRadius;
                            if (DrawFloatProperty(SR.VortexModifier_Property_OuterRadius_Name, SR.VortexModifier_Property_OuterRadius_Description, ref vortexOuterRadius, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.OuterRadius = vortexOuterRadius;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            float vortexInnerRadius = vortex.InnerRadius;
                            if (DrawFloatProperty(SR.VortexModifier_Property_InnerRadius_Name, SR.VortexModifier_Property_InnerRadius_Description, ref vortexInnerRadius, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.InnerRadius = vortexInnerRadius;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            float vortexMaxVelocity = vortex.MaxVelocity;
                            if (DrawFloatProperty(SR.VortexModifier_Property_MaxVelocity_Name, SR.VortexModifier_Property_MaxVelocity_Description, ref vortexMaxVelocity, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.MaxVelocity = vortexMaxVelocity;
                                EmberContext.HasUnsavedChanges = true;
                            }

                            float vortexRotationAngle = vortex.RotationAngle;
                            if (DrawFloatProperty(SR.VortexModifier_Property_RotationAngle_Name, SR.VortexModifier_Property_RotationAngle_Description, ref vortexRotationAngle, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.RotationAngle = vortexRotationAngle;
                                EmberContext.HasUnsavedChanges = true;
                            }
                            break;
                    }

                    PropertyTable.EndPropertyTable();
                }
                EndDisabled();
            }
            EndChild();
        }
    }

    private void DrawChooseModifierPopup()
    {
        if (_chooseModifier)
        {
            OpenPopup("Choose Modifier"u8);
            _chooseModifier = false;
            _modifierTypeToAdd = null;
        }


        ImGuiViewportPtr viewportPtr = GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        SetNextWindowSizeConstraints(new SysVec2(400, 0), viewportPtr.WorkSize);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.AlwaysAutoResize
                                      | ImGuiWindowFlags.NoResize
                                      | ImGuiWindowFlags.NoCollapse
                                      | ImGuiWindowFlags.NoMove;

        if (BeginPopupModal("Choose Modifier"u8, modalFlags))
        {
            if (BeginChild("##choose-modifier-list"u8, ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY))
            {
                AddModifierChoice(typeof(AgeModifier), "Age Modifier"u8, "Applies interpolators to particles based on their age over their lifetime"u8);
                AddModifierChoice(typeof(CircleContainerModifier), "Circle Container Modifier"u8, "Constrains particles within or outside a circular boundary with bouncing effects"u8);
                AddModifierChoice(typeof(DragModifier), "Drag Modifier"u8, "Applies fluid resistance to particles, gradually reducing their velocity over time"u8);
                AddModifierChoice(typeof(LinearGravityModifier), "Linear Gravity Modifier"u8, "Applies a constant directional force to particles, simulating gravity or wind"u8);
                AddModifierChoice(typeof(OpacityFastFadeModifier), "Opacity Fast Fade Modifier"u8, "Rapidly decreases particle opacity based on their age, creating a linear fade-out effect"u8);
                AddModifierChoice(typeof(RectangleContainerModifier), "Rectangle Container Modifier"u8, "Constrains particles within a rectangular boundary with bouncing effects"u8);
                AddModifierChoice(typeof(RectangleLoopContainerModifier), "Rectangle Loop Container Modifier"u8, "Wraps particles to the opposite side when they exit a rectangular boundary"u8);
                AddModifierChoice(typeof(RotationModifier), "Rotation Modifier"u8, "Applies a constant rotational velocity to particles over time"u8);
                AddModifierChoice(typeof(VelocityColorModifier), "Velocity Color Modifier"u8, "Changes particle colors dynamically based on their movement speed"u8);
                AddModifierChoice(typeof(VelocityModifier), "Velocity Modifier"u8, "Applies interpolators to particles based on their velocity magnitude"u8);
                AddModifierChoice(typeof(VortexModifier), "Vortex Modifier"u8, "Creates a gravitational vortex effect, pulling particles toward a central point"u8);
            }
            EndChild();

            Separator();

            BeginDisabled(_modifierTypeToAdd == null);
            if (Button("Select"u8))
            {
                _context.AddModifier(_modifierTypeToAdd);
                _modifierTypeToAdd = null;
                CloseCurrentPopup();
            }
            EndDisabled();

            SameLine();
            if (Button("Cancel"u8))
            {
                _modifierTypeToAdd = null;
                CloseCurrentPopup();
            }

            EndPopup();
        }
    }

    private void AddModifierChoice(Type modifierType, ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip)
    {
        bool isSelected = _modifierTypeToAdd == modifierType;
        if (Selectable(label, isSelected))
        {
            _modifierTypeToAdd = modifierType;
        }
        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            SetTooltip(tooltip);
        }
        if (IsItemHovered() && IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            _context.AddModifier(modifierType);
            _modifierTypeToAdd = null;
            CloseCurrentPopup();
        }
    }

}
