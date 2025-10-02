// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hexa.NET.ImGui;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Profiles;

namespace Ember.UI.ChildWindows;

public static class SelectedEmitterProfileChildWindow
{
    private static readonly Dictionary<Type, DisplayAttribute> s_profiles = [];
    private static readonly Dictionary<CircleRadiation, DisplayAttribute> s_radiations = [];

    static SelectedEmitterProfileChildWindow()
    {
        // TODO: Hardcoding these for now. What I'd like to eventually do is move this to using the actual
        //       DisplayAttribute on the classes in MGE and then use reflection once to read them here and
        //       store them.  Then ultimately do the same thing for all class, properties, and fields throughout
        //       the particle system implementation in MGE, so that it could allow users to easily add new
        //       types from their own libraries.  Need to really think on this design more though before
        //       implementing it since it's going to rely on reflection further down in the actual draw calls.

        s_profiles[typeof(BoxFillProfile)] = new DisplayAttribute() { Name = nameof(SR.BoxFillProfile_Name), Description = nameof(SR.BoxFillProfile_Description) };
        s_profiles[typeof(BoxProfile)] = new DisplayAttribute() { Name = nameof(SR.BoxProfile_Name), Description = nameof(SR.BoxProfile_Description) };
        s_profiles[typeof(BoxUniformProfile)] = new DisplayAttribute() { Name = nameof(SR.BoxUniformProfile_Name), Description = nameof(SR.BoxUniformProfile_Description) };
        s_profiles[typeof(CircleProfile)] = new DisplayAttribute() { Name = nameof(SR.CircleProfile_Name), Description = nameof(SR.CircleProfile_Description) };
        s_profiles[typeof(LineProfile)] = new DisplayAttribute() { Name = nameof(SR.LineProfile_Name), Description = nameof(SR.LineProfile_Description) };
        s_profiles[typeof(PointProfile)] = new DisplayAttribute() { Name = nameof(SR.PointProfile_Name), Description = nameof(SR.PointProfile_Description) };
        s_profiles[typeof(RingProfile)] = new DisplayAttribute() { Name = nameof(SR.RingProfile_Name), Description = nameof(SR.RingProfile_Description) };
        s_profiles[typeof(SprayProfile)] = new DisplayAttribute() { Name = nameof(SR.SprayProfile_Name), Description = nameof(SR.SprayProfile_Description) };


        s_radiations[CircleRadiation.None] = new DisplayAttribute() { Name = nameof(SR.CircleRadiation_None_Name), Description = nameof(SR.CircleRadiation_None_Description) };
        s_radiations[CircleRadiation.In] = new DisplayAttribute() { Name = nameof(SR.CircleRadiation_In_Name), Description = nameof(SR.CircleRadiation_In_Description) };
        s_radiations[CircleRadiation.Out] = new DisplayAttribute() { Name = nameof(SR.CircleRadiation_Out_Name), Description = nameof(SR.CircleRadiation_Out_Description) };
    }

    public static void Draw()
    {
        if (ImGui.CollapsingHeader(SR.CollapsingHeader_SelectedEmitterProfile, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY;

            if (ImGui.BeginChild("##selected_emitter_profile_child_window"u8, SysVec2.Zero, childFlags))
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
        if (ImGui.BeginTable("##selected_emitter_profile_properties_table"u8, 2, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("##selected_emitter_profile_property_label_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##selected_emitter_profile_property_value_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

            DrawSelectedEmitterProfileTypeRow(emitter);
            DrawSelectedEmitterProfilePropertiesRow(emitter);

            ImGui.EndTable();
        }
        ImGui.EndDisabled();
    }

    private static void DrawSelectedEmitterProfileTypeRow(ParticleEmitter emitter)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.Profile_Name);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(SR.Profile_Description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ReadOnlySpan<byte> preview = SR.GetResourceUtf8Bytes(s_profiles[emitter.Profile.GetType()].Name);
        if (ImGui.BeginCombo("##selected_emitter_profile_value"u8, preview))
        {
            foreach (var kvp in s_profiles)
            {
                Type profileType = kvp.Key;
                DisplayAttribute display = s_profiles[profileType];

                bool isSelected = emitter.Profile.GetType() == profileType;

                if (ImGui.Selectable(SR.GetResourceUtf8Bytes(display.Name), isSelected))
                {
                    emitter.Profile = (Profile)Activator.CreateInstance(profileType);
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
    }

    private static void DrawSelectedEmitterProfilePropertiesRow(ParticleEmitter emitter)
    {
        switch (emitter.Profile)
        {
            case BoxFillProfile boxFill:
                float boxFillWidth = boxFill.Width;
                if (DrawFloatProperty(SR.BoxFillProfile_Property_Width_Name, "##box_fill_width"u8, ref boxFillWidth, 0.1f, 0.0f, float.MaxValue))
                {
                    boxFill.Width = boxFillWidth;
                    EmberContext.HasUnsavedChanges = true;
                }

                float boxFillHeight = boxFill.Height;
                if (DrawFloatProperty(SR.BoxFillProfile_Property_Height_Name, "##box_fill_height"u8, ref boxFillHeight, 0.1f, 0.0f, float.MaxValue))
                {
                    boxFill.Height = boxFillHeight;
                    EmberContext.HasUnsavedChanges = true;
                }
                break;

            case BoxProfile box:
                float boxWidth = box.Width;
                if (DrawFloatProperty(SR.BoxProfile_Property_Width_Name, "##box_width"u8, ref boxWidth, 0.1f, 0.0f, float.MaxValue))
                {
                    box.Width = boxWidth;
                    EmberContext.HasUnsavedChanges = true;
                }

                float boxHeight = box.Height;
                if (DrawFloatProperty(SR.BoxProfile_Property_Height_Name, "##box_height"u8, ref boxHeight, 0.1f, 0.0f, float.MaxValue))
                {
                    box.Height = boxHeight;
                    EmberContext.HasUnsavedChanges = true;
                }
                break;

            case BoxUniformProfile boxUniform:
                float boxUniformWidth = boxUniform.Width;
                if (DrawFloatProperty(SR.BoxUniformProfile_Property_Width_Name, "##box_uniform_width"u8, ref boxUniformWidth, 0.1f, 0.0f, float.MaxValue))
                {
                    boxUniform.Width = boxUniformWidth;
                    EmberContext.HasUnsavedChanges = true;
                }

                float boxUniformHeight = boxUniform.Height;
                if (DrawFloatProperty(SR.BoxUniformProfile_Property_Height_Name, "##box_uniform_height"u8, ref boxUniformHeight, 0.1f, 0.0f, float.MaxValue))
                {
                    boxUniform.Height = boxUniformHeight;
                    EmberContext.HasUnsavedChanges = true;
                }
                break;

            case CircleProfile circle:
                float circleRadius = circle.Radius;
                if (DrawFloatProperty(SR.CircleProfile_Property_Radius_Name, "##circle_radius"u8, ref circleRadius, 0.1f, 0.0f, float.MaxValue))
                {
                    circle.Radius = circleRadius;
                    EmberContext.HasUnsavedChanges = true;
                }

                CircleRadiation circleRadiate = circle.Radiate;
                if (DrawRadiationProperty(SR.CircleProfile_Property_Radiate_Name, "##circle_radiate"u8, ref circleRadiate))
                {
                    circle.Radiate = circleRadiate;
                    EmberContext.HasUnsavedChanges = true;
                }
                break;

            case LineProfile line:
                XnaVec2 lineAxis = line.Axis;
                if (DrawVector2Property(SR.LineProfile_Property_Axis_Name, "##line_axis_x"u8, "##line_axis_y"u8, ref lineAxis, 1f, -1f, 1f))
                {
                    line.Axis = lineAxis;
                    EmberContext.HasUnsavedChanges = true;
                }

                float lineLength = line.Length;
                if (DrawFloatProperty(SR.LineProfile_Property_Length_Name, "##line_length"u8, ref lineLength, 0.1f, 0.0f, float.MaxValue))
                {
                    line.Length = lineLength;
                    EmberContext.HasUnsavedChanges = true;
                }
                break;

            case PointProfile point:
                // No additional properties
                break;

            case RingProfile ring:
                float ringRadius = ring.Radius;
                if (DrawFloatProperty(SR.RingProfile_Property_Radius_Name, "##ring_radius"u8, ref ringRadius, 0.1f, 0.0f, float.MaxValue))
                {
                    ring.Radius = ringRadius;
                    EmberContext.HasUnsavedChanges = true;
                }

                CircleRadiation ringRadiate = ring.Radiate;
                if (DrawRadiationProperty(SR.RingProfile_Property_Radiate_Name, "##ring_radiate"u8, ref ringRadiate))
                {
                    ring.Radiate = ringRadiate;
                    EmberContext.HasUnsavedChanges = true;
                }
                break;

            case SprayProfile spray:
                XnaVec2 sprayDirection = spray.Direction;
                if (DrawVector2Property(SR.SprayProfile_Property_Direction_Name, "##spray_direction_x"u8, "##spray_direction_y"u8, ref sprayDirection, 0.1f, float.MinValue, float.MaxValue))
                {
                    spray.Direction = sprayDirection;
                    EmberContext.HasUnsavedChanges = true;
                }

                float spraySpread = spray.Spread;
                if (DrawFloatProperty(SR.SprayProfile_Property_Spread_Name, "##spray_spread"u8, ref spraySpread, 0.1f, 0.0f, float.MaxValue))
                {
                    spray.Spread = spraySpread;
                    EmberContext.HasUnsavedChanges = true;
                }
                break;
        }
    }

    private static bool DrawFloatProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> id, ref float value, float step, float min, float max)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);

        return ImGui.DragFloat(id, ref value, step, min, max, "%.2f"u8);
    }

    private static bool DrawVector2Property(ReadOnlySpan<byte> label, ReadOnlySpan<byte> idX, ReadOnlySpan<byte> idY, ref XnaVec2 value, float step, float min, float max)
    {
        bool result = false;

        ImGuiStylePtr stylePtr = ImGui.GetStyle();

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        ImGui.TableNextColumn();
        float availWidth = ImGui.GetContentRegionAvail().X;
        float itemSpacingWidth = stylePtr.ItemSpacing.X;
        float dragWidth = (availWidth - itemSpacingWidth) * 0.5f;
        ImGui.SetNextItemWidth(dragWidth);

        if (ImGui.DragFloat(idX, ref value.X, step, min, max, "X: %.2F"u8))
        {
            result = true;
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(dragWidth);
        if (ImGui.DragFloat(idY, ref value.Y, step, min, max, "Y: %.2f"u8))
        {
            result = true;
        }

        return result;
    }

    private static bool DrawRadiationProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> id, ref CircleRadiation value)
    {
        bool result = false;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ReadOnlySpan<byte> preview = SR.GetResourceUtf8Bytes(s_radiations[value].Name);
        if (ImGui.BeginCombo(id, preview))
        {
            foreach (var kvp in s_radiations)
            {
                CircleRadiation radiation = kvp.Key;
                DisplayAttribute display = kvp.Value;

                bool isSelected = value == radiation;

                if (ImGui.Selectable(SR.GetResourceUtf8Bytes(display.Name), isSelected))
                {
                    value = radiation;
                    result = true;
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

        return result;
    }

}
