using System;
using System.IO;
using System.Text;
using Ember.Graphics;
using Hexa.NET.ImGui;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Particles.Data;
using static Hexa.NET.ImGui.ImGui;

namespace Ember.Architecture.Components;

public static class PropertyTable
{
    public static bool BeginPropertyTable(ReadOnlySpan<byte> id)
    {
        if (BeginTable(id, columns: 2, ImGuiTableFlags.SizingFixedSame))
        {
            TableSetupColumn("##property-label"u8, ImGuiTableColumnFlags.WidthFixed, -1, 0);
            TableSetupColumn("##property-value"u8, ImGuiTableColumnFlags.WidthStretch, -1, 1);
            return true;
        }

        return false;
    }

    public static void EndPropertyTable() => EndTable();

    public static bool BeginReleaseParameterPropertyTable(ReadOnlySpan<byte> id)
    {
        if (BeginTable(id, columns: 3, ImGuiTableFlags.SizingFixedSame))
        {
            TableSetupColumn("##property-label"u8, ImGuiTableColumnFlags.WidthFixed, -1, 0);
            TableSetupColumn("##property-kind"u8, ImGuiTableColumnFlags.WidthStretch, -1, 1);
            TableSetupColumn("##property-value"u8, ImGuiTableColumnFlags.WidthStretch, -1, 2);

            return true;
        }

        return false;
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

    public static bool InputTextProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref string value)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
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

    public static bool CheckboxProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref bool value)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);
        TableNextColumn();
        SetNextItemWidth(-1);
        if (Checkbox("##value", ref value))
        {
            changed = true;
        }
        PopID();
        return changed;
    }

    public static bool DragIntProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref int value, int step, int min, int max)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);
        TableNextColumn();
        SetNextItemWidth(-1);
        if (DragInt("##value"u8, ref value, step, min, max))
        {
            changed = true;
        }

        PopID();
        return changed;
    }

    public static bool InputIntProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref int value)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);
        TableNextColumn();
        SetNextItemWidth(-1);
        if (InputInt("##value"u8, ref value, 0, 0))
        {
            changed = true;
        }

        PopID();
        return changed;
    }

    public static bool InputRectProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref XnaRect value)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);
        TableNextColumn();
        SetNextItemWidth(-1);
        int[] components = [value.X, value.Y, value.Width, value.Height];
        if (InputInt4("##value"u8, ref components[0]))
        {
            value.X = components[0];
            value.Y = components[1];
            value.Width = components[2];
            value.Height = components[3];
            changed = true;
        }

        PopID();
        return changed;

    }

    public static bool DragFloatProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref float value, float step, float min, float max)
    {
        bool changed = false;

        TableNextRow();
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

    public static bool DragVector2Property(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref XnaVec2 value, float step, float min, float max)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);
        TableNextColumn();

        ImGuiStylePtr stylePtr = GetStyle();
        float availWidth = GetContentRegionAvail().X;
        float itemSpacingWidth = stylePtr.ItemSpacing.X;
        float dragWidth = (availWidth - itemSpacingWidth) * 0.5f;

        SetNextItemWidth(dragWidth);
        if (DragFloat("##value-x"u8, ref value.X, step, min, max, "X: %.2F"))
        {
            changed = true;
        }

        SameLine();
        SetNextItemWidth(dragWidth);
        if (DragFloat("##value-y"u8, ref value.Y, step, min, max, "Y: %.2F"))
        {
            changed = true;
        }

        PopID();
        return changed;
    }

    public static bool Color3Property(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref HslColor value)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);
        TableNextColumn();

        XnaColor rgbColor = HslColor.ToRgb(value);
        SysVec4 color = new(rgbColor.R / 255.0f, rgbColor.G / 255.0f, rgbColor.B / 255.0f, 1.0f);

        float availWidth = GetContentRegionAvail().X;
        SysVec2 buttonSize = new(availWidth, GetFrameHeight());

        if (ColorButton("##color-button"u8, color, ImGuiColorEditFlags.None, buttonSize))
        {
            OpenPopup("##color-picker");
        }

        if (BeginPopup("##color-picker"))
        {
            SysVec3 rgb = new SysVec3(color.X, color.Y, color.Z);
            if (ColorPicker3("##value", ref rgb))
            {
                XnaColor newRgb = new XnaColor(rgb.X, rgb.Y, rgb.Z);
                value = HslColor.FromRgb(newRgb);
                changed = true;
            }

            EndPopup();
        }

        PopID();
        return changed;
    }

    public static bool TextureProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, Texture2DRegion region)
    {
        bool clicked = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);
        TableNextColumn();

        if (region != null)
        {
            string texturePath = !string.IsNullOrEmpty(region.Texture.Name)
                                  ? region.Texture.Name
                                  : region.Name;

            string displayName = !string.IsNullOrEmpty(texturePath)
                                  ? Path.GetFileName(texturePath)
                                  : "Texture";

            if (Button(Encoding.UTF8.GetBytes(displayName), -SysVec2.UnitX))
            {
                clicked = true;
            }

            if (IsItemHovered())
            {
                if (BeginTooltip())
                {
                    float maxPreviewSize = 256.0f;
                    float textureWidth = region.Texture.Width;
                    float textureHeight = region.Texture.Height;
                    float aspectRatio = textureWidth / textureHeight;

                    SysVec2 previewSize;
                    if (aspectRatio > 1.0f)
                    {
                        previewSize = new SysVec2(maxPreviewSize, maxPreviewSize / aspectRatio);
                    }
                    else
                    {
                        previewSize = new SysVec2(maxPreviewSize * aspectRatio, maxPreviewSize);
                    }

                    ImTextureRef textureRef = ImGuiRenderer.BindTexture(region.Texture);
                    SysVec2 uv0 = new SysVec2()
                    {
                        X = (float)region.Bounds.Left / region.Texture.Width,
                        Y = (float)region.Bounds.Top / region.Texture.Height
                    };
                    SysVec2 uv1 = new SysVec2()
                    {
                        X = (float)region.Bounds.Right / region.Texture.Width,
                        Y = (float)region.Bounds.Bottom / region.Texture.Height
                    };

                    Image(textureRef, previewSize, uv0, uv1);

                    if (!string.IsNullOrEmpty(texturePath))
                    {
                        TextDisabled(texturePath);
                    }

                    EndTooltip();
                }
            }
        }
        else
        {
            if (Button("Select Texture"u8, -SysVec2.UnitX))
            {
                clicked = true;
            }
        }

        PopID();
        return clicked;
    }

    public static bool ButtonProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip)
    {
        bool clicked = false;

        PushID(label);

        TableNextRow();
        TableNextColumn();
        TableNextColumn();

        if (Button(label, -SysVec2.UnitX))
        {
            clicked = true;
        }
        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            SetTooltip(tooltip);
        }

        PopID();
        return clicked;

    }

    public static bool BeginComboProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ReadOnlySpan<byte> preview)
    {
        PushID(label);
        TableNextRow();
        Label(label, tooltip);
        TableNextColumn();
        SetNextItemWidth(-1);
        bool opened = BeginCombo("##value"u8, preview);
        if (!opened)
        {
            PopID();
        }
        return opened;
    }

    public static bool ComboItem(ReadOnlySpan<byte> itemLabel, ReadOnlySpan<byte> itemToolTip, bool isSelected)
    {
        bool clicked = Selectable(itemLabel, isSelected);

        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            SetTooltip(itemToolTip);
        }

        if (isSelected)
        {
            SetItemDefaultFocus();
        }

        return clicked;
    }

    public static void EndComboProperty()
    {
        EndCombo();
        PopID();
    }

    public static bool ReleaseParameter(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref ParticleInt32Parameter value)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);

        if (ReleaseParameterKind(ref value.Kind))
        {
            changed = true;
        }

        if (value.Kind == ParticleValueKind.Constant)
        {
            if (ReleaseParameterConstantInt(ref value.Constant))
            {
                changed = true;
            }
        }
        else if (value.Kind == ParticleValueKind.Random)
        {
            if (ReleaseParameterRandomInt(ref value.RandomMin, ref value.RandomMax))
            {
                changed = true;
            }
        }

        PopID();

        return changed;
    }

    public static bool ReleaseParameter(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref ParticleFloatParameter value)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);

        if (ReleaseParameterKind(ref value.Kind))
        {
            changed = true;
        }

        if (value.Kind == ParticleValueKind.Constant)
        {
            if (ReleaseParameterConstantFloat(ref value.Constant))
            {
                changed = true;
            }
        }
        else if (value.Kind == ParticleValueKind.Random)
        {
            if (ReleaseParameterRandomFloat(ref value.RandomMin, ref value.RandomMax))
            {
                changed = true;
            }
        }

        PopID();

        return changed;
    }

    public static bool ReleaseParameter(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref ParticleVector2Parameter value)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);

        if (ReleaseParameterKind(ref value.Kind))
        {
            changed = true;
        }

        if (value.Kind == ParticleValueKind.Constant)
        {
            if (ReleaseParameterConstantVector2(ref value.Constant))
            {
                changed = true;
            }
        }
        else if (value.Kind == ParticleValueKind.Random)
        {
            if (ReleaseParameterRandomVector2(ref value.RandomMin, ref value.RandomMax))
            {
                changed = true;
            }
        }

        PopID();

        return changed;
    }

    public static bool ReleaseParameter(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ref ParticleColorParameter value)
    {
        bool changed = false;

        PushID(label);

        TableNextRow();
        Label(label, tooltip);

        if (ReleaseParameterKind(ref value.Kind))
        {
            changed = true;
        }

        if (value.Kind == ParticleValueKind.Constant)
        {
            if (ReleaseParameterConstantColor(ref value.Constant))
            {
                changed = true;
            }
        }
        else if (value.Kind == ParticleValueKind.Random)
        {
            if (ReleaseParameterRandomColor(ref value.RandomMin, ref value.RandomMax))
            {
                changed = true;
            }
        }

        PopID();

        return changed;
    }

    private static bool ReleaseParameterKind(ref ParticleValueKind value)
    {
        bool changed = false;

        ReadOnlySpan<byte> kindPreview = value switch
        {
            ParticleValueKind.Constant => "Constant"u8,
            ParticleValueKind.Random => "Random"u8,
            _ => throw new InvalidOperationException($"Unknown particle value kind '{value}'")
        };

        TableNextColumn();
        SetNextItemWidth(-1);
        if (BeginCombo("##kind", kindPreview))
        {
            if (ComboItem("Constant"u8, "All particles will be released with the same value for this property"u8, value == ParticleValueKind.Constant))
            {
                value = ParticleValueKind.Constant;
                changed = true;
            }

            if (ComboItem("Random"u8, "Each particle will be released with a unique random value within the defined minimum and maximum bounds"u8, value == ParticleValueKind.Random))
            {
                value = ParticleValueKind.Random;
                changed = true;
            }

            EndCombo();
        }

        return changed;
    }

    private static bool ReleaseParameterConstantInt(ref int value)
    {
        TableNextColumn();
        SetNextItemWidth(-1);
        return DragInt("##constant"u8, ref value, 1, 0, int.MaxValue);
    }

    private static bool ReleaseParameterRandomInt(ref int min, ref int max)
    {
        bool changed = false;

        ImGuiStylePtr stylePtr = GetStyle();

        TableNextColumn();

        float availWidth = GetContentRegionAvail().X;
        float toWidth = CalcTextSize(" to "u8).X;
        float spacing = stylePtr.ItemSpacing.X * 2.0f;
        float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

        SetNextItemWidth(dragWidth);
        if (DragInt("##random-min"u8, ref min, 1, 0, max))
        {
            changed = true;
        }

        SameLine();
        Text(" to "u8);

        SameLine();
        SetNextItemWidth(dragWidth);
        if (DragInt("##random-max"u8, ref max, 1, min, int.MaxValue))
        {
            changed = true;
        }

        return changed;
    }

    private static bool ReleaseParameterConstantFloat(ref float value)
    {
        TableNextColumn();
        SetNextItemWidth(-1);
        return DragFloat("##constant"u8, ref value, 0.1f, 0.0f, float.MaxValue, "%.2f"u8);
    }

    private static bool ReleaseParameterRandomFloat(ref float min, ref float max)
    {
        bool changed = false;

        ImGuiStylePtr stylePtr = GetStyle();
        TableNextColumn();

        float availWidth = GetContentRegionAvail().X;
        float toWidth = CalcTextSize(" to "u8).X;
        float spacing = stylePtr.ItemSpacing.X * 2.0f;
        float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

        SetNextItemWidth(dragWidth);
        if (DragFloat("##random-min"u8, ref min, 0.1f, 0.0f, max, "%.2f"u8))
        {
            changed = true;
        }

        SameLine();
        Text(" to "u8);

        SameLine();
        SetNextItemWidth(dragWidth);
        if (DragFloat("##random-max"u8, ref max, 0.1f, min, float.MaxValue, "%.2f"u8))
        {
            changed = true;
        }

        return changed;
    }

    private static bool ReleaseParameterConstantVector2(ref XnaVec2 value)
    {
        bool changed = false;

        ImGuiStylePtr stylePtr = GetStyle();
        TableNextColumn();

        float availWidth = GetContentRegionAvail().X;
        float spacing = stylePtr.ItemSpacing.X;
        float dragWidth = (availWidth - spacing) * 0.5f;

        SetNextItemWidth(dragWidth);
        float x = value.X;
        if (DragFloat("##constant-x"u8, ref x, 0.1f, 0.0f, float.MaxValue, "X: %.2f"u8))
        {
            value.X = x;
            changed = true;
        }

        SameLine();
        SetNextItemWidth(dragWidth);
        float y = value.Y;
        if (DragFloat("##constant-y"u8, ref y, 0.1f, 0.0f, float.MaxValue, "Y: %.2f"u8))
        {
            value.Y = y;
            changed = true;
        }

        return changed;
    }

    private static bool ReleaseParameterRandomVector2(ref XnaVec2 min, ref XnaVec2 max)
    {
        bool changed = false;

        ImGuiStylePtr stylePtr = GetStyle();
        TableNextColumn();

        float availWidth = GetContentRegionAvail().X;
        float toWidth = CalcTextSize(" to "u8).X;
        float spacing = stylePtr.ItemSpacing.X * 2.0f;
        float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

        SetNextItemWidth(dragWidth);
        float minX = min.X;
        if (DragFloat("##min-x"u8, ref minX, 0.1f, 0.0f, max.X, "X: %.2f"u8))
        {
            min.X = minX;
            changed = true;
        }

        SameLine();
        Text(" to "u8);

        SameLine();
        SetNextItemWidth(dragWidth);
        float maxX = max.X;
        if (DragFloat("##max-x"u8, ref maxX, 0.1f, min.X, float.MaxValue, "X: %.2f"u8))
        {
            max.X = maxX;
            changed = true;
        }

        TableNextRow();
        TableNextColumn();
        TableNextColumn();
        TableNextColumn();

        SetNextItemWidth(dragWidth);
        float minY = min.Y;
        if (DragFloat("##min-y"u8, ref minY, 0.1f, 0.0f, max.Y, "X: %.2f"u8))
        {
            min.Y = minY;
            changed = true;
        }

        SameLine();
        Text(" to "u8);

        SameLine();
        SetNextItemWidth(dragWidth);
        float maxY = max.Y;
        if (DragFloat("##max-y"u8, ref maxY, 0.1f, min.Y, float.MaxValue, "Y: %.2f"u8))
        {
            max.Y = maxY;
            changed = true;
        }

        return changed;
    }

    private static bool ReleaseParameterConstantColor(ref XnaVec3 value)
    {
        bool changed = false;

        TableNextColumn();

        HslColor valueHsl = new HslColor(value.X, value.Y, value.Z);
        XnaColor valueRgb = HslColor.ToRgb(valueHsl);
        SysVec4 valueColor = new SysVec4(valueRgb.R / 255.0f, valueRgb.G / 255.0f, valueRgb.B / 255.0f, 1.0f);

        float availWidth = GetContentRegionAvail().X;
        SysVec2 buttonSize = new SysVec2(availWidth, GetFrameHeight());

        if (ColorButton("##constant-button"u8, valueColor, ImGuiColorEditFlags.AlphaBar, buttonSize))
        {
            OpenPopup("##constant-color-picker"u8);
        }

        if (BeginPopup("##constant-color-picker"u8))
        {
            float[] rgb = [valueColor.X, valueColor.Y, valueColor.Z];
            if (ColorPicker3("##constant-value"u8, rgb))
            {
                XnaColor newValueRgb = new XnaColor(rgb[0], rgb[1], rgb[2]);
                HslColor newValueHsl = HslColor.FromRgb(newValueRgb);
                value = new XnaVec3(newValueHsl.H, newValueHsl.S, newValueHsl.L);
                changed = true;
            }

            EndPopup();
        }

        return changed;

    }

    private static bool ReleaseParameterRandomColor(ref XnaVec3 min, ref XnaVec3 max)
    {
        bool changed = false;

        TableNextColumn();

        ImGuiStylePtr stylePtr = GetStyle();
        float availableWidth = GetContentRegionAvail().X;
        float toWidth = CalcTextSize(" to "u8).X;
        float spacing = stylePtr.ItemSpacing.X * 2.0f;
        float buttonWidth = (availableWidth - toWidth - spacing) * 0.5f;
        SysVec2 buttonSize = new SysVec2(buttonWidth, GetFrameHeight());

        HslColor randomMinHsl = new HslColor(min.X, min.Y, min.Z);
        XnaColor randomMinRgb = HslColor.ToRgb(randomMinHsl);
        SysVec4 randomMinColor = new SysVec4(randomMinRgb.R / 255.0f, randomMinRgb.G / 255.0f, randomMinRgb.B / 255.0f, 1.0f);

        if (ColorButton("##random-min-button"u8, randomMinColor, ImGuiColorEditFlags.AlphaBar, buttonSize))
        {
            OpenPopup("##random-min-color-picker"u8);
        }

        if (BeginPopup("##random-min-color-picker"u8))
        {
            float[] rgb = [randomMinColor.X, randomMinColor.Y, randomMinColor.Z];
            if (ColorPicker3("##random-min-value"u8, rgb))
            {
                XnaColor newValueRgb = new XnaColor(rgb[0], rgb[1], rgb[2]);
                HslColor newValueHsl = HslColor.FromRgb(newValueRgb);
                min = new XnaVec3(newValueHsl.H, newValueHsl.S, newValueHsl.L);
                changed = true;
            }

            EndPopup();
        }

        SameLine();
        Text(" to "u8);

        HslColor randomMaxHsl = new HslColor(max.X, max.Y, max.Z);
        XnaColor randomMaxRgb = HslColor.ToRgb(randomMaxHsl);
        SysVec4 randomMaxColor = new SysVec4(randomMaxRgb.R / 255.0f, randomMaxRgb.G / 255.0f, randomMaxRgb.B / 255.0f, 1.0f);
        SameLine();

        if (ColorButton("##random-max-button"u8, randomMaxColor, ImGuiColorEditFlags.AlphaBar, buttonSize))
        {
            OpenPopup("##random-max-color-picker"u8);
        }

        if (BeginPopup("##random-max-color-picker"u8))
        {
            float[] rgb = [randomMaxColor.X, randomMaxColor.Y, randomMaxColor.Z];
            if (ColorPicker3("##random-max-value"u8, rgb))
            {
                XnaColor newValueRgb = new XnaColor(rgb[0], rgb[1], rgb[2]);
                HslColor newValueHsl = HslColor.FromRgb(newValueRgb);
                max = new XnaVec3(newValueHsl.H, newValueHsl.S, newValueHsl.L);
                changed = true;
            }

            EndPopup();
        }

        return changed;
    }
}

