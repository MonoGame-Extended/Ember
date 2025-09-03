// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Hexa.NET.ImGui;

namespace Ember.UI;

public static class Fonts
{
    private static readonly uint[] s_excludeRange = [0xE000, 0xF8FF, 0];
    private static ImFontPtr s_mainFontPtr;

    public const string VisibleIcon = "\uf06e";
    public const string NotVisibleIcon = "\uf070";
    public const string LockIcon = "\uf023";
    public const string UnlockedIcon = "\uf3c1";
    public const string DeleteIcon = "\uf1f8";
    public const string EnabledIcon = "\uf14a";
    public const string DisabledIcon = "\uf2d3";
    public const string DirectoryIcon = "\uf07b";
    public const string ImageIcon = "\uf03e";

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
