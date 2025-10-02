// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;

namespace Ember.UI.Styling;

/// <summary>
/// Contains all color values for a specific Catppuccin theme variant.
/// Color values are normalized floating-point (0.0 to 1.0).
/// </summary>
public struct CatppuccinPalette
{
    // Base colors (backgrounds)
    public SysVec4 Base;
    public SysVec4 Mantle;
    public SysVec4 Crust;

    // Surface hierarchy
    public SysVec4 Surface0;
    public SysVec4 Surface1;
    public SysVec4 Surface2;

    // Overlay hierarchy
    public SysVec4 Overlay0;
    public SysVec4 Overlay1;
    public SysVec4 Overlay2;

    // Text hierarchy
    public SysVec4 Text;
    public SysVec4 Subtext1;
    public SysVec4 Subtext0;

    // Accent colors
    public SysVec4 Rosewater;
    public SysVec4 Blue;
    public SysVec4 Lavender;
    public SysVec4 Sapphire;
    public SysVec4 Sky;
    public SysVec4 Teal;
    public SysVec4 Green;
    public SysVec4 Yellow;
    public SysVec4 Peach;
    public SysVec4 Maroon;
    public SysVec4 Red;
    public SysVec4 Mauve;
    public SysVec4 Pink;

    /// <summary>
    /// Gets the color palette for the specified Catppuccin variant.
    /// </summary>
    /// <param name="variant">The theme variant to get the palette for.</param>
    /// <returns>A palette containing all normalized color values for the specified variant.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid variant is specified.</exception>
    public static CatppuccinPalette GetPalette(CatppuccinVariant variant) => variant switch
    {
        CatppuccinVariant.Latte => new()
        {
            Base = new(0.937f, 0.945f, 0.961f, 1.0f),
            Mantle = new(0.902f, 0.914f, 0.937f, 1.0f),
            Crust = new(0.863f, 0.878f, 0.910f, 1.0f),
            Surface0 = new(0.800f, 0.816f, 0.855f, 1.0f),
            Surface1 = new(0.737f, 0.753f, 0.800f, 1.0f),
            Surface2 = new(0.675f, 0.694f, 0.745f, 1.0f),
            Overlay0 = new(0.612f, 0.627f, 0.690f, 1.0f),
            Overlay1 = new(0.549f, 0.561f, 0.631f, 1.0f),
            Overlay2 = new(0.486f, 0.498f, 0.576f, 1.0f),
            Text = new(0.298f, 0.310f, 0.412f, 1.0f),
            Subtext1 = new(0.361f, 0.373f, 0.467f, 1.0f),
            Subtext0 = new(0.424f, 0.435f, 0.522f, 1.0f),
            Rosewater = new(0.863f, 0.537f, 0.467f, 1.0f),
            Blue = new(0.118f, 0.400f, 0.961f, 1.0f),
            Lavender = new(0.451f, 0.529f, 0.992f, 1.0f),
            Sapphire = new(0.129f, 0.624f, 0.710f, 1.0f),
            Sky = new(0.016f, 0.647f, 0.898f, 1.0f),
            Teal = new(0.094f, 0.573f, 0.600f, 1.0f),
            Green = new(0.251f, 0.627f, 0.169f, 1.0f),
            Yellow = new(0.875f, 0.557f, 0.114f, 1.0f),
            Peach = new(0.996f, 0.392f, 0.043f, 1.0f),
            Maroon = new(0.902f, 0.271f, 0.325f, 1.0f),
            Red = new(0.820f, 0.059f, 0.224f, 1.0f),
            Mauve = new(0.533f, 0.224f, 0.937f, 1.0f),
            Pink = new(0.918f, 0.463f, 0.796f, 1.0f)
        },

        CatppuccinVariant.Frappe => new()
        {
            Base = new(0.192f, 0.200f, 0.271f, 1.0f),
            Mantle = new(0.161f, 0.169f, 0.235f, 1.0f),
            Crust = new(0.137f, 0.145f, 0.200f, 1.0f),
            Surface0 = new(0.247f, 0.267f, 0.349f, 1.0f),
            Surface1 = new(0.314f, 0.337f, 0.427f, 1.0f),
            Surface2 = new(0.380f, 0.408f, 0.502f, 1.0f),
            Overlay0 = new(0.447f, 0.478f, 0.576f, 1.0f),
            Overlay1 = new(0.514f, 0.549f, 0.651f, 1.0f),
            Overlay2 = new(0.580f, 0.616f, 0.725f, 1.0f),
            Text = new(0.780f, 0.816f, 0.961f, 1.0f),
            Subtext1 = new(0.714f, 0.753f, 0.894f, 1.0f),
            Subtext0 = new(0.647f, 0.682f, 0.816f, 1.0f),
            Rosewater = new(0.949f, 0.835f, 0.812f, 1.0f),
            Blue = new(0.549f, 0.671f, 0.929f, 1.0f),
            Lavender = new(0.729f, 0.729f, 0.949f, 1.0f),
            Sapphire = new(0.518f, 0.761f, 0.863f, 1.0f),
            Sky = new(0.600f, 0.812f, 0.863f, 1.0f),
            Teal = new(0.510f, 0.780f, 0.753f, 1.0f),
            Green = new(0.651f, 0.812f, 0.537f, 1.0f),
            Yellow = new(0.898f, 0.784f, 0.561f, 1.0f),
            Peach = new(0.937f, 0.624f, 0.463f, 1.0f),
            Maroon = new(0.918f, 0.600f, 0.612f, 1.0f),
            Red = new(0.906f, 0.514f, 0.518f, 1.0f),
            Mauve = new(0.792f, 0.624f, 0.902f, 1.0f),
            Pink = new(0.961f, 0.722f, 0.894f, 1.0f)
        },

        CatppuccinVariant.Macchiato => new()
        {
            Base = new(0.141f, 0.153f, 0.227f, 1.0f),
            Mantle = new(0.118f, 0.125f, 0.188f, 1.0f),
            Crust = new(0.094f, 0.098f, 0.149f, 1.0f),
            Surface0 = new(0.212f, 0.227f, 0.310f, 1.0f),
            Surface1 = new(0.286f, 0.302f, 0.392f, 1.0f),
            Surface2 = new(0.357f, 0.376f, 0.471f, 1.0f),
            Overlay0 = new(0.431f, 0.451f, 0.553f, 1.0f),
            Overlay1 = new(0.502f, 0.529f, 0.635f, 1.0f),
            Overlay2 = new(0.576f, 0.604f, 0.714f, 1.0f),
            Text = new(0.792f, 0.827f, 0.961f, 1.0f),
            Subtext1 = new(0.722f, 0.753f, 0.878f, 1.0f),
            Subtext0 = new(0.647f, 0.675f, 0.796f, 1.0f),
            Rosewater = new(0.957f, 0.855f, 0.839f, 1.0f),
            Blue = new(0.541f, 0.678f, 0.957f, 1.0f),
            Lavender = new(0.718f, 0.737f, 0.973f, 1.0f),
            Sapphire = new(0.490f, 0.769f, 0.894f, 1.0f),
            Sky = new(0.569f, 0.843f, 0.890f, 1.0f),
            Teal = new(0.549f, 0.835f, 0.788f, 1.0f),
            Green = new(0.651f, 0.855f, 0.584f, 1.0f),
            Yellow = new(0.933f, 0.827f, 0.624f, 1.0f),
            Peach = new(0.961f, 0.663f, 0.498f, 1.0f),
            Maroon = new(0.933f, 0.600f, 0.627f, 1.0f),
            Red = new(0.929f, 0.529f, 0.588f, 1.0f),
            Mauve = new(0.776f, 0.627f, 0.961f, 1.0f),
            Pink = new(0.961f, 0.737f, 0.902f, 1.0f)
        },

        CatppuccinVariant.Mocha => new()
        {
            Base = new(0.118f, 0.141f, 0.216f, 1.0f),
            Mantle = new(0.110f, 0.129f, 0.196f, 1.0f),
            Crust = new(0.090f, 0.106f, 0.176f, 1.0f),
            Surface0 = new(0.149f, 0.196f, 0.322f, 1.0f),
            Surface1 = new(0.196f, 0.247f, 0.376f, 1.0f),
            Surface2 = new(0.251f, 0.306f, 0.435f, 1.0f),
            Overlay0 = new(0.322f, 0.376f, 0.514f, 1.0f),
            Overlay1 = new(0.392f, 0.451f, 0.580f, 1.0f),
            Overlay2 = new(0.463f, 0.514f, 0.639f, 1.0f),
            Text = new(0.804f, 0.835f, 0.894f, 1.0f),
            Subtext1 = new(0.714f, 0.749f, 0.835f, 1.0f),
            Subtext0 = new(0.627f, 0.678f, 0.784f, 1.0f),
            Rosewater = new(0.961f, 0.761f, 0.761f, 1.0f),
            Blue = new(0.541f, 0.725f, 0.961f, 1.0f),
            Lavender = new(0.714f, 0.725f, 0.961f, 1.0f),
            Sapphire = new(0.459f, 0.725f, 0.961f, 1.0f),
            Sky = new(0.541f, 0.882f, 0.961f, 1.0f),
            Teal = new(0.580f, 0.886f, 0.816f, 1.0f),
            Green = new(0.659f, 0.890f, 0.631f, 1.0f),
            Yellow = new(0.988f, 0.875f, 0.549f, 1.0f),
            Peach = new(0.988f, 0.718f, 0.549f, 1.0f),
            Maroon = new(0.937f, 0.573f, 0.580f, 1.0f),
            Red = new(0.961f, 0.533f, 0.561f, 1.0f),
            Mauve = new(0.804f, 0.624f, 0.843f, 1.0f),
            Pink = new(0.961f, 0.678f, 0.851f, 1.0f)
        },

        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, "Invalid Catppuccin variant specified.")
    };
}
