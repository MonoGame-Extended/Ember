using Microsoft.Xna.Framework;

namespace Ember.Architecture.Services;

/// <summary>
/// Manages user interface settings including font scaling and colors.
/// </summary>
public interface IUISettingsService
{
    /// <summary>
    /// Gets or sets the base font size in pixels.
    /// </summary>
    /// <remarks>
    /// Value is clamped between 8.0 and 72.0 pixels.
    /// </remarks>
    float BaseFontSize { get; set; }

    /// <summary>
    /// Gets or sets the main font scale multiplier.
    /// </summary>
    /// <remarks>
    /// Value is clamped between 0.5 and 4.0.
    /// </remarks>
    float FontScaleMain { get; set; }

    /// <summary>
    /// Gets the effective font size (base size multiplied by scale).
    /// </summary>
    float EffectiveFontSize { get; }

    /// <summary>
    /// Gets or sets the clear color for the viewport.
    /// </summary>
    Color ClearColor { get; set; }

    /// <summary>
    /// Gets the current color theme.
    /// </summary>
    ColorTheme Theme { get; }

    /// <summary>
    /// Applies the current font settings to ImGui.
    /// </summary>
    void ApplyFontSettings();

    /// <summary>
    /// Applies the given color theme to ImGui.
    /// </summary>
    /// <param name="theme">The theme to apply</param>
    void ApplyTheme(ColorTheme theme);
}
