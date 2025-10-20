// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Hexa.NET.ImGui;

namespace Ember.Architecture;

public static partial class Fonts
{
    private static readonly uint[] s_excludeRange = [0xE000, 0xF8FF, 0];
    private static ImFontPtr s_mainFontPtr;

    public static unsafe void Load()
    {
        ImGuiIOPtr ioPtr = ImGui.GetIO();

        ioPtr.Fonts.Clear();

        fixed (uint* iconExcludeRangePtr = s_excludeRange)
        {
            // Load the base font, excluding the icon range
            ImFontConfigPtr baseFontConfigPtr = ImGui.ImFontConfig();
            baseFontConfigPtr.GlyphExcludeRanges = iconExcludeRangePtr;
            s_mainFontPtr = ioPtr.Fonts.AddFontFromFileTTF("./Content/fonts/JetBrainsMono-Regular.ttf"u8, 0.0f, baseFontConfigPtr);

            // Load the font with icons, which will use the excluded range
            // on merge. I dunno, ImGui magic
            ImFontConfigPtr iconFontConfigPtr = ImGui.ImFontConfig();
            iconFontConfigPtr.MergeMode = true;
            ioPtr.Fonts.AddFontFromFileTTF("./Content/fonts/fa-solid-900.ttf"u8, 0.0f, iconFontConfigPtr);
        }

    }
}
