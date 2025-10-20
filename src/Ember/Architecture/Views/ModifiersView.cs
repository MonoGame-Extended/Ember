using System;
using System.Collections.Generic;
using Ember.Architecture.Components;
using Hexa.NET.ImGui;
using MonoGame.Extended;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
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
    private bool _chooseInterpolator;
    private int _interpolatorDragFromIndex = -1;
    private int _interpolatorDragToIndex = -1;
    private Type _interpolatorToAdd;

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
            DrawSelectedModifierProperties();
            DrawInterpolatorList();
            DrawSelectedInterpolatorProperties();
        }
        End();

        DrawChooseModifierPopup();
        DrawChooseInterpolatorPopup();
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
                            uint buttonColor = isSelected ? GetColorU32(ImGuiCol.Button) : GetColorU32(SysVec4.Zero);
                            uint buttonHoverColor = GetColorU32(ImGuiCol.ButtonHovered);

                            // Name Column
                            TableNextColumn();
                            SysVec2 nameButtonSize = new SysVec2(-1, GetFrameHeight());
                            PushStyleColor(ImGuiCol.Button, buttonColor);
                            PushStyleColor(ImGuiCol.ButtonHovered, buttonHoverColor);
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

                            PopStyleColor(2);
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

                            // Reorder emitters if a drag/drop occurred
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
                    if (PropertyTable.InputTextProperty("Name"u8, "The display name of the selected modifier"u8, ref modifierName))
                    {
                        modifier.Name = modifierName;
                        _context.HasUnsavedChanges = true;
                    }

                    // Frequency property
                    float modifierFrequency = modifier.Frequency;
                    if (PropertyTable.DragFloatProperty("Frequency"u8, "How often, in times per second, the modifier attempts to update the particle buffer"u8, ref modifierFrequency, 0.1f, 0.0f, float.MaxValue))
                    {
                        modifier.Frequency = modifierFrequency;
                        _context.HasUnsavedChanges = true;
                    }

                    switch (modifier)
                    {
                        case AgeModifier: /* No Additional Properties */ break;

                        case CircleContainerModifier circleContainer:
                            bool circleContainerInside = circleContainer.Inside;
                            if (PropertyTable.CheckboxProperty("Inside"u8, ""u8, ref circleContainerInside))
                            {
                                circleContainer.Inside = circleContainerInside;
                                _context.HasUnsavedChanges = true;
                            }

                            float circleContainerRadius = circleContainer.Radius;
                            if (PropertyTable.DragFloatProperty("Radius"u8, "The radius of the circular container"u8, ref circleContainerRadius, 0.1f, 0.0f, float.MaxValue))
                            {
                                circleContainer.Radius = circleContainerRadius;
                                _context.HasUnsavedChanges = true;
                            }

                            float circleContainerRestitutionCoefficient = circleContainer.RestitutionCoefficient;
                            if (PropertyTable.DragFloatProperty("Restitution Coefficient"u8, "The coefficient of restitution (bounciness) for particle collisions with the boundary"u8, ref circleContainerRestitutionCoefficient, 0.1f, 0.0f, float.MaxValue))
                            {
                                circleContainer.RestitutionCoefficient = circleContainerRestitutionCoefficient;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case DragModifier drag:
                            float dragDensity = drag.Density;
                            if (PropertyTable.DragFloatProperty("Density"u8, "The density of the fluid medium, affecting the strength of the drag force"u8, ref dragDensity, 0.1f, 0.0f, float.MaxValue))
                            {
                                drag.Density = dragDensity;
                                _context.HasUnsavedChanges = true;
                            }

                            float dragDragCoefficient = drag.DragCoefficient;
                            if (PropertyTable.DragFloatProperty("Drag Coefficient"u8, "The drag coefficient, representing the aerodynamic or hydrodynamic properties of particles"u8, ref dragDragCoefficient, 0.1f, 0.0f, float.MaxValue))
                            {
                                drag.DragCoefficient = dragDragCoefficient;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case LinearGravityModifier gravity:
                            XnaVec2 gravityDirection = gravity.Direction;
                            if (PropertyTable.DragVector2Property("Direction"u8, "The direction vector of the gravitational force"u8, ref gravityDirection, 0.1f, float.MinValue, float.MaxValue))
                            {
                                gravity.Direction = gravityDirection;
                                _context.HasUnsavedChanges = true;
                            }

                            float gravityStrength = gravity.Strength;
                            if (PropertyTable.DragFloatProperty("Strength"u8, "The strength of the gravitational force, in units per second squared"u8, ref gravityStrength, 0.1f, 0.0f, float.MaxValue))
                            {
                                gravity.Strength = gravityStrength;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case OpacityFastFadeModifier:
                            // No additional properties
                            break;

                        case RectangleContainerModifier rectContainer:
                            int rectContainerWidth = rectContainer.Width;
                            if (PropertyTable.DragIntProperty("Width"u8, "The width of the rectangular container"u8, ref rectContainerWidth, 1, 0, int.MaxValue))
                            {
                                rectContainer.Width = rectContainerWidth;
                                _context.HasUnsavedChanges = true;
                            }

                            int rectContainerHeight = rectContainer.Height;
                            if (PropertyTable.DragIntProperty("Height"u8, "The height of the rectangular container"u8, ref rectContainerHeight, 1, 0, int.MaxValue))
                            {
                                rectContainer.Height = rectContainerHeight;
                                _context.HasUnsavedChanges = true;
                            }

                            float rectContainerRestitutionCoefficient = rectContainer.RestitutionCoefficient;
                            if (PropertyTable.DragFloatProperty("Restitution Coefficient"u8, "The coefficient of restitution (bounciness) for particle collisions with the boundary"u8, ref rectContainerRestitutionCoefficient, 0.1f, 0.0f, float.MaxValue))
                            {
                                rectContainer.RestitutionCoefficient = rectContainerRestitutionCoefficient;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case RectangleLoopContainerModifier rectLoop:
                            int rectLoopWidth = rectLoop.Width;
                            if (PropertyTable.DragIntProperty("Width"u8, "The width of the rectangular container"u8, ref rectLoopWidth, 1, 0, int.MaxValue))
                            {
                                rectLoop.Width = rectLoopWidth;
                                _context.HasUnsavedChanges = true;
                            }

                            int rectLoopHeight = rectLoop.Height;
                            if (PropertyTable.DragIntProperty("Height"u8, "The height of the rectangular container"u8, ref rectLoopHeight, 1, 0, int.MaxValue))
                            {
                                rectLoop.Height = rectLoopHeight;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case RotationModifier rotation:
                            float rotationRotationRate = rotation.RotationRate;
                            if (PropertyTable.DragFloatProperty("Rotation Rate"u8, "The rate at which particles rotate, in radians per second"u8, ref rotationRotationRate, 0.1f, float.MinValue, float.MaxValue))
                            {
                                rotation.RotationRate = rotationRotationRate;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case VelocityColorModifier velocityColor:
                            HslColor velocityColorStationaryColor = velocityColor.StationaryColor;
                            if (PropertyTable.Color3Property("Stationary Color"u8, "The color for particles that are stationary or moving slowly"u8, ref velocityColorStationaryColor))
                            {
                                velocityColor.StationaryColor = velocityColorStationaryColor;
                                _context.HasUnsavedChanges = true;
                            }

                            HslColor velocityColorVelocityColor = velocityColor.VelocityColor;
                            if (PropertyTable.Color3Property("Velocity Color"u8, "The color for particles that have reached or exceeded the velocity threshold"u8, ref velocityColorVelocityColor))
                            {
                                velocityColor.VelocityColor = velocityColorVelocityColor;
                                _context.HasUnsavedChanges = true;
                            }

                            float velocityColorVelocityThreshold = velocityColor.VelocityThreshold;
                            if (PropertyTable.DragFloatProperty("Velocity Threshold"u8, "The velocity magnitude at which particles reach the maximum interpolation effect"u8, ref velocityColorVelocityThreshold, 0.1f, 0.0f, float.MaxValue))
                            {
                                velocityColor.VelocityThreshold = velocityColorVelocityThreshold;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case VelocityModifier velocity:
                            float velocityVelocityThreshold = velocity.VelocityThreshold;
                            if (PropertyTable.DragFloatProperty("Velocity Threshold"u8, "The velocity magnitude at which particles reach the maximum interpolation effect"u8, ref velocityVelocityThreshold, 0.1f, 0.0f, float.MaxValue))
                            {
                                velocity.VelocityThreshold = velocityVelocityThreshold;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case VortexModifier vortex:
                            XnaVec2 vortexPosition = vortex.Position;
                            if (PropertyTable.DragVector2Property("Position"u8, "The vortex center relative to the particle emitter"u8, ref vortexPosition, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.Position = vortexPosition;
                                _context.HasUnsavedChanges = true;
                            }

                            float vortexStrength = vortex.Strength;
                            if (PropertyTable.DragFloatProperty("Strength"u8, "The force strength applied to particles at the outer radius"u8, ref vortexStrength, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.Strength = vortexStrength;
                                _context.HasUnsavedChanges = true;
                            }

                            float vortexOuterRadius = vortex.OuterRadius;
                            if (PropertyTable.DragFloatProperty("Outer Radius"u8, "The maximum distance from the vortex center where forces are applied"u8, ref vortexOuterRadius, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.OuterRadius = vortexOuterRadius;
                                _context.HasUnsavedChanges = true;
                            }

                            float vortexInnerRadius = vortex.InnerRadius;
                            if (PropertyTable.DragFloatProperty("Inner Radius"u8, "The minimum distance from the vortex center where forces are applied"u8, ref vortexInnerRadius, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.InnerRadius = vortexInnerRadius;
                                _context.HasUnsavedChanges = true;
                            }

                            float vortexMaxVelocity = vortex.MaxVelocity;
                            if (PropertyTable.DragFloatProperty("Max Velocity"u8, "The maximum velocity magnitude that particles can reach under vortex influence"u8, ref vortexMaxVelocity, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.MaxVelocity = vortexMaxVelocity;
                                _context.HasUnsavedChanges = true;
                            }

                            float vortexRotationAngle = vortex.RotationAngle;
                            if (PropertyTable.DragFloatProperty("Rotation Angle"u8, "The rotation angle, in radians, applied to gravitational force vectors"u8, ref vortexRotationAngle, 0.1f, float.MinValue, float.MaxValue))
                            {
                                vortex.RotationAngle = vortexRotationAngle;
                                _context.HasUnsavedChanges = true;
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

    private unsafe void DrawInterpolatorList()
    {
        if (!_context.SupportsInterpolators(_context.SelectedModifier))
        {
            return;
        }

        List<Interpolator> interpolators = _context.GetCurrentInterpolators();

        if (CollapsingHeader("Interpolators"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            SysVec2 childWindowSize = new SysVec2(0.0f, 300.0f);

            if (Button("Add Interpolator"u8, -SysVec2.UnitX))
            {
                _chooseInterpolator = true;
            }

            if (BeginChild("##interpolator-list-child-window"u8, childWindowSize, childFlags))
            {
                float iconColumnWidth = 20.0f;
                ImGuiTableFlags tableFlags = ImGuiTableFlags.ScrollY
                                             | ImGuiTableFlags.RowBg
                                             | ImGuiTableFlags.SizingStretchProp;

                if (BeginTable("##interpolator-table"u8, columns: 4, tableFlags))
                {
                    TableSetupColumn("##interpolator-list-name-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                    TableSetupColumn("##interpolator-list-lock-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
                    TableSetupColumn("##interpolator-list-visibility-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
                    TableSetupColumn("##interpolator-list-delete-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);

                    for (int i = 0; i < interpolators.Count; i++)
                    {
                        TableNextRow();
                        PushID(i);

                        Interpolator interpolator = interpolators[i];
                        bool isLocked = _context.IsLocked(interpolator);
                        bool isSelected = interpolator == _context.SelectedInterpolator;
                        uint buttonColor = isSelected ? GetColorU32(ImGuiCol.Button) : GetColorU32(SysVec4.Zero);
                        uint buttonHoverColor = GetColorU32(ImGuiCol.ButtonHovered);

                        // Name Column
                        TableNextColumn();
                        SysVec2 nameButtonSize = new SysVec2(-1, GetFrameHeight());
                        PushStyleColor(ImGuiCol.Button, buttonColor);
                        PushStyleColor(ImGuiCol.ButtonHovered, buttonHoverColor);
                        PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new SysVec2(0.0f, 0.5f));
                        if (Button(interpolator.Name, nameButtonSize))
                        {
                            _context.SelectInterpolator(i);
                        }

                        if (BeginDragDropSource(ImGuiDragDropFlags.None))
                        {
                            int* indexPtr = &i;
                            SetDragDropPayload("interpolator-reorder-payload"u8, &i, sizeof(int));
                            Text($"Moving: {interpolator.Name}");
                            EndDragDropSource();
                        }

                        if (BeginDragDropTarget())
                        {
                            ImGuiPayloadPtr payloadPtr = AcceptDragDropPayload("interpolator-reorder-payload"u8);
                            if (!payloadPtr.IsNull)
                            {
                                _interpolatorDragFromIndex = *(int*)payloadPtr.Data;
                                _interpolatorDragToIndex = i;
                            }

                            EndDragDropTarget();
                        }

                        PopStyleColor(2);
                        PopStyleVar();

                        // Lock column
                        TableNextColumn();
                        Text(isLocked ? Fonts.LockIcon : Fonts.UnlockedIcon);
                        if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                        {
                            _context.ToggleLock(interpolator);
                        }

                        // Visibility Column
                        TableNextColumn();
                        BeginDisabled(isLocked);
                        Text(interpolator.Enabled ? Fonts.EnabledIcon : Fonts.DisabledIcon);
                        if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                        {
                            interpolator.Enabled = !interpolator.Enabled;
                            _context.HasUnsavedChanges = true;
                        }
                        EndDisabled();

                        // Delete column
                        TableNextColumn();
                        BeginDisabled(isLocked);
                        Text(Fonts.DeleteIcon);
                        if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                        {
                            _context.RemoveInterpolator(i);
                        }
                        EndDisabled();

                        // Reorder emitters if a drag/drop occurred
                        if (_interpolatorDragFromIndex != -1 && _interpolatorDragToIndex != -1 && _interpolatorDragFromIndex != _interpolatorDragToIndex)
                        {
                            _context.ReorderInterpolators(_interpolatorDragFromIndex, _interpolatorDragToIndex);
                            _interpolatorDragFromIndex = -1;
                            _interpolatorDragToIndex = -1;
                        }

                        PopID();
                    }
                    EndTable();
                }
            }
            EndChild();
        }


    }

    private void DrawSelectedInterpolatorProperties()
    {
        if (_context.SelectedInterpolator is not Interpolator interpolator)
        {
            return;
        }

        if (CollapsingHeader("Interpolator Properties"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            if (BeginChild("##selected-interpolator-properties-child-window"u8, SysVec2.Zero, childFlags))
            {
                BeginDisabled(_context.IsLocked(interpolator));
                if (PropertyTable.BeginPropertyTable("##selected-interpolator-properties-table"u8))
                {
                    // Name property
                    string interpolatorName = interpolator.Name;
                    if (PropertyTable.InputTextProperty("Name"u8, "The display name of the selected interpolator"u8, ref interpolatorName))
                    {
                        interpolator.Name = interpolatorName;
                        _context.HasUnsavedChanges = true;
                    }

                    switch (interpolator)
                    {
                        case ColorInterpolator colorInterpolator:
                            HslColor colorInterpolatorStartValue = colorInterpolator.StartValue;
                            if (PropertyTable.Color3Property("Start Value"u8, "Initial HSL color for particles"u8, ref colorInterpolatorStartValue))
                            {
                                colorInterpolator.StartValue = colorInterpolatorStartValue;
                                _context.HasUnsavedChanges = true;
                            }

                            HslColor colorInterpolatorEndValue = colorInterpolator.EndValue;
                            if (PropertyTable.Color3Property("End Value"u8, "Final HSL color for particles"u8, ref colorInterpolatorEndValue))
                            {
                                colorInterpolator.EndValue = colorInterpolatorEndValue;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case HueInterpolator hueInterpolator:
                            float hueInterpolatorStartValue = hueInterpolator.StartValue;
                            if (PropertyTable.DragFloatProperty("Start Value"u8, "Initial hue value (0.0 = red, 0.33 = green, 0.66 = blue, 1.0 = red)"u8, ref hueInterpolatorStartValue, 0.01f, 0.0f, 1.0f))
                            {
                                hueInterpolator.StartValue = hueInterpolatorStartValue;
                                _context.HasUnsavedChanges = true;
                            }

                            float hueInterpolatorEndValue = hueInterpolator.EndValue;
                            if (PropertyTable.DragFloatProperty("End Value"u8, "Final hue value (0.0 = red, 0.33 = green, 0.66 = blue, 1.0 = red)"u8, ref hueInterpolatorEndValue, 0.01f, 0.0f, 1.0f))
                            {
                                hueInterpolator.EndValue = hueInterpolatorEndValue;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case OpacityInterpolator opacityInterpolator:
                            float opacityInterpolatorStartValue = opacityInterpolator.StartValue;
                            if (PropertyTable.DragFloatProperty("Start Value"u8, "Initial opacity (0.0 = transparent, 1.0 = opaque)"u8, ref opacityInterpolatorStartValue, 0.01f, 0.0f, 1.0f))
                            {
                                opacityInterpolator.StartValue = opacityInterpolatorStartValue;
                                _context.HasUnsavedChanges = true;
                            }

                            float opacityInterpolatorEndValue = opacityInterpolator.EndValue;
                            if (PropertyTable.DragFloatProperty("End Value"u8, "Final opacity (0.0 = transparent, 1.0 = opaque)"u8, ref opacityInterpolatorEndValue, 0.01f, 0.0f, 1.0f))
                            {
                                opacityInterpolator.EndValue = opacityInterpolatorEndValue;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case RotationInterpolator rotationInterpolator:
                            float rotationInterpolatorStartValue = rotationInterpolator.StartValue;
                            if (PropertyTable.DragFloatProperty("Start Value"u8, "Initial rotation angle in radians (π = 180°, 2π = 360°)"u8, ref rotationInterpolatorStartValue, 0.01f, -MathF.PI * 2.0f, MathF.PI * 2.0f))
                            {
                                rotationInterpolator.StartValue = rotationInterpolatorStartValue;
                                _context.HasUnsavedChanges = true;
                            }

                            float rotationInterpolatorEndValue = rotationInterpolator.EndValue;
                            if (PropertyTable.DragFloatProperty("End Value"u8, "Final rotation angle in radians (π = 180°, 2π = 360°)"u8, ref rotationInterpolatorEndValue, 0.01f, -MathF.PI * 2.0f, MathF.PI * 2.0f))
                            {
                                rotationInterpolator.EndValue = rotationInterpolatorEndValue;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case ScaleInterpolator scaleInterpolator:
                            XnaVec2 scaleInterpolatorStartValue = scaleInterpolator.StartValue;
                            if (PropertyTable.DragVector2Property("Start Value"u8, "Initial particle size multiplier (1.0 = original size)"u8, ref scaleInterpolatorStartValue, 0.01f, 0.0f, 10.0f))
                            {
                                scaleInterpolator.StartValue = scaleInterpolatorStartValue;
                                _context.HasUnsavedChanges = true;
                            }

                            XnaVec2 scaleInterpolatorEndValue = scaleInterpolator.EndValue;
                            if (PropertyTable.DragVector2Property("End Value"u8, "Final particle size multiplier (1.0 = original size)"u8, ref scaleInterpolatorEndValue, 0.01f, 0.0f, 10.0f))
                            {
                                scaleInterpolator.EndValue = scaleInterpolatorEndValue;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case VelocityInterpolator velocityInterpolator:
                            XnaVec2 velocityInterpolatorStartValue = velocityInterpolator.StartValue;
                            if (PropertyTable.DragVector2Property("Start Value"u8, "Initial velocity vector in units per second (X, Y)"u8, ref velocityInterpolatorStartValue, 1.0f, -1000.0f, 1000.0f))
                            {
                                velocityInterpolator.StartValue = velocityInterpolatorStartValue;
                                _context.HasUnsavedChanges = true;
                            }

                            XnaVec2 velocityInterpolatorEndValue = velocityInterpolator.EndValue;
                            if (PropertyTable.DragVector2Property("End Value"u8, "Final velocity vector in units per second (X, Y)"u8, ref velocityInterpolatorEndValue, 1.0f, -1000.0f, 1000.0f))
                            {
                                velocityInterpolator.EndValue = velocityInterpolatorEndValue;
                                _context.HasUnsavedChanges = true;
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

    private void DrawChooseInterpolatorPopup()
    {
        if (_chooseInterpolator)
        {
            OpenPopup("Choose Interpolator"u8);
            _chooseInterpolator = false;
            _interpolatorToAdd = null;
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

        if (BeginPopupModal("Choose Interpolator"u8, modalFlags))
        {
            if (BeginChild("##choose-interpolator-list"u8, ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY))
            {
                AddInterpolatorChoice(typeof(ColorInterpolator), "Color Interpolator"u8, "Gradually changes all color components (hue, saturation, lightness) of particles over their lifetime"u8);
                AddInterpolatorChoice(typeof(HueInterpolator), "Hue Interpolator"u8, "Changes only the hue component of particle colors while preserving saturation and lightness"u8);
                AddInterpolatorChoice(typeof(OpacityInterpolator), "Opacity Interpolator"u8, "Gradually changes particle transparency from completely transparent to opaque"u8);
                AddInterpolatorChoice(typeof(RotationInterpolator), "Rotation Interpolator"u8, "Gradually changes the rotation angle of particles over their lifetime"u8);
                AddInterpolatorChoice(typeof(ScaleInterpolator), "Scale Interpolator"u8, "Gradually changes the size of particles by scaling their width and height"u8);
                AddInterpolatorChoice(typeof(VelocityInterpolator), "Velocity Interpolator"u8, "Gradually changes the velocity vector of particles, affecting their movement direction and speed"u8);
            }
            EndChild();

            Separator();

            BeginDisabled(_modifierTypeToAdd == null);
            if (Button("Select"u8))
            {
                _context.AddInterpolator(_interpolatorToAdd);
                _interpolatorToAdd = null;
                CloseCurrentPopup();
            }
            EndDisabled();

            SameLine();
            if (Button("Cancel"u8))
            {
                _interpolatorToAdd = null;
                CloseCurrentPopup();
            }

            EndPopup();
        }
    }

    private void AddInterpolatorChoice(Type interpolatorType, ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip)
    {
        bool isSelected = _modifierTypeToAdd == interpolatorType;
        if (Selectable(label, isSelected))
        {
            _interpolatorToAdd = interpolatorType;
        }
        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            SetTooltip(tooltip);
        }
        if (IsItemHovered() && IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            _context.AddInterpolator(interpolatorType);
            _interpolatorToAdd = null;
            CloseCurrentPopup();
        }
    }
}
