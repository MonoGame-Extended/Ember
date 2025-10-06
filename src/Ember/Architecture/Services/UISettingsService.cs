using System;
using Hexa.NET.ImGui;
using static Hexa.NET.ImGui.ImGui;
using Microsoft.Xna.Framework;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages user interface settings including font scaling and colors.
/// </summary>
public sealed class UISettingsService : IUISettingsService
{
    private float _baseFontSize = 16.0f;
    private float _fontScaleMain = 1.0f;

    /// <summary>
    /// Gets or sets the base font size in pixels.
    /// </summary>
    /// <remarks>
    /// Value is clamped between 8.0 and 72.0 pixels. Automatically applies settings when changed.
    /// </remarks>
    public float BaseFontSize
    {
        get => _baseFontSize;
        set
        {
            if (Math.Abs(_baseFontSize - value) > 0.01f)
            {
                _baseFontSize = Math.Clamp(value, 8.0f, 72.0f);
                ApplyFontSettings();
            }
        }
    }

    /// <summary>
    /// Gets or sets the main font scale multiplier.
    /// </summary>
    /// <remarks>
    /// Value is clamped between 0.5 and 4.0. Automatically applies settings when changed.
    /// </remarks>
    public float FontScaleMain
    {
        get => _fontScaleMain;
        set
        {
            if (Math.Abs(_fontScaleMain - value) > 0.01f)
            {
                _fontScaleMain = Math.Clamp(value, 0.5f, 4.0f);
                ApplyFontSettings();
            }
        }
    }

    /// <summary>
    /// Gets the effective font size (base size multiplied by scale).
    /// </summary>
    public float EffectiveFontSize => _baseFontSize * _fontScaleMain;

    /// <summary>
    /// Gets or sets the clear color for the viewport.
    /// </summary>
    public Color ClearColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets the current color theme.
    /// </summary>
    public ColorTheme Theme { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UISettingsService"/> class.
    /// </summary>
    /// <param name="initialBaseFontSize">The initial base font size in pixels.</param>
    /// <param name="initialClearColor">The initial clear color for the viewport.</param>
    public UISettingsService(float initialBaseFontSize = 16.0f, Color? initialClearColor = null)
    {
        _baseFontSize = Math.Clamp(initialBaseFontSize, 8.0f, 72.0f);
        ClearColor = initialClearColor ?? Color.Black;
        ApplyFontSettings();
    }

    /// <summary>
    /// Applies the current font settings to ImGui.
    /// </summary>
    public void ApplyFontSettings()
    {
        ImGuiStylePtr stylePtr = GetStyle();

        stylePtr.FontSizeBase = _baseFontSize;
        stylePtr.FontScaleMain = _fontScaleMain;
        stylePtr.FontScaleDpi = 1.0f;

        // Temporary hack from ImGui demo until font atlas rebuilding is finalized
        stylePtr.NextFrameFontSizeBase = _baseFontSize;
    }

    /// <summary>
    /// Applies the given color theme to ImGui.
    /// </summary>
    /// <param name="theme">The theme to apply</param>
    public void ApplyTheme(ColorTheme theme)
    {
        // Themes are based on Catppuccin Latte (Light) and Frappe (Dark)
        // https://catppuccin.com/

        ImGuiStylePtr stylePtr = GetStyle();

        SysVec4 @base = theme == ColorTheme.Light ? new(0.937f, 0.945f, 0.961f, 1.0f) : new(0.192f, 0.200f, 0.271f, 1.0f);
        SysVec4 mantle = theme == ColorTheme.Light ? new(0.902f, 0.914f, 0.937f, 1.0f) : new(0.161f, 0.169f, 0.235f, 1.0f);
        SysVec4 crust = theme == ColorTheme.Light ? new(0.863f, 0.878f, 0.910f, 1.0f) : new(0.137f, 0.145f, 0.200f, 1.0f);
        SysVec4 surface0 = theme == ColorTheme.Light ? new(0.800f, 0.816f, 0.855f, 1.0f) : new(0.247f, 0.267f, 0.349f, 1.0f);
        SysVec4 surface1 = theme == ColorTheme.Light ? new(0.737f, 0.753f, 0.800f, 1.0f) : new(0.314f, 0.337f, 0.427f, 1.0f);
        SysVec4 surface2 = theme == ColorTheme.Light ? new(0.675f, 0.694f, 0.745f, 1.0f) : new(0.380f, 0.408f, 0.502f, 1.0f);
        SysVec4 overlay0 = theme == ColorTheme.Light ? new(0.612f, 0.627f, 0.690f, 1.0f) : new(0.447f, 0.478f, 0.576f, 1.0f);
        SysVec4 overlay1 = theme == ColorTheme.Light ? new(0.549f, 0.561f, 0.631f, 1.0f) : new(0.514f, 0.549f, 0.651f, 1.0f);
        SysVec4 overlay2 = theme == ColorTheme.Light ? new(0.486f, 0.498f, 0.576f, 1.0f) : new(0.580f, 0.616f, 0.725f, 1.0f);
        SysVec4 text = theme == ColorTheme.Light ? new(0.298f, 0.310f, 0.412f, 1.0f) : new(0.780f, 0.816f, 0.961f, 1.0f);
        SysVec4 subtext1 = theme == ColorTheme.Light ? new(0.361f, 0.373f, 0.467f, 1.0f) : new(0.714f, 0.753f, 0.894f, 1.0f);
        SysVec4 subtext0 = theme == ColorTheme.Light ? new(0.424f, 0.435f, 0.522f, 1.0f) : new(0.647f, 0.682f, 0.816f, 1.0f);
        SysVec4 rosewater = theme == ColorTheme.Light ? new(0.863f, 0.537f, 0.467f, 1.0f) : new(0.949f, 0.835f, 0.812f, 1.0f);
        SysVec4 blue = theme == ColorTheme.Light ? new(0.118f, 0.400f, 0.961f, 1.0f) : new(0.549f, 0.671f, 0.929f, 1.0f);
        SysVec4 lavender = theme == ColorTheme.Light ? new(0.451f, 0.529f, 0.992f, 1.0f) : new(0.729f, 0.729f, 0.949f, 1.0f);
        SysVec4 sapphire = theme == ColorTheme.Light ? new(0.129f, 0.624f, 0.710f, 1.0f) : new(0.518f, 0.761f, 0.863f, 1.0f);
        SysVec4 sky = theme == ColorTheme.Light ? new(0.016f, 0.647f, 0.898f, 1.0f) : new(0.600f, 0.812f, 0.863f, 1.0f);
        SysVec4 teal = theme == ColorTheme.Light ? new(0.094f, 0.573f, 0.600f, 1.0f) : new(0.510f, 0.780f, 0.753f, 1.0f);
        SysVec4 green = theme == ColorTheme.Light ? new(0.251f, 0.627f, 0.169f, 1.0f) : new(0.651f, 0.812f, 0.537f, 1.0f);
        SysVec4 yellow = theme == ColorTheme.Light ? new(0.875f, 0.557f, 0.114f, 1.0f) : new(0.898f, 0.784f, 0.561f, 1.0f);
        SysVec4 peach = theme == ColorTheme.Light ? new(0.996f, 0.392f, 0.043f, 1.0f) : new(0.937f, 0.624f, 0.463f, 1.0f);
        SysVec4 maroon = theme == ColorTheme.Light ? new(0.902f, 0.271f, 0.325f, 1.0f) : new(0.918f, 0.600f, 0.612f, 1.0f);
        SysVec4 red = theme == ColorTheme.Light ? new(0.820f, 0.059f, 0.224f, 1.0f) : new(0.906f, 0.514f, 0.518f, 1.0f);
        SysVec4 mauve = theme == ColorTheme.Light ? new(0.533f, 0.224f, 0.937f, 1.0f) : new(0.792f, 0.624f, 0.902f, 1.0f);
        SysVec4 pink = theme == ColorTheme.Light ? new(0.918f, 0.463f, 0.796f, 1.0f) : new(0.961f, 0.722f, 0.894f, 1.0f);

        // Selection color uses blue for light theme and mauve for dark themes with 30% opacity
        SysVec4 selection = (theme == ColorTheme.Light ? blue : mauve) with { Z = 0.3f };

        // Background hierarchy: Base (primary) -> Mantle (secondary) -> Surface0 (tertiary)
        stylePtr.Colors[(int)ImGuiCol.WindowBg] = @base;
        stylePtr.Colors[(int)ImGuiCol.ChildBg] = mantle;
        stylePtr.Colors[(int)ImGuiCol.PopupBg] = surface0;
        stylePtr.Colors[(int)ImGuiCol.MenuBarBg] = mantle;

        // Text hierarchy and selection
        stylePtr.Colors[(int)ImGuiCol.Text] = text;
        stylePtr.Colors[(int)ImGuiCol.TextDisabled] = subtext0;
        stylePtr.Colors[(int)ImGuiCol.TextSelectedBg] = selection;

        // Frame backgrounds need sufficient contrast from container backgrounds
        stylePtr.Colors[(int)ImGuiCol.FrameBg] = surface1;
        stylePtr.Colors[(int)ImGuiCol.FrameBgHovered] = surface2;
        stylePtr.Colors[(int)ImGuiCol.FrameBgActive] = overlay0;

        // Primary interactive elements use blue as the main accent color
        stylePtr.Colors[(int)ImGuiCol.Button] = blue with { Z = 0.3f };
        stylePtr.Colors[(int)ImGuiCol.ButtonHovered] = blue with { Z = 0.5f };
        stylePtr.Colors[(int)ImGuiCol.ButtonActive] = blue with { Z = 0.7f };

        // Secondary interactive elements use lavender for distinction
        stylePtr.Colors[(int)ImGuiCol.Header] = lavender with { Z = 0.3f };
        stylePtr.Colors[(int)ImGuiCol.HeaderHovered] = lavender with { Z = 0.5f };
        stylePtr.Colors[(int)ImGuiCol.HeaderActive] = lavender with { Z = 0.7f };

        // Semantic feedback colors
        stylePtr.Colors[(int)ImGuiCol.CheckMark] = green;

        // Title bar styling using the deepest background colors
        stylePtr.Colors[(int)ImGuiCol.TitleBg] = crust;
        stylePtr.Colors[(int)ImGuiCol.TitleBgActive] = mantle;
        stylePtr.Colors[(int)ImGuiCol.TitleBgCollapsed] = crust;

        // Borders and separators
        stylePtr.Colors[(int)ImGuiCol.Border] = overlay0;
        stylePtr.Colors[(int)ImGuiCol.BorderShadow] = @base with { Z = 0.0f };
        stylePtr.Colors[(int)ImGuiCol.Separator] = overlay0;
        stylePtr.Colors[(int)ImGuiCol.SeparatorHovered] = lavender;
        stylePtr.Colors[(int)ImGuiCol.SeparatorActive] = lavender;

        // Scrollbar components following the surface hierarchy
        stylePtr.Colors[(int)ImGuiCol.ScrollbarBg] = @base;
        stylePtr.Colors[(int)ImGuiCol.ScrollbarGrab] = surface0;
        stylePtr.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = surface1;
        stylePtr.Colors[(int)ImGuiCol.ScrollbarGrabActive] = surface2;

        // Sliders using primary accent colors
        stylePtr.Colors[(int)ImGuiCol.SliderGrab] = blue;
        stylePtr.Colors[(int)ImGuiCol.SliderGrabActive] = sapphire;

        // Tab styling using lavender as the accent color for consistency with headers
        stylePtr.Colors[(int)ImGuiCol.Tab] = surface0;
        stylePtr.Colors[(int)ImGuiCol.TabHovered] = lavender with { Z = 0.4f };
        stylePtr.Colors[(int)ImGuiCol.TabSelected] = lavender with { Z = 0.6f };
        stylePtr.Colors[(int)ImGuiCol.TabSelectedOverline] = lavender;
        stylePtr.Colors[(int)ImGuiCol.TabDimmed] = surface0;
        stylePtr.Colors[(int)ImGuiCol.TabDimmedSelected] = lavender with { Z = 0.3f };
        stylePtr.Colors[(int)ImGuiCol.TabDimmedSelectedOverline] = lavender with { Z = 0.6f };

        // Resize grips using overlay colors with blue accent when active
        stylePtr.Colors[(int)ImGuiCol.ResizeGrip] = overlay2 with { Z = 0.5f };
        stylePtr.Colors[(int)ImGuiCol.ResizeGripHovered] = blue with { Z = 0.7f };
        stylePtr.Colors[(int)ImGuiCol.ResizeGripActive] = blue with { Z = 0.9f };

        // Plot colors using complementary theme colors
        stylePtr.Colors[(int)ImGuiCol.PlotLines] = blue;
        stylePtr.Colors[(int)ImGuiCol.PlotLinesHovered] = sky;
        stylePtr.Colors[(int)ImGuiCol.PlotHistogram] = teal;
        stylePtr.Colors[(int)ImGuiCol.PlotHistogramHovered] = green;

        // Table styling following background and overlay hierarchy
        stylePtr.Colors[(int)ImGuiCol.TableHeaderBg] = surface0;
        stylePtr.Colors[(int)ImGuiCol.TableBorderStrong] = overlay1;
        stylePtr.Colors[(int)ImGuiCol.TableBorderLight] = overlay0;
        stylePtr.Colors[(int)ImGuiCol.TableRowBg] = @base with { Z = 0.0f };
        stylePtr.Colors[(int)ImGuiCol.TableRowBgAlt] = surface0 with { Z = 0.3f };

        // Docking system colors
        stylePtr.Colors[(int)ImGuiCol.DockingPreview] = blue with { Z = 0.7f };
        stylePtr.Colors[(int)ImGuiCol.DockingEmptyBg] = @base;

        // Drag and drop visual feedback
        stylePtr.Colors[(int)ImGuiCol.DragDropTarget] = yellow with { Z = 0.9f };

        // Navigation and modal overlay colors
        stylePtr.Colors[(int)ImGuiCol.NavWindowingHighlight] = text with { Z = 0.7f };
        stylePtr.Colors[(int)ImGuiCol.NavWindowingDimBg] = @base with { Z = 0.2f };
        stylePtr.Colors[(int)ImGuiCol.ModalWindowDimBg] = @base with { Z = 0.35f };
    }
}
