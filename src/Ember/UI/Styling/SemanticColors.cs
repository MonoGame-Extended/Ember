// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Ember.UI.Styling;

/// <summary>
/// Provides semantic color access for the current Catppuccin theme.
/// </summary>
public static class SemanticColors
{
    /// <summary>
    /// Gets the current color palette.
    /// </summary>
    private static CatppuccinPalette CurrentPalette => CatppuccinTheme.CurrentPalette;

    /// <summary>
    /// Colors for indicating successful operations, confirmations, and positive states.
    /// </summary>
    public static class Success
    {
        /// <summary>Primary success color for main success indicators such as checkmarks and confirmation messages.</summary>
        public static SysVec4 Primary => CurrentPalette.Green;

        /// <summary>Subtle success color for background highlights and low-emphasis success states.</summary>
        public static SysVec4 Background => ColorUtils.WithOpacity(CurrentPalette.Green, 0.15f);

        /// <summary>Success color for borders and outlines around success-related UI elements.</summary>
        public static SysVec4 Border => ColorUtils.WithOpacity(CurrentPalette.Green, 0.4f);
    }

    /// <summary>
    /// Colors for indicating warnings, cautions, and states requiring user attention.
    /// </summary>
    public static class Warning
    {
        /// <summary>Primary warning color for main warning indicators such as alert icons and caution messages.</summary>
        public static SysVec4 Primary => CurrentPalette.Yellow;

        /// <summary>Subtle warning color for background highlights and low-emphasis warning states.</summary>
        public static SysVec4 Background => ColorUtils.WithOpacity(CurrentPalette.Yellow, 0.15f);

        /// <summary>Warning color for borders and outlines around warning-related UI elements.</summary>
        public static SysVec4 Border => ColorUtils.WithOpacity(CurrentPalette.Yellow, 0.4f);
    }

    /// <summary>
    /// Colors for indicating errors, failures, and critical states requiring immediate attention.
    /// </summary>
    public static class Error
    {
        /// <summary>Primary error color for main error indicators such as error icons and failure messages.</summary>
        public static SysVec4 Primary => CurrentPalette.Red;

        /// <summary>Subtle error color for background highlights and low-emphasis error states.</summary>
        public static SysVec4 Background => ColorUtils.WithOpacity(CurrentPalette.Red, 0.15f);

        /// <summary>Error color for borders and outlines around error-related UI elements.</summary>
        public static SysVec4 Border => ColorUtils.WithOpacity(CurrentPalette.Red, 0.4f);
    }

    /// <summary>
    /// Colors for informational content, help text, and neutral information.
    /// </summary>
    public static class Info
    {
        /// <summary>Primary information color for main info indicators such as info icons and help messages.</summary>
        public static SysVec4 Primary => CurrentPalette.Blue;

        /// <summary>Subtle information color for background highlights and low-emphasis informational states.</summary>
        public static SysVec4 Background => ColorUtils.WithOpacity(CurrentPalette.Blue, 0.15f);

        /// <summary>Information color for borders and outlines around informational UI elements.</summary>
        public static SysVec4 Border => ColorUtils.WithOpacity(CurrentPalette.Blue, 0.4f);
    }

    /// <summary>
    /// Colors for primary actions and key interface elements.
    /// </summary>
    public static class Primary
    {
        /// <summary>Primary accent color for main actions, highlights, and key interactive elements.</summary>
        public static SysVec4 Main => CurrentPalette.Blue;

        /// <summary>Secondary primary color for supporting elements and complementary accents.</summary>
        public static SysVec4 Secondary => CurrentPalette.Sapphire;

        /// <summary>Subtle primary color for background highlights and low-emphasis primary states.</summary>
        public static SysVec4 Background => ColorUtils.WithOpacity(CurrentPalette.Blue, 0.15f);

        /// <summary>Primary color for borders and outlines around primary UI elements.</summary>
        public static SysVec4 Border => ColorUtils.WithOpacity(CurrentPalette.Blue, 0.4f);
    }

    /// <summary>
    /// Colors for secondary actions and alternative interface elements.
    /// </summary>
    public static class Secondary
    {
        /// <summary>Secondary accent color for alternative actions and secondary interactive elements.</summary>
        public static SysVec4 Main => CurrentPalette.Lavender;

        /// <summary>Muted secondary color for supporting elements and subtle accents.</summary>
        public static SysVec4 Muted => CurrentPalette.Mauve;

        /// <summary>Subtle secondary color for background highlights and low-emphasis secondary states.</summary>
        public static SysVec4 Background => ColorUtils.WithOpacity(CurrentPalette.Lavender, 0.15f);
    }

    /// <summary>
    /// Neutral colors for backgrounds, text, and structural elements.
    /// </summary>
    public static class Neutral
    {
        /// <summary>Primary text color for main content and high-contrast text.</summary>
        public static SysVec4 Text => CurrentPalette.Text;

        /// <summary>Secondary text color for less prominent content and medium-contrast text.</summary>
        public static SysVec4 TextSecondary => CurrentPalette.Subtext1;

        /// <summary>Tertiary text color for disabled, placeholder, or low-contrast content.</summary>
        public static SysVec4 TextTertiary => CurrentPalette.Subtext0;

        /// <summary>Primary background color for main application surfaces.</summary>
        public static SysVec4 Background => CurrentPalette.Base;

        /// <summary>Secondary background color for panels, containers, and secondary surfaces.</summary>
        public static SysVec4 BackgroundSecondary => CurrentPalette.Mantle;

        /// <summary>Tertiary background color for elevated surfaces, cards, and interactive backgrounds.</summary>
        public static SysVec4 BackgroundTertiary => CurrentPalette.Surface0;

        /// <summary>Border color for separators, outlines, and structural boundaries.</summary>
        public static SysVec4 Border => CurrentPalette.Overlay0;

        /// <summary>Subtle border color for minimal separation and low-contrast boundaries.</summary>
        public static SysVec4 BorderSubtle => CurrentPalette.Overlay1;
    }
}
