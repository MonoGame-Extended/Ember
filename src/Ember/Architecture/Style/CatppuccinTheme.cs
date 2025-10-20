// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Ember.Architecture.Style;

public abstract class CatppuccinTheme : ITheme
{
    public ThemeData GetThemeData()
    {
        return new ThemeData
        {
            Colors = CreateThemeColors(),
            Metrics = CreateThemeMetrics()
        };
    }

    protected abstract ThemeColors CreateThemeColors();

    private ThemeMetrics CreateThemeMetrics()
    {
        return new ThemeMetrics
        {
            // DPI scaling
            DpiScale = 1.0f,

            // Transparency
            Alpha = 1.0f,
            DisabledAlpha = 0.6f,

            // Window styling
            WindowPadding = new SysVec2(12.0f, 12.0f),
            WindowRounding = 8.0f,
            WindowBorderSize = 1.0f,

            // Child window styling
            ChildRounding = 6.0f,
            ChildBorderSize = 1.0f,

            // Popup styling
            PopupRounding = 6.0f,
            PopupBorderSize = 1.0f,

            // Frame styling
            FramePadding = new SysVec2(8.0f, 8.0f),
            FrameRounding = 6.0f,
            FrameBorderSize = 1.0f,

            // Item layout
            ItemSpacing = new SysVec2(8.0f, 8.0f),
            ItemInnerSpacing = new SysVec2(6.0f, 6.0f),
            CellPadding = new SysVec2(4.0f, 2.0f),
            IndentSpacing = 24.0f,

            // Scrollbar styling
            ScrollbarSize = 16.0f,
            ScrollbarRounding = 6.0f,
            GrabMinimumSize = 12.0f,
            GrabRounding = 6.0f,

            // Tab styling
            TabRounding = 4.0f,
            TabBorderSize = 0.0f,

            // Rendering quality
            AntiAliasedLines = true,
            AntiAliasedFill = true
        };
    }
}
