using System;
using Hexa.NET.ImGui;
using static Hexa.NET.ImGui.ImGui;

namespace Ember.Architecture.Components;

public static class PropertyTable
{
    public static bool BeginPropertyTable(ReadOnlySpan<byte> id)
    {
        if (BeginTable(id, columns: 2, ImGuiTableFlags.SizingFixedSame | ImGuiTableFlags.BordersOuter))
        {
            TableSetupColumn("##property-label"u8, ImGuiTableColumnFlags.WidthFixed, -1, 0);
            TableSetupColumn("##property-value"u8, ImGuiTableColumnFlags.WidthStretch, -1, 1);
            return true;
        }

        return false;
    }

    public static bool TextProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref string value)
    {
        bool changed = false;

        PushID(label);
        Label(label, tooltip);
        TableNextColumn();
        SetNextItemWidth(-1);
        if (InputText("##value"u8, ref value, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.EscapeClearsAll))
        {
            changed = true;
        }

        PopID();
        return changed;
    }

    public static bool FloatProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref float value, float step, float min, float max)
    {
        bool changed = false;

        PushID(label);
        Label(label, tooltip);
        TableNextColumn();
        SetNextItemWidth(-1);
        if (DragFloat("##value"u8, ref value, step, min, max, "%.2f"u8))
        {
            changed = true;
        }

        PopID();
        return changed;
    }

    private static void Label(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip)
    {
        TableNextColumn();
        AlignTextToFramePadding();
        Text(label);
        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            SetTooltip(tooltip);
        }
    }

    public static void EndPropertyTable() => EndTable();
}

