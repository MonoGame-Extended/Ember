// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Hexa.NET.ImGui;

namespace Ember.UI.ChildWindows;

public static class ParticleEffectPropertiesChildWindow
{
    public static void Draw()
    {
        if (ImGui.CollapsingHeader(SR.CollapsingHeader_ParticleEffectProperties, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY;

            if (ImGui.BeginChild("##particle_effect_properties"u8, SysVec2.Zero, childFlags))
            {
                DrawContent();
            }
            ImGui.EndChild();
        }
    }

    private static void DrawContent()
    {
        if (ImGui.BeginTable("##particle_effect_properties_table"u8, 2, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("##particle_effect_property_label_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##particle_effect_property_value_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

            DrawAutoTriggerPropertyRow();
            DrawAutoTriggerFrequencyPropertyRow();

            ImGui.EndTable();
        }
    }

    private static void DrawAutoTriggerPropertyRow()
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.ParticleEffect_Property_AutoTrigger_Name);

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(SR.ParticleEffect_Property_AutoTrigger_Description);
        }

        ImGui.TableNextColumn();
        bool autoTrigger = EmberContext.ParticleEffect.AutoTrigger;
        if (ImGui.Checkbox("##particle_effect_auto_trigger"u8, ref autoTrigger))
        {
            EmberContext.ParticleEffect.AutoTrigger = autoTrigger;
            EmberContext.HasUnsavedChanges = true;
        }
    }

    private static void DrawAutoTriggerFrequencyPropertyRow()
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.ParticleEffect_Property_AutoTriggerFrequency_Name);

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(SR.ParticleEffect_Property_AutoTriggerFrequency_Description);
        }

        ImGui.TableNextColumn();
        ImGui.BeginDisabled(!EmberContext.ParticleEffect.AutoTrigger);
        ImGui.SetNextItemWidth(-1);
        float frequency = EmberContext.ParticleEffect.AutoTriggerFrequency;
        if (ImGui.DragFloat("##particle_effect_auto_trigger_frequency"u8, ref frequency, 0.1f, 0.1f, float.MaxValue, "%.2f"u8))
        {
            EmberContext.ParticleEffect.AutoTriggerFrequency = frequency;
            EmberContext.HasUnsavedChanges = true;
        }
        ImGui.EndDisabled();
    }
}
