// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hexa.NET.ImGui;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Data;

namespace Ember.UI.ChildWindows;

public static class SelectedEmitterReleaseParametersChildWindow
{
    private static readonly Dictionary<ParticleValueKind, DisplayAttribute> s_valueKinds = [];

    static SelectedEmitterReleaseParametersChildWindow()
    {
        // TODO: Hardcoding these for now. What I'd like to eventually do is move this to using the actual
        //       DisplayAttribute on the classes in MGE and then use reflection once to read them here and
        //       store them.  Then ultimately do the same thing for all class, properties, and fields throughout
        //       the particle system implementation in MGE, so that it could allow users to easily add new
        //       types from their own libraries.  Need to really think on this design more though before
        //       implementing it since it's going to rely on reflection further down in the actual draw calls.

        s_valueKinds[ParticleValueKind.Constant] = new DisplayAttribute() { Name = nameof(SR.ParticleValueKind_Constant_Name), Description = nameof(SR.ParticleValueKind_Constant_Description) };
        s_valueKinds[ParticleValueKind.Random] = new DisplayAttribute() { Name = nameof(SR.ParticleValueKind_Random_Name), Description = nameof(SR.ParticleValueKind_Random_Description) };
    }

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
        if (ImGui.BeginTable("##selected_emitter_release_parameters_properties_table"u8, 4, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("##selected_emitter_release_parameters_label_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##selected_emitter_release_parameters_kind_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##selected_emitter_release_parameters_value_column"u8, ImGuiTableColumnFlags.WidthStretch, 2.0f);

            DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Quantity_Name, SR.ReleaseParameter_Quantity_Description, ref emitter.Parameters.Quantity);
            DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Speed_Name, SR.ReleaseParameter_Speed_Description, ref emitter.Parameters.Speed);
            DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Color_Name, SR.ReleaseParameter_Color_Description, ref emitter.Parameters.Color);
            DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Opacity_Name, SR.ReleaseParameter_Opacity_Description, ref emitter.Parameters.Opacity);
            DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Scale_Name, SR.ReleaseParameter_Scale_Description, ref emitter.Parameters.Scale);
            DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Rotation_Name, SR.ReleaseParameter_Rotation_Description, ref emitter.Parameters.Rotation);
            DrawEmitterReleaseParameterRow(SR.ReleaseParameter_Mass_Name, SR.ReleaseParameter_Mass_Description, ref emitter.Parameters.Mass);

            ImGui.EndTable();
        }
        ImGui.EndDisabled();
    }

    private static void DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref ParticleInt32Parameter parameter)
    {
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
        ImGui.SetNextItemWidth(-1);
        ReadOnlySpan<byte> preview = SR.GetResourceUtf8Bytes(s_valueKinds[parameter.Kind].Name);
        if (ImGui.BeginCombo("##parameter_kind"u8, preview))
        {
            foreach (var kvp in s_valueKinds)
            {
                ParticleValueKind kind = kvp.Key;
                DisplayAttribute display = kvp.Value;

                bool isSelected = kind == parameter.Kind;

                if (ImGui.Selectable(SR.GetResourceUtf8Bytes(display.Name), isSelected))
                {
                    parameter.Kind = kind;
                    EmberContext.HasUnsavedChanges = true;
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                {
                    ImGui.SetTooltip(SR.GetResourceUtf8Bytes(display.Description));
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }

        if (parameter.Kind == ParticleValueKind.Constant)
        {
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.DragInt("##constant_value"u8, ref parameter.Constant, 1, 0, int.MaxValue);
        }
        else
        {
            ImGui.TableNextColumn();

            float availWidth = ImGui.GetContentRegionAvail().X;
            float toWidth = ImGui.CalcTextSize(" to ").X;
            float spacing = style.ItemSpacing.X * 2.0f;

            float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

            ImGui.SetNextItemWidth(dragWidth);
            ImGui.DragInt("##random_min_value"u8, ref parameter.RandomMin, 1, 0, parameter.RandomMax);

            ImGui.SameLine();
            ImGui.Text(SR.Label_To);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(dragWidth);
            ImGui.DragInt("##random_max_value"u8, ref parameter.RandomMax, 1, parameter.RandomMin, int.MaxValue);
        }

        ImGui.PopID();
    }

    private static void DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref ParticleFloatParameter parameter)
    {
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
        ImGui.SetNextItemWidth(-1);
        ReadOnlySpan<byte> preview = SR.GetResourceUtf8Bytes(s_valueKinds[parameter.Kind].Name);
        if (ImGui.BeginCombo("##parameter_kind"u8, preview))
        {
            foreach (var kvp in s_valueKinds)
            {
                ParticleValueKind kind = kvp.Key;
                DisplayAttribute display = kvp.Value;

                bool isSelected = kind == parameter.Kind;

                if (ImGui.Selectable(SR.GetResourceUtf8Bytes(display.Name), isSelected))
                {
                    parameter.Kind = kind;
                    EmberContext.HasUnsavedChanges = true;
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                {
                    ImGui.SetTooltip(SR.GetResourceUtf8Bytes(display.Description));
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }

        if (parameter.Kind == ParticleValueKind.Constant)
        {
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.DragFloat("##constant_value"u8, ref parameter.Constant, 0.1f, 0, float.MaxValue, "%.2f"u8);
        }
        else
        {
            ImGui.TableNextColumn();

            float availWidth = ImGui.GetContentRegionAvail().X;
            float toWidth = ImGui.CalcTextSize(SR.Label_To).X;
            float spacing = style.ItemSpacing.X * 2.0f;

            float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

            ImGui.SetNextItemWidth(dragWidth);
            ImGui.DragFloat("##random_min_value"u8, ref parameter.RandomMin, 0.1f, 0, parameter.RandomMax, "%.2f"u8);

            ImGui.SameLine();
            ImGui.Text(SR.Label_To);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(dragWidth);
            ImGui.DragFloat("##random_max_value"u8, ref parameter.RandomMax, 1, parameter.RandomMin, float.MaxValue, "%.2f"u8);
        }

        ImGui.PopID();
    }

    private static void DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref ParticleVector2Parameter parameter)
    {
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
        ImGui.SetNextItemWidth(-1);
        ReadOnlySpan<byte> preview = SR.GetResourceUtf8Bytes(s_valueKinds[parameter.Kind].Name);
        if (ImGui.BeginCombo("##parameter_kind"u8, preview))
        {
            foreach (var kvp in s_valueKinds)
            {
                ParticleValueKind kind = kvp.Key;
                DisplayAttribute display = kvp.Value;

                bool isSelected = kind == parameter.Kind;

                if (ImGui.Selectable(SR.GetResourceUtf8Bytes(display.Name), isSelected))
                {
                    parameter.Kind = kind;
                    EmberContext.HasUnsavedChanges = true;
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                {
                    ImGui.SetTooltip(SR.GetResourceUtf8Bytes(display.Description));
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }


        if (parameter.Kind == ParticleValueKind.Constant)
        {
            ImGui.TableNextColumn();

            float availWidth = ImGui.GetContentRegionAvail().X;
            float spacing = style.ItemSpacing.X;
            float dragWidth = (availWidth - spacing) * 0.5f;

            ImGui.SetNextItemWidth(dragWidth);
            ImGui.DragFloat("##constant_x_value"u8, ref parameter.Constant.X, 0.1f, 0.0f, float.MaxValue, "X: %.2f"u8);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(dragWidth);
            ImGui.DragFloat("##constant_y_value"u8, ref parameter.Constant.Y, 0.1f, 0.0f, float.MaxValue, "Y: %.2f"u8);

        }
        else
        {
            ImGui.TableNextColumn();

            float availWidth = ImGui.GetContentRegionAvail().X;
            float toWidth = ImGui.CalcTextSize(SR.Label_To).X;
            float spacing = style.ItemSpacing.X * 2.0f;
            float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

            ImGui.SetNextItemWidth(dragWidth);
            ImGui.DragFloat("##random_min_x_value"u8, ref parameter.RandomMin.X, 0.1f, 0.0f, parameter.RandomMax.X, "X: %.2f"u8);

            ImGui.SameLine();
            ImGui.Text(SR.Label_To);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(dragWidth);
            ImGui.DragFloat("##random_max_x_value"u8, ref parameter.RandomMax.X, 0.1f, parameter.RandomMin.X, float.MaxValue, "X: %.2f"u8);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();

            ImGui.SetNextItemWidth(dragWidth);
            ImGui.DragFloat("##random_min_y_value"u8, ref parameter.RandomMin.Y, 0.1f, 0.0f, parameter.RandomMax.Y, "Y: %.2f"u8);

            ImGui.SameLine();
            ImGui.Text(SR.Label_To);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(dragWidth);
            ImGui.DragFloat("##random_max_y_value"u8, ref parameter.RandomMax.Y, 0.1f, parameter.RandomMin.Y, float.MaxValue, "Y: %.2f"u8);
        }

        ImGui.PopID();
    }

    private static void DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref ParticleColorParameter parameter)
    {
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
        ImGui.SetNextItemWidth(-1);
        ReadOnlySpan<byte> preview = SR.GetResourceUtf8Bytes(s_valueKinds[parameter.Kind].Name);
        if (ImGui.BeginCombo("##parameter_kind"u8, preview))
        {
            foreach (var kvp in s_valueKinds)
            {
                ParticleValueKind kind = kvp.Key;
                DisplayAttribute display = kvp.Value;

                bool isSelected = kind == parameter.Kind;

                if (ImGui.Selectable(SR.GetResourceUtf8Bytes(display.Name), isSelected))
                {
                    parameter.Kind = kind;
                    EmberContext.HasUnsavedChanges = true;
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                {
                    ImGui.SetTooltip(SR.GetResourceUtf8Bytes(display.Description));
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }


        if (parameter.Kind == ParticleValueKind.Constant)
        {
            ImGui.TableNextColumn();

            HslColor hslColor = new HslColor(parameter.Constant.X, parameter.Constant.Y, parameter.Constant.Z);
            XnaColor rgbColor = HslColor.ToRgb(hslColor);
            SysVec4 color = new SysVec4(rgbColor.R / 255.0f, rgbColor.G / 255.0f, rgbColor.B / 255.0f, 1.0f);

            SysVec2 contentRegionAvail = ImGui.GetContentRegionAvail();
            SysVec2 buttonSize = new SysVec2(contentRegionAvail.X, ImGui.GetFrameHeight());

            if (ImGui.ColorButton("##constant_value_button"u8, color, ImGuiColorEditFlags.None, buttonSize))
            {
                ImGui.OpenPopup("##constant_value_color_picker"u8);
            }

            if (ImGui.BeginPopup("##constant_value_color_picker"u8))
            {
                float[] rgb = [color.X, color.Y, color.Z];
                if (ImGui.ColorPicker3("##constant_value"u8, rgb))
                {
                    XnaColor newColor = new XnaColor(rgb[0], rgb[1], rgb[2]);
                    HslColor newHslColor = HslColor.FromRgb(newColor);
                    parameter.Constant = new SysVec3(newHslColor.H, newHslColor.S, newHslColor.L);
                }
                ImGui.EndPopup();
            }

        }
        else
        {
            ImGui.TableNextColumn();

            float availableWidth = ImGui.GetContentRegionAvail().X;
            float toWidth = ImGui.CalcTextSize(SR.Label_To).X;
            float spacing = style.ItemSpacing.X * 2.0f;
            float buttonWidth = (availableWidth - toWidth - spacing) * 0.5f;
            SysVec2 buttonSize = new SysVec2(buttonWidth, ImGui.GetFrameHeight());

            HslColor hslMin = new HslColor(parameter.RandomMin.X, parameter.RandomMin.Y, parameter.RandomMin.Z);
            XnaColor rgbMin = HslColor.ToRgb(hslMin);
            SysVec4 colorMin = new SysVec4(rgbMin.R / 255.0f, rgbMin.G / 255.0f, rgbMin.B / 255.0f, 1.0f);

            if (ImGui.ColorButton("##random_min_value_button"u8, colorMin, ImGuiColorEditFlags.None, buttonSize))
            {
                ImGui.OpenPopup("##random_min_value_color_picker"u8);
            }

            if (ImGui.BeginPopup("##random_min_value_color_picker"u8))
            {
                float[] rgb = [colorMin.X, colorMin.Y, colorMin.Z];
                if (ImGui.ColorPicker3("##random_min_value"u8, rgb))
                {
                    XnaColor newRgb = new XnaColor(rgb[0], rgb[1], rgb[2]);
                    HslColor newHsl = HslColor.FromRgb(newRgb);
                    parameter.RandomMin = new XnaVec3(newHsl.H, newHsl.S, newHsl.L);
                }

                ImGui.EndPopup();
            }

            ImGui.SameLine();
            ImGui.Text(SR.Label_To);

            HslColor hslMax = new HslColor(parameter.RandomMax.X, parameter.RandomMax.Y, parameter.RandomMax.Z);
            XnaColor rgbMax = HslColor.ToRgb(hslMax);
            SysVec4 colorMax = new SysVec4(rgbMax.R / 255.0f, rgbMax.G / 255.0f, rgbMax.B / 255.0f, 1.0f);
            ImGui.SameLine();
            if (ImGui.ColorButton("##random_max_value_button"u8, colorMax, ImGuiColorEditFlags.None, buttonSize))
            {
                ImGui.OpenPopup("##random_max_value_color_picker"u8);
            }

            if (ImGui.BeginPopup("##random_max_value_color_picker"u8))
            {
                float[] rgb = [colorMax.X, colorMax.Y, colorMax.Z];
                if (ImGui.ColorPicker3("##random_max_value"u8, rgb))
                {
                    XnaColor newRgb = new XnaColor(rgb[0], rgb[1], rgb[2]);
                    HslColor newHsl = HslColor.FromRgb(newRgb);
                    parameter.RandomMax = new XnaVec3(newHsl.H, newHsl.S, newHsl.L);
                }

                ImGui.EndPopup();
            }
        }
        ImGui.PopID();
    }
}
