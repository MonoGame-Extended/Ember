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
                    DrawBoolProperty(SR.CircleContainerModifier_Property_Inside_Name, SR.CircleContainerModifier_Property_Inside_Description, ref circleContainer.Inside);
                    DrawFloatProperty(SR.CircleContainerModifier_Property_Radius_Name, SR.CircleContainerModifier_Property_Radius_Description, ref circleContainer.Radius, 0.1f, 0.0f, float.MaxValue);
                    DrawFloatProperty(SR.CircleContainerModifier_Property_RestitutionCoefficient_Name, SR.CircleContainerModifier_Property_RestitutionCoefficient_Description, ref circleContainer.RestitutionCoefficient, 0.1f, 0.0f, float.MaxValue);
                    break;

                case DragModifier drag:
                    DrawFloatProperty(SR.DragModifier_Property_Density_Name, SR.DragModifier_Property_Density_Description, ref drag.Density, 0.1f, 0.0f, float.MaxValue);
                    DrawFloatProperty(SR.DragModifier_Property_DragCoefficient_Name, SR.DragModifier_Property_DragCoefficient_Description, ref drag.DragCoefficient, 0.1f, 0.0f, float.MaxValue);
                    break;

                case LinearGravityModifier gravity:
                    DrawVector2Property(SR.LinearGravityModifier_Property_Direction_Name, SR.LinearGravityModifier_Property_Direction_Description, ref gravity.Direction, 0.1f, float.MinValue, float.MaxValue);
                    DrawFloatProperty(SR.LinearGravityModifier_Property_Strength_Name, SR.LinearGravityModifier_Property_Strength_Description, ref gravity.Strength, 0.1f, 0.0f, float.MaxValue);
                    break;

                case OpacityFastFadeModifier:
                    // No additional properties
                    break;

                case RectangleContainerModifier rectContainer:
                    DrawIntProperty(SR.RectangleContainerModifier_Property_Width_Name, SR.RectangleContainerModifier_Property_Width_Description, ref rectContainer.Width, 1, 0, int.MaxValue);
                    DrawIntProperty(SR.RectangleContainerModifier_Property_Height_Name, SR.RectangleContainerModifier_Property_Height_Description, ref rectContainer.Height, 1, 0, int.MaxValue);
                    DrawFloatProperty(SR.RectangleContainerModifier_Property_RestitutionCoefficient_Name, SR.RectangleContainerModifier_Property_RestitutionCoefficient_Description, ref rectContainer.RestitutionCoefficient, 0.1f, 0.0f, float.MaxValue);
                    break;

                case RectangleLoopContainerModifier rectLoop:
                    DrawIntProperty(SR.RectangleLoopContainerModifier_Property_Width_Name, SR.RectangleLoopContainerModifier_Property_Width_Description, ref rectLoop.Width, 1, 0, int.MaxValue);
                    DrawIntProperty(SR.RectangleLoopContainerModifier_Property_Height_Name, SR.RectangleLoopContainerModifier_Property_Height_Description, ref rectLoop.Height, 1, 0, int.MaxValue);
                    break;

                case RotationModifier rotation:
                    DrawFloatProperty(SR.RotationModifier_Property_RotationRate_Name, SR.RotationModifier_Property_RotationRate_Description, ref rotation.RotationRate, 0.1f, float.MinValue, float.MaxValue);
                    break;

                case VelocityColorModifier velocityColor:
                    DrawColorProperty(SR.VelocityColorModifier_Property_StationaryColor_Name, SR.VelocityColorModifier_Property_StationaryColor_Description, ref velocityColor.StationaryColor);
                    DrawColorProperty(SR.VelocityColorModifier_Property_VelocityColor_Name, SR.VelocityColorModifier_Property_VelocityColor_Description, ref velocityColor.VelocityColor);
                    DrawFloatProperty(SR.VelocityColorModifier_Property_VelocityThreshold_Name, SR.VelocityColorModifier_Property_VelocityThreshold_Description, ref velocityColor.VelocityThreshold, 0.1f, 0.0f, float.MaxValue);
                    break;

                case VelocityModifier velocity:
                    DrawFloatProperty(SR.VelocityModifier_Property_VelocityThreshold_Name, SR.VelocityModifier_Property_VelocityThreshold_Description, ref velocity.VelocityThreshold, 0.1f, 0.0f, float.MaxValue);
                    break;

                case VortexModifier vortex:
                    DrawVector2Property(SR.VortexModifier_Property_Position_Name, SR.VortexModifier_Property_Position_Description, ref vortex.Position, 0.1f, float.MinValue, float.MaxValue);
                    DrawFloatProperty(SR.VortexModifier_Property_Mass_Name, SR.VortexModifier_Property_Mass_Description, ref vortex.Mass, 0.1f, float.MinValue, float.MaxValue);
                    DrawFloatProperty(SR.VortexModifier_Property_MaxSpeed_Name, SR.VortexModifier_Property_MaxSpeed_Description, ref vortex.MaxSpeed, 0.1f, float.MinValue, float.MaxValue);
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
        if (ImGui.InputText("##selected_modifier_name_value"u8, ref modifier.Name, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
        {
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
        float frequency = modifier.Frequency;
        if (ImGui.DragFloat(SR.Modifier_Property_Frequency_Description, ref frequency, 0.1f, 0.0f, float.MaxValue, "%.2f"))
        {
            modifier.Frequency = frequency;
            EmberContext.HasUnsavedChanges = true;
        }
    }

    private static void DrawBoolProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref bool value)
    {
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
            EmberContext.HasUnsavedChanges = true;
        }

        ImGui.PopID();
    }

    private static void DrawIntProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref int value, int step, int min, int max)
    {
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
            EmberContext.HasUnsavedChanges = true;
        }

        ImGui.PopID();
    }

    private static void DrawFloatProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref float value, float step, float min, float max)
    {
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
            EmberContext.HasUnsavedChanges = true;
        }

        ImGui.PopID();
    }

    private static void DrawVector2Property(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref XnaVec2 value, float step, float min, float max)
    {
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
            EmberContext.HasUnsavedChanges = true;
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(dragWidth);
        if (ImGui.DragFloat("##y"u8, ref value.Y, step, min, max, "Y: %.2f"u8))
        {
            EmberContext.HasUnsavedChanges = true;
        }

        ImGui.PopID();
    }

    private static void DrawColorProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref XnaVec3 value)
    {
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

        HslColor hslColor = new(value.X, value.Y, value.Z);
        XnaColor rgbColor = HslColor.ToRgb(hslColor);
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
                HslColor newHslColor = HslColor.FromRgb(newColor);
                value.X = newHslColor.H;
                value.Y = newHslColor.S;
                value.Z = newHslColor.L;

                EmberContext.HasUnsavedChanges = true;
            }

            ImGui.EndPopup();
        }

        ImGui.PopID();
    }
}
