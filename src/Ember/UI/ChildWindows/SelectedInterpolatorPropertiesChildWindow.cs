// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Hexa.NET.ImGui;
using MonoGame.Extended;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.UI.ChildWindows;

public static class SelectedInterpolatorPropertiesChildWindow
{
    public static void Draw()
    {
        if (ImGui.CollapsingHeader(SR.CollapsingHeader_SelectedInterpolatorProperties, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY;

            if (ImGui.BeginChild("##selected_interpolator_properties_child_window"u8, SysVec2.Zero, childFlags))
            {
                DrawContent();
            }
            ImGui.EndChild();
        }
    }

    private static void DrawContent()
    {
        if (EmberContext.SelectedInterpolator is not Interpolator interpolator)
        {
            ImGui.TextDisabled(SR.Message_NoInterpolatorSelected);
            return;
        }

        bool isLocked = EmberContext.IsLocked(EmberContext.SelectedParticleEmitter)
                        || EmberContext.IsLocked(EmberContext.SelectedModifier)
                        || EmberContext.IsLocked(interpolator);

        ImGui.BeginDisabled(isLocked);
        if (ImGui.BeginTable("##selected_interpolator_properties_table"u8, 2, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("##selected_interpolator_property_label_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##selected_interpolator_property_value_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

            DrawSelectedInterpolatorNamePropertyRow(interpolator);

            switch (interpolator)
            {
                case ColorInterpolator colorInterpolator:
                    DrawColorProperty(SR.Interpolator_Property_StartValue_Name, SR.ColorInterpolator_Property_StartValue_Description, ref colorInterpolator.StartValue);
                    DrawColorProperty(SR.Interpolator_Property_EndValue_Name, SR.ColorInterpolator_Property_EndValue_Description, ref colorInterpolator.EndValue);
                    break;

                case HueInterpolator hueInterpolator:
                    DrawFloatProperty(SR.Interpolator_Property_StartValue_Name, SR.HueInterpolator_Property_StartValue_Description, ref hueInterpolator.StartValue, 0.01f, 0.0f, 1.0f);
                    DrawFloatProperty(SR.Interpolator_Property_EndValue_Name, SR.HueInterpolator_Property_EndValue_Description, ref hueInterpolator.EndValue, 0.01f, 0.0f, 1.0f);
                    break;

                case OpacityInterpolator opacityInterpolator:
                    DrawFloatProperty(SR.Interpolator_Property_StartValue_Name, SR.OpacityInterpolator_Property_StartValue_Description, ref opacityInterpolator.StartValue, 0.01f, 0.0f, 1.0f);
                    DrawFloatProperty(SR.Interpolator_Property_EndValue_Name, SR.OpacityInterpolator_Property_EndValue_Description, ref opacityInterpolator.EndValue, 0.01f, 0.0f, 1.0f);
                    break;

                case RotationInterpolator rotationInterpolator:
                    DrawFloatProperty(SR.Interpolator_Property_StartValue_Name, SR.RotationInterpolator_Property_StartValue_Description, ref rotationInterpolator.StartValue, 0.01f, -MathF.PI * 2.0f, MathF.PI * 2.0f);
                    DrawFloatProperty(SR.Interpolator_Property_EndValue_Name, SR.RotationInterpolator_Property_EndValue_Description, ref rotationInterpolator.EndValue, 0.01f, -MathF.PI * 2.0f, MathF.PI * 2.0f);
                    break;

                case ScaleInterpolator scaleInterpolator:
                    DrawVector2Property(SR.Interpolator_Property_StartValue_Name, SR.ScaleInterpolator_Property_StartValue_Description, ref scaleInterpolator.StartValue, 0.01f, 0.0f, 10.0f);
                    DrawVector2Property(SR.Interpolator_Property_EndValue_Name, SR.ScaleInterpolator_Property_EndValue_Description, ref scaleInterpolator.EndValue, 0.01f, 0.0f, 10.0f);
                    break;

                case VelocityInterpolator velocityInterpolator:
                    DrawVector2Property(SR.Interpolator_Property_StartValue_Name, SR.VelocityInterpolator_Property_StartValue_Description, ref velocityInterpolator.StartValue, 1.0f, -1000.0f, 1000.0f);
                    DrawVector2Property(SR.Interpolator_Property_EndValue_Name, SR.VelocityInterpolator_Property_EndValue_Description, ref velocityInterpolator.EndValue, 1.0f, -1000.0f, 1000.0f);
                    break;
            }

            ImGui.EndTable();
        }
        ImGui.EndDisabled();
    }

    private static void DrawSelectedInterpolatorNamePropertyRow(Interpolator interpolator)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.Interpolator_Property_Name_Name);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(SR.Interpolator_Property_Name_Description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputText("##selected_interpolator_name_value"u8, ref interpolator.Name, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
        {
            EmberContext.HasUnsavedChanges = true;
        }
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
