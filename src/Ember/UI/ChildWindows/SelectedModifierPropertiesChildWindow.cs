// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Hexa.NET.ImGui;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended;

namespace Ember.UI.ChildWindows;

public static class SelectedModifierPropertiesChildWindow
{
    public static void Draw()
    {
        if (ImGui.CollapsingHeader(SR.CollapsingHeader_SelectedModifierProperties, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY;

            if (ImGui.BeginChild("##selected_modifier_properties_child_window"u8, SysVec2.Zero, childFlags))
            {
                DrawContent();
            }
            ImGui.EndChild();
        }
    }

    private static void DrawContent()
    {
        if (EmberContext.SelectedModifier is not Modifier modifier)
        {
            ImGui.TextDisabled(SR.Message_NoModifierSelected);
            return;
        }

        bool isLocked = EmberContext.IsLocked(EmberContext.SelectedParticleEmitter) || EmberContext.IsLocked(modifier);

        ImGui.BeginDisabled(isLocked);
        if (ImGui.BeginTable("##selected_modifier_properties_table"u8, 2, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("##selected_modifier_property_label_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##selected_modifier_property_value_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

            DrawSelectedModifierNamePropertyRow(modifier);
            DrawSelectedModifierFrequencyPropertyRow(modifier);

            switch (modifier)
            {
                case AgeModifier:
                    // No additional properties
                    break;

                case CircleContainerModifier circleContainer:
                    bool circleContainerInside = circleContainer.Inside;
                    if (DrawBoolProperty(SR.CircleContainerModifier_Property_Inside_Name, SR.CircleContainerModifier_Property_Inside_Description, ref circleContainerInside))
                    {
                        circleContainer.Inside = circleContainerInside;
                        EmberContext.HasUnsavedChanges = true;
                    }

                    float circleContainerRadius = circleContainer.Radius;
                    if (DrawFloatProperty(SR.CircleContainerModifier_Property_Radius_Name, SR.CircleContainerModifier_Property_Radius_Description, ref circleContainerRadius, 0.1f, 0.0f, float.MaxValue))
                    {
                        circleContainer.Radius = circleContainerRadius;
                        EmberContext.HasUnsavedChanges = true;
                    }

                    float circleContainerRestitutionCoefficient = circleContainer.RestitutionCoefficient;
                    if (DrawFloatProperty(SR.CircleContainerModifier_Property_RestitutionCoefficient_Name, SR.CircleContainerModifier_Property_RestitutionCoefficient_Description, ref circleContainerRestitutionCoefficient, 0.1f, 0.0f, float.MaxValue))
                    {
                        circleContainer.RestitutionCoefficient = circleContainerRestitutionCoefficient;
                        EmberContext.HasUnsavedChanges = true;
                    }
                    break;

                case DragModifier drag:
                    float dragDensity = drag.Density;
                    if (DrawFloatProperty(SR.DragModifier_Property_Density_Name, SR.DragModifier_Property_Density_Description, ref dragDensity, 0.1f, 0.0f, float.MaxValue))
                    {
                        drag.Density = dragDensity;
                        EmberContext.HasUnsavedChanges = true;
                    }

                    float dragDragCoefficient = drag.DragCoefficient;
                    if (DrawFloatProperty(SR.DragModifier_Property_DragCoefficient_Name, SR.DragModifier_Property_DragCoefficient_Description, ref dragDragCoefficient, 0.1f, 0.0f, float.MaxValue))
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
                    if (DrawFloatProperty(SR.LinearGravityModifier_Property_Strength_Name, SR.LinearGravityModifier_Property_Strength_Description, ref gravityStrength, 0.1f, 0.0f, float.MaxValue))
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
                    if (DrawFloatProperty(SR.RectangleContainerModifier_Property_RestitutionCoefficient_Name, SR.RectangleContainerModifier_Property_RestitutionCoefficient_Description, ref rectContainerRestitutionCoefficient, 0.1f, 0.0f, float.MaxValue))
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
                    if (DrawFloatProperty(SR.RotationModifier_Property_RotationRate_Name, SR.RotationModifier_Property_RotationRate_Description, ref rotationRotationRate, 0.1f, float.MinValue, float.MaxValue))
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
                    if (DrawFloatProperty(SR.VelocityColorModifier_Property_VelocityThreshold_Name, SR.VelocityColorModifier_Property_VelocityThreshold_Description, ref velocityColorVelocityThreshold, 0.1f, 0.0f, float.MaxValue))
                    {
                        velocityColor.VelocityThreshold = velocityColorVelocityThreshold;
                        EmberContext.HasUnsavedChanges = true;
                    }
                    break;

                case VelocityModifier velocity:
                    float velocityVelocityThreshold = velocity.VelocityThreshold;
                    if (DrawFloatProperty(SR.VelocityModifier_Property_VelocityThreshold_Name, SR.VelocityModifier_Property_VelocityThreshold_Description, ref velocityVelocityThreshold, 0.1f, 0.0f, float.MaxValue))
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

            ImGui.EndTable();
        }
        ImGui.EndDisabled();
    }

    private static void DrawSelectedModifierNamePropertyRow(Modifier modifier)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.Modifier_Property_Name_Name);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(SR.Modifier_Property_Name_Description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        string modifierName = modifier.Name;
        if (ImGui.InputText("##selected_modifier_name_value"u8, ref modifierName, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
        {
            modifier.Name = modifierName;
            EmberContext.HasUnsavedChanges = true;
        }
    }

    private static void DrawSelectedModifierFrequencyPropertyRow(Modifier modifier)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.Modifier_Property_Frequency_Name);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(SR.Modifier_Property_Frequency_Description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        float modifierFrequency = modifier.Frequency;
        if (ImGui.DragFloat(SR.Modifier_Property_Frequency_Description, ref modifierFrequency, 0.1f, 0.0f, float.MaxValue, "%.2f"))
        {
            modifier.Frequency = modifierFrequency;
            EmberContext.HasUnsavedChanges = true;
        }
    }

    private static bool DrawBoolProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref bool value)
    {
        bool valueChanged = false;

        ImGui.PushID(label);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);

        if (ImGui.Checkbox("##value"u8, ref value))
        {
            valueChanged = true;
        }

        ImGui.PopID();

        return valueChanged;
    }

    private static bool DrawIntProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref int value, int step, int min, int max)
    {
        bool valueChanged = false;

        ImGui.PushID(label);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);

        if (ImGui.DragInt("##value"u8, ref value, step, min, max))
        {
            valueChanged = true;
        }

        ImGui.PopID();

        return valueChanged;
    }

    private static bool DrawFloatProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref float value, float step, float min, float max)
    {
        bool valueChanged = false;

        ImGui.PushID(label);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);

        if (ImGui.DragFloat("##value"u8, ref value, step, min, max, "%.2f"u8))
        {
            valueChanged = true;
        }

        ImGui.PopID();

        return valueChanged;
    }

    private static bool DrawVector2Property(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref XnaVec2 value, float step, float min, float max)
    {
        bool valueChanged = false;

        ImGui.PushID(label);

        ImGuiStylePtr stylePtr = ImGui.GetStyle();

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(description);
        }

        ImGui.TableNextColumn();
        float availWidth = ImGui.GetContentRegionAvail().X;
        float itemSpacingWidth = stylePtr.ItemSpacing.X;
        float dragWidth = (availWidth - itemSpacingWidth) * 0.5f;
        ImGui.SetNextItemWidth(dragWidth);

        if (ImGui.DragFloat("##x"u8, ref value.X, step, min, max, "X: %.2F"u8))
        {
            valueChanged = true;
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(dragWidth);
        if (ImGui.DragFloat("##y"u8, ref value.Y, step, min, max, "Y: %.2f"u8))
        {
            valueChanged = true;
        }

        ImGui.PopID();

        return valueChanged;
    }

    private static bool DrawColorProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref HslColor value)
    {
        bool valueChanged = false;

        ImGui.PushID(label);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(description);
        }

        ImGui.TableNextColumn();

        XnaColor rgbColor = HslColor.ToRgb(value);
        SysVec4 color = new(rgbColor.R / 255.0f, rgbColor.G / 255.0f, rgbColor.B / 255.0f, 1.0f);

        float availWidth = ImGui.GetContentRegionAvail().X;
        SysVec2 buttonSize = new(availWidth, ImGui.GetFrameHeight());

        if (ImGui.ColorButton("##color_button"u8, color, ImGuiColorEditFlags.None, buttonSize))
        {
            ImGui.OpenPopup("##color_picker");
        }

        if (ImGui.BeginPopup("##color_picker"))
        {
            SysVec3 rgb = new(color.X, color.Y, color.Z);
            if (ImGui.ColorPicker3("##value", ref rgb))
            {
                XnaColor newColor = new(rgb.X, rgb.Y, rgb.Z);
                value = HslColor.FromRgb(newColor);
                valueChanged = true;
            }

            ImGui.EndPopup();
        }

        ImGui.PopID();

        return valueChanged;
    }
}
