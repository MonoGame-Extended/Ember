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
                    HslColor colorInterpolatorStartValue = colorInterpolator.StartValue;
                    if (DrawColorProperty(SR.Interpolator_Property_StartValue_Name, SR.ColorInterpolator_Property_StartValue_Description, ref colorInterpolatorStartValue))
                    {
                        colorInterpolator.StartValue = colorInterpolatorStartValue;
                        EmberContext.HasUnsavedChanges = true;
                    }

                    HslColor colorInterpolatorEndValue = colorInterpolator.EndValue;
                    if (DrawColorProperty(SR.Interpolator_Property_EndValue_Name, SR.ColorInterpolator_Property_EndValue_Description, ref colorInterpolatorEndValue))
                    {
                        colorInterpolator.EndValue = colorInterpolatorEndValue;
                        EmberContext.HasUnsavedChanges = true;
                    }
                    break;

                case HueInterpolator hueInterpolator:
                    float hueInterpolatorStartValue = hueInterpolator.StartValue;
                    if (DrawFloatProperty(SR.Interpolator_Property_StartValue_Name, SR.HueInterpolator_Property_StartValue_Description, ref hueInterpolatorStartValue, 0.01f, 0.0f, 1.0f))
                    {
                        hueInterpolator.StartValue = hueInterpolatorStartValue;
                        EmberContext.HasUnsavedChanges = true;
                    }

                    float hueInterpolatorEndValue = hueInterpolator.EndValue;
                    if (DrawFloatProperty(SR.Interpolator_Property_EndValue_Name, SR.HueInterpolator_Property_EndValue_Description, ref hueInterpolatorEndValue, 0.01f, 0.0f, 1.0f))
                    {
                        hueInterpolator.EndValue = hueInterpolatorEndValue;
                        EmberContext.HasUnsavedChanges = true;
                    }
                    break;

                case OpacityInterpolator opacityInterpolator:
                    float opacityInterpolatorStartValue = opacityInterpolator.StartValue;
                    if (DrawFloatProperty(SR.Interpolator_Property_StartValue_Name, SR.OpacityInterpolator_Property_StartValue_Description, ref opacityInterpolatorStartValue, 0.01f, 0.0f, 1.0f))
                    {
                        opacityInterpolator.StartValue = opacityInterpolatorStartValue;
                        EmberContext.HasUnsavedChanges = true;
                    }

                    float opacityInterpolatorEndValue = opacityInterpolator.EndValue;
                    if (DrawFloatProperty(SR.Interpolator_Property_EndValue_Name, SR.OpacityInterpolator_Property_EndValue_Description, ref opacityInterpolatorEndValue, 0.01f, 0.0f, 1.0f))
                    {
                        opacityInterpolator.EndValue = opacityInterpolatorEndValue;
                        EmberContext.HasUnsavedChanges = true;
                    }
                    break;

                case RotationInterpolator rotationInterpolator:
                    float rotationInterpolatorStartValue = rotationInterpolator.StartValue;
                    if (DrawFloatProperty(SR.Interpolator_Property_StartValue_Name, SR.RotationInterpolator_Property_StartValue_Description, ref rotationInterpolatorStartValue, 0.01f, -MathF.PI * 2.0f, MathF.PI * 2.0f))
                    {
                        rotationInterpolator.StartValue = rotationInterpolatorStartValue;
                        EmberContext.HasUnsavedChanges = true;
                    }

                    float rotationInterpolatorEndValue = rotationInterpolator.EndValue;
                    if (DrawFloatProperty(SR.Interpolator_Property_EndValue_Name, SR.RotationInterpolator_Property_EndValue_Description, ref rotationInterpolatorEndValue, 0.01f, -MathF.PI * 2.0f, MathF.PI * 2.0f))
                    {
                        rotationInterpolator.EndValue = rotationInterpolatorEndValue;
                        EmberContext.HasUnsavedChanges = true;
                    }
                    break;

                case ScaleInterpolator scaleInterpolator:
                    XnaVec2 scaleInterpolatorStartValue = scaleInterpolator.StartValue;
                    if (DrawVector2Property(SR.Interpolator_Property_StartValue_Name, SR.ScaleInterpolator_Property_StartValue_Description, ref scaleInterpolatorStartValue, 0.01f, 0.0f, 10.0f))
                    {
                        scaleInterpolator.StartValue = scaleInterpolatorStartValue;
                        EmberContext.HasUnsavedChanges = true;
                    }

                    XnaVec2 scaleInterpolatorEndValue = scaleInterpolator.EndValue;
                    if (DrawVector2Property(SR.Interpolator_Property_EndValue_Name, SR.ScaleInterpolator_Property_EndValue_Description, ref scaleInterpolatorEndValue, 0.01f, 0.0f, 10.0f))
                    {
                        scaleInterpolator.EndValue = scaleInterpolatorEndValue;
                        EmberContext.HasUnsavedChanges = true;
                    }
                    break;

                case VelocityInterpolator velocityInterpolator:
                    XnaVec2 velocityInterpolatorStartValue = velocityInterpolator.StartValue;
                    if (DrawVector2Property(SR.Interpolator_Property_StartValue_Name, SR.VelocityInterpolator_Property_StartValue_Description, ref velocityInterpolatorStartValue, 1.0f, -1000.0f, 1000.0f))
                    {
                        velocityInterpolator.StartValue = velocityInterpolatorStartValue;
                        EmberContext.HasUnsavedChanges = true;
                    }

                    XnaVec2 velocityInterpolatorEndValue = velocityInterpolator.EndValue;
                    if (DrawVector2Property(SR.Interpolator_Property_EndValue_Name, SR.VelocityInterpolator_Property_EndValue_Description, ref velocityInterpolatorEndValue, 1.0f, -1000.0f, 1000.0f))
                    {
                        velocityInterpolator.EndValue = velocityInterpolatorEndValue;
                        EmberContext.HasUnsavedChanges = true;
                    }
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
        string interpolatorName = interpolator.Name;
        if (ImGui.InputText("##selected_interpolator_name_value"u8, ref interpolatorName, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
        {
            interpolator.Name = interpolatorName;
            EmberContext.HasUnsavedChanges = true;
        }
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
