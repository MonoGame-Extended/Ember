// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;

namespace Ember.UI;

/// <summary>
/// Utility methods for color manipulation and conversion.
/// </summary>
public static class ColorUtils
{
    /// <summary>
    /// Creates a new color with the specified opacity while preserving RGB values.
    /// </summary>
    /// <param name="color">The base color to modify.</param>
    /// <param name="opacity">The opacity value between 0.0 (fully transparent) and 1.0 (fully opaque).</param>
    /// <returns>A new color with the specified opacity.</returns>
    public static Vector4 WithOpacity(Vector4 color, float opacity)
    {
        return new Vector4(color.X, color.Y, color.Z, Math.Clamp(opacity, 0.0f, 1.0f));
    }

    /// <summary>
    /// Creates a darker variant of the specified color by reducing luminance.
    /// </summary>
    /// <param name="color">The base color to darken.</param>
    /// <param name="factor">The darkening factor between 0.0 and 1.0, where values less than 1.0 make the color darker.</param>
    /// <returns>A darker version of the input color with the same alpha value.</returns>
    public static Vector4 Darken(Vector4 color, float factor = 0.8f)
    {
        factor = Math.Clamp(factor, 0.0f, 1.0f);
        return new Vector4(color.X * factor, color.Y * factor, color.Z * factor, color.W);
    }

    /// <summary>
    /// Creates a lighter variant of the specified color by increasing luminance.
    /// </summary>
    /// <param name="color">The base color to lighten.</param>
    /// <param name="factor">The lightening factor greater than 1.0, where higher values make the color lighter.</param>
    /// <returns>A lighter version of the input color with RGB values clamped to 1.0 and the same alpha value.</returns>
    public static Vector4 Lighten(Vector4 color, float factor = 1.2f)
    {
        factor = Math.Max(factor, 1.0f);
        return new Vector4(
            Math.Min(color.X * factor, 1.0f),
            Math.Min(color.Y * factor, 1.0f),
            Math.Min(color.Z * factor, 1.0f),
            color.W
        );
    }
}
