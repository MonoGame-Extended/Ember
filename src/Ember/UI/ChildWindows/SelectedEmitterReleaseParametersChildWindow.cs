// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Hexa.NET.ImGui;
using MonoGame.Extended;
using MonoGame.Extended.Particles;

namespace Ember.UI.ChildWindows;

public static class SelectedEmitterReleaseParametersChildWindow
{
    public static void Draw()
    {
        if (ImGui.CollapsingHeader(SR.CollapsingHeader_SelectedEmitterReleaseParameters, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY;

            if (ImGui.BeginChild("##selected_emitter_release_parameters_child_window"u8, SysVec2.Zero, childFlags))
            {
                DrawContent();
            }
            ImGui.EndChild();
        }
    }

    private static void DrawContent()
    {
        if (EmberContext.SelectedParticleEmitter is not ParticleEmitter emitter)
        {
            ImGui.TextDisabled(SR.Message_NoParticleEmitterSelected);
            return;
        }

        ImGui.BeginDisabled(EmberContext.IsLocked(emitter));
        if (ImGui.BeginTable("##selected_emitter_release_parameters_properties_table"u8, 2, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("##selected_emitter_release_parameters_label_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##selected_emitter_release_parameters_value_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

            Interval<int> quantity = emitter.Parameters.Quantity;
            if (DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Quantity_Name, SR.ReleaseParameter_Quantity_Description, ref quantity))
            {
                emitter.Parameters.Quantity = quantity;
                EmberContext.HasUnsavedChanges = true;
            }

            Interval<float> speed = emitter.Parameters.Speed;
            if (DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Speed_Name, SR.ReleaseParameter_Speed_Description, ref speed))
            {
                emitter.Parameters.Speed = speed;
                EmberContext.HasUnsavedChanges = true;
            }

            Interval<HslColor> color = emitter.Parameters.Color;
            if (DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Color_Name, SR.ReleaseParameter_Color_Description, ref color))
            {
                emitter.Parameters.Color = color;
                EmberContext.HasUnsavedChanges = true;
            }

            Interval<float> opacity = emitter.Parameters.Opacity;
            if (DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Opacity_Name, SR.ReleaseParameter_Opacity_Description, ref opacity))
            {
                emitter.Parameters.Opacity = opacity;
                EmberContext.HasUnsavedChanges = true;
            }

            Interval<float> scaleX = emitter.Parameters.ScaleX;
            if (DrawEmitterReleaseParameterRow(SR.ReleaseParameter_ScaleX_Name, SR.ReleaseParameter_ScaleX_Description, ref scaleX))
            {
                emitter.Parameters.ScaleX = scaleX;
                EmberContext.HasUnsavedChanges = true;
            }

            Interval<float> scaleY = emitter.Parameters.ScaleY;
            if (DrawEmitterReleaseParameterRow(SR.ReleaseParameter_ScaleY_Name, SR.ReleaseParameter_ScaleY_Description, ref scaleY))
            {
                emitter.Parameters.ScaleY = scaleY;
                EmberContext.HasUnsavedChanges = true;
            }

            Interval<float> rotation = emitter.Parameters.Rotation;
            if (DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Rotation_Name, SR.ReleaseParameter_Rotation_Description, ref rotation))
            {
                emitter.Parameters.Rotation = rotation;
                EmberContext.HasUnsavedChanges = true;
            }

            Interval<float> mass = emitter.Parameters.Mass;
            if (DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Mass_Name, SR.ReleaseParameter_Mass_Description, ref mass))
            {
                emitter.Parameters.Mass = mass;
                EmberContext.HasUnsavedChanges = true;
            }

            ImGui.EndTable();
        }
        ImGui.EndDisabled();
    }

    private static bool DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref Interval<int> parameter)
    {
        bool valueChanged = false;

        ImGui.PushID(label);
        ImGuiStylePtr style = ImGui.GetStyle();

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(description);
        }

        ImGui.TableNextColumn();

        float availWidth = ImGui.GetContentRegionAvail().X;
        float toWidth = ImGui.CalcTextSize(" to ").X;
        float spacing = style.ItemSpacing.X * 2.0f;

        float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

        ImGui.SetNextItemWidth(dragWidth);
        int min = parameter.Min;
        if (ImGui.DragInt("##min_value"u8, ref min, 1, 0, parameter.Max))
        {
            parameter = new(min, parameter.Max);
            valueChanged = true;
        }

        ImGui.SameLine();
        ImGui.Text(SR.Label_To);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(dragWidth);
        int max = parameter.Max;
        if (ImGui.DragInt("##max_value"u8, ref max, 1, parameter.Min, int.MaxValue))
        {
            parameter = new(parameter.Min, max);
            valueChanged = true;
        }

        ImGui.PopID();

        return valueChanged;
    }

    private static bool DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref Interval<float> parameter)
    {
        bool valueChanged = false;

        ImGui.PushID(label);
        ImGuiStylePtr style = ImGui.GetStyle();

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(description);
        }

        ImGui.TableNextColumn();

        float availWidth = ImGui.GetContentRegionAvail().X;
        float toWidth = ImGui.CalcTextSize(SR.Label_To).X;
        float spacing = style.ItemSpacing.X * 2.0f;

        float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

        ImGui.SetNextItemWidth(dragWidth);
        float min = parameter.Min;
        if (ImGui.DragFloat("##min_value"u8, ref min, 0.1f, 0, parameter.Max, "%.2f"u8))
        {
            parameter = new(min, parameter.Max);
            valueChanged = true;
        }

        ImGui.SameLine();
        ImGui.Text(SR.Label_To);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(dragWidth);
        float max = parameter.Max;
        if (ImGui.DragFloat("##max_value"u8, ref max, 1, parameter.Min, float.MaxValue, "%.2f"u8))
        {
            parameter = new(parameter.Min, max);
            valueChanged = true;
        }

        ImGui.PopID();

        return valueChanged;
    }

    private static bool DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref Interval<HslColor> parameter)
    {
        bool valueChanged = false;

        ImGui.PushID(label);
        ImGuiStylePtr style = ImGui.GetStyle();

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(description);
        }

        ImGui.TableNextColumn();

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float toWidth = ImGui.CalcTextSize(SR.Label_To).X;
        float spacing = style.ItemSpacing.X * 2.0f;
        float buttonWidth = (availableWidth - toWidth - spacing) * 0.5f;
        SysVec2 buttonSize = new SysVec2(buttonWidth, ImGui.GetFrameHeight());

        XnaColor rgbMin = HslColor.ToRgb(parameter.Min);
        SysVec4 colorMin = new SysVec4(rgbMin.R / 255.0f, rgbMin.G / 255.0f, rgbMin.B / 255.0f, 1.0f);

        if (ImGui.ColorButton("##min_value_button"u8, colorMin, ImGuiColorEditFlags.None, buttonSize))
        {
            ImGui.OpenPopup("##min_value_color_picker"u8);
        }

        if (ImGui.BeginPopup("##min_value_color_picker"u8))
        {
            float[] rgb = [colorMin.X, colorMin.Y, colorMin.Z];
            if (ImGui.ColorPicker3("##min_value"u8, rgb))
            {
                XnaColor newRgb = new XnaColor(rgb[0], rgb[1], rgb[2]);
                HslColor newHsl = HslColor.FromRgb(newRgb);
                parameter = new(newHsl, parameter.Max);
                valueChanged = true;
            }

            ImGui.EndPopup();
        }

        ImGui.SameLine();
        ImGui.Text(SR.Label_To);

        XnaColor rgbMax = HslColor.ToRgb(parameter.Max);
        SysVec4 colorMax = new SysVec4(rgbMax.R / 255.0f, rgbMax.G / 255.0f, rgbMax.B / 255.0f, 1.0f);
        ImGui.SameLine();
        if (ImGui.ColorButton("##max_value_button"u8, colorMax, ImGuiColorEditFlags.None, buttonSize))
        {
            ImGui.OpenPopup("##max_value_color_picker"u8);
        }

        if (ImGui.BeginPopup("##max_value_color_picker"u8))
        {
            float[] rgb = [colorMax.X, colorMax.Y, colorMax.Z];
            if (ImGui.ColorPicker3("##max_value"u8, rgb))
            {
                XnaColor newRgb = new XnaColor(rgb[0], rgb[1], rgb[2]);
                HslColor newHsl = HslColor.FromRgb(newRgb);
                parameter = new(parameter.Min, newHsl);
                valueChanged = true;
            }

            ImGui.EndPopup();
        }

        ImGui.PopID();

        return valueChanged;
    }
}
