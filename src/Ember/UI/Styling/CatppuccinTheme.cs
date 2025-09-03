// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using Hexa.NET.ImGui;

namespace Ember.UI.Styling;

/// <summary>
/// Simplified Catppuccin theming for ImGui applications using semantic color approach.
/// </summary>
public static class CatppuccinTheme
{
    private static CatppuccinVariant _currentVariant = CatppuccinVariant.Mocha;
    private static CatppuccinPalette _currentPalette;

    static CatppuccinTheme()
    {
        _currentPalette = CatppuccinPalette.GetPalette(CatppuccinVariant.Mocha);
    }

    /// <summary>
    /// Gets the currently active theme variant.
    /// </summary>
    public static CatppuccinVariant CurrentVariant => _currentVariant;

    /// <summary>
    /// Gets the current palette based on the active theme variant.
    /// </summary>
    public static CatppuccinPalette CurrentPalette => _currentPalette;

    /// <summary>
    /// Applies the specified Catppuccin theme variant to ImGui.
    /// </summary>
    /// <param name="variant">The Catppuccin variant to apply.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid variant is specified.</exception>
    public static void Apply(CatppuccinVariant variant = CatppuccinVariant.Mocha)
    {
        if (!Enum.IsDefined(typeof(CatppuccinVariant), variant))
        {
            throw new ArgumentOutOfRangeException(nameof(variant), variant, "Invalid Catppuccin variant specified.");
        }

        _currentVariant = variant;
        _currentPalette = CatppuccinPalette.GetPalette(variant);

        var style = ImGui.GetStyle();
        ApplyStyleProperties(style);
        ApplySemanticColors(style, _currentPalette);
    }

    /// <summary>
    /// Configures ImGui style properties for optimal appearance with Catppuccin themes.
    /// </summary>
    /// <param name="style">The ImGui style to configure.</param>
    private static void ApplyStyleProperties(ImGuiStylePtr style)
    {
        // Rounded corners for modern appearance
        style.WindowRounding = 8.0f;
        style.ChildRounding = 6.0f;
        style.FrameRounding = 6.0f;
        style.PopupRounding = 6.0f;
        style.ScrollbarRounding = 6.0f;
        style.GrabRounding = 6.0f;
        style.TabRounding = 4.0f;

        // Spacing and padding optimized for readability
        style.WindowPadding = new Vector2(12.0f, 12.0f);
        style.FramePadding = new Vector2(8.0f, 8.0f);
        style.ItemSpacing = new Vector2(8.0f, 8.0f);
        style.ItemInnerSpacing = new Vector2(6.0f, 6.0f);

        // Layout dimensions
        style.IndentSpacing = 24.0f;
        style.ScrollbarSize = 16.0f;
        style.GrabMinSize = 12.0f;

        // Borders
        style.WindowBorderSize = 1.0f;
        style.ChildBorderSize = 1.0f;
        style.PopupBorderSize = 1.0f;
        style.FrameBorderSize = 1.0f;
        style.TabBorderSize = 0.0f;

        // Rendering quality
        style.AntiAliasedLines = true;
        style.AntiAliasedFill = true;

        // Alignment preferences
        style.WindowTitleAlign = new Vector2(0.5f, 0.5f);

        style.FontSizeBase = 16.0f;
    }

    /// <summary>
    /// Applies semantic color mapping to ImGui elements based on the Catppuccin design system.
    /// </summary>
    /// <param name="style">The ImGui style to apply colors to.</param>
    /// <param name="palette">The color palette containing theme-specific color values.</param>
    private static void ApplySemanticColors(ImGuiStylePtr style, CatppuccinPalette palette)
    {
        // Background hierarchy: Base (primary) -> Mantle (secondary) -> Surface0 (tertiary)
        style.Colors[(int)ImGuiCol.WindowBg] = palette.Base;
        style.Colors[(int)ImGuiCol.ChildBg] = palette.Mantle;
        style.Colors[(int)ImGuiCol.PopupBg] = palette.Surface0;
        style.Colors[(int)ImGuiCol.MenuBarBg] = palette.Mantle;

        // Text hierarchy and selection
        style.Colors[(int)ImGuiCol.Text] = palette.Text;
        style.Colors[(int)ImGuiCol.TextDisabled] = palette.Subtext0;

        // Selection colors matching official website implementation:
        // Latte uses Blue, dark themes use Mauve - with 30% opacity (70% transparent)
        Vector4 selectionColor = _currentVariant == CatppuccinVariant.Latte ? palette.Blue : palette.Mauve;
        style.Colors[(int)ImGuiCol.TextSelectedBg] = ColorUtils.WithOpacity(selectionColor, 0.3f);

        // Interactive elements
        // Frame backgrounds need sufficient contrast from container backgrounds
        style.Colors[(int)ImGuiCol.FrameBg] = palette.Surface1;
        style.Colors[(int)ImGuiCol.FrameBgHovered] = palette.Surface2;
        style.Colors[(int)ImGuiCol.FrameBgActive] = palette.Overlay0;

        // Primary interactive elements use Blue as the main accent color
        style.Colors[(int)ImGuiCol.Button] = ColorUtils.WithOpacity(palette.Blue, 0.3f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = ColorUtils.WithOpacity(palette.Blue, 0.5f);
        style.Colors[(int)ImGuiCol.ButtonActive] = ColorUtils.WithOpacity(palette.Blue, 0.7f);

        // Secondary interactive elements use Lavender for distinction
        style.Colors[(int)ImGuiCol.Header] = ColorUtils.WithOpacity(palette.Lavender, 0.3f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = ColorUtils.WithOpacity(palette.Lavender, 0.5f);
        style.Colors[(int)ImGuiCol.HeaderActive] = ColorUtils.WithOpacity(palette.Lavender, 0.7f);

        // Semantic feedback colors
        style.Colors[(int)ImGuiCol.CheckMark] = palette.Green;

        // Title bar styling using the deepest background colors
        style.Colors[(int)ImGuiCol.TitleBg] = palette.Crust;
        style.Colors[(int)ImGuiCol.TitleBgActive] = palette.Mantle;
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = palette.Crust;

        // Borders and separators
        style.Colors[(int)ImGuiCol.Border] = palette.Overlay0;
        style.Colors[(int)ImGuiCol.BorderShadow] = ColorUtils.WithOpacity(palette.Base, 0.0f);
        style.Colors[(int)ImGuiCol.Separator] = palette.Overlay0;
        style.Colors[(int)ImGuiCol.SeparatorHovered] = palette.Lavender;
        style.Colors[(int)ImGuiCol.SeparatorActive] = palette.Lavender;

        // Scrollbar components following the surface hierarchy
        style.Colors[(int)ImGuiCol.ScrollbarBg] = palette.Base;
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = palette.Surface0;
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = palette.Surface1;
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = palette.Surface2;

        // Sliders using primary accent colors
        style.Colors[(int)ImGuiCol.SliderGrab] = palette.Blue;
        style.Colors[(int)ImGuiCol.SliderGrabActive] = palette.Sapphire;

        // Tab styling using Lavender as the accent color for consistency with headers
        style.Colors[(int)ImGuiCol.Tab] = palette.Surface0;
        style.Colors[(int)ImGuiCol.TabHovered] = ColorUtils.WithOpacity(palette.Lavender, 0.4f);
        style.Colors[(int)ImGuiCol.TabSelected] = ColorUtils.WithOpacity(palette.Lavender, 0.6f);
        style.Colors[(int)ImGuiCol.TabSelectedOverline] = palette.Lavender;
        style.Colors[(int)ImGuiCol.TabDimmed] = palette.Surface0;
        style.Colors[(int)ImGuiCol.TabDimmedSelected] = ColorUtils.WithOpacity(palette.Lavender, 0.3f);
        style.Colors[(int)ImGuiCol.TabDimmedSelectedOverline] = ColorUtils.WithOpacity(palette.Lavender, 0.6f);

        // Resize grips using overlay colors with Blue accent when active
        style.Colors[(int)ImGuiCol.ResizeGrip] = ColorUtils.WithOpacity(palette.Overlay2, 0.5f);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = ColorUtils.WithOpacity(palette.Blue, 0.7f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = ColorUtils.WithOpacity(palette.Blue, 0.9f);

        // Plot colors using complementary theme colors
        style.Colors[(int)ImGuiCol.PlotLines] = palette.Blue;
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = palette.Sky;
        style.Colors[(int)ImGuiCol.PlotHistogram] = palette.Teal;
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = palette.Green;

        // Table styling following background and overlay hierarchy
        style.Colors[(int)ImGuiCol.TableHeaderBg] = palette.Surface0;
        style.Colors[(int)ImGuiCol.TableBorderStrong] = palette.Overlay1;
        style.Colors[(int)ImGuiCol.TableBorderLight] = palette.Overlay0;
        style.Colors[(int)ImGuiCol.TableRowBg] = ColorUtils.WithOpacity(palette.Base, 0.0f);
        style.Colors[(int)ImGuiCol.TableRowBgAlt] = ColorUtils.WithOpacity(palette.Surface0, 0.3f);

        // Docking system colors
        style.Colors[(int)ImGuiCol.DockingPreview] = ColorUtils.WithOpacity(palette.Blue, 0.7f);
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = palette.Base;

        // Drag and drop visual feedback
        style.Colors[(int)ImGuiCol.DragDropTarget] = ColorUtils.WithOpacity(palette.Yellow, 0.9f);

        // Navigation and modal overlay colors
        style.Colors[(int)ImGuiCol.NavWindowingHighlight] = ColorUtils.WithOpacity(palette.Text, 0.7f);
        style.Colors[(int)ImGuiCol.NavWindowingDimBg] = ColorUtils.WithOpacity(palette.Base, 0.2f);
        style.Colors[(int)ImGuiCol.ModalWindowDimBg] = ColorUtils.WithOpacity(palette.Base, 0.35f);
    }
}
