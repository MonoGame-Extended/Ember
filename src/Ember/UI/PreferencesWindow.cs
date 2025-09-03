// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Ember.UI.Styling;
using Hexa.NET.ImGui;

namespace Ember.UI;

public static class PreferencesWindow
{
    private static bool s_isOpen = false;

    public static void Open()
    {
        s_isOpen = true;
    }

    public static void Draw()
    {
        if (!s_isOpen)
        {
            return;
        }

        ImGuiViewportPtr viewPortPtr = ImGui.GetMainViewport();
        SysVec2 workPos = viewPortPtr.WorkPos;
        SysVec2 workSize = viewPortPtr.WorkSize;
        SysVec2 workCenter = workPos + (workSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Appearing, new SysVec2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new SysVec2(500, 400), ImGuiCond.Appearing);

        if (ImGui.Begin(SR.Window_Preferences, ref s_isOpen))
        {
            if (ImGui.BeginTabBar("PreferencesTabBar"u8, ImGuiTabBarFlags.None))
            {
                ShowFontTab();
                ShowBackgroundTab();

                ImGui.EndTabBar();
            }
        }
        ImGui.End();
    }

    private static void ShowFontTab()
    {
        if (ImGui.BeginTabItem(SR.TabItem_Font))
        {
            // Base font size control
            float baseFontSize = EmberContext.BaseFontSize;
            ImGui.Text(SR.Label_BaseFontSize);
            ImGui.SetNextItemWidth(250.0f);
            if (ImGui.DragFloat("##base_font_size"u8, ref baseFontSize, 0.5f, 8.0f, 72.0f, "%.0f px"u8))
            {
                EmberContext.BaseFontSize = baseFontSize;
            }
            ImGui.SameLine();
            ImGui.TextDisabled(SR.FormatUtf8(nameof(SR.Message_EffectiveFontSize), ImGui.GetFontSize().ToString("F1")));

            ImGui.Spacing();

            // Main scaling factor
            float fontScaleMain = EmberContext.FontScaleMain;
            ImGui.Text(SR.Label_UIScale);
            ImGui.SetNextItemWidth(250.0f);
            if (ImGui.DragFloat("##ui_scale"u8, ref fontScaleMain, 0.01f, 0.5f, 4.0f, "%.2f"u8))
            {
                EmberContext.FontScaleMain = fontScaleMain;
            }

            ImGui.Spacing();
            ImGui.Spacing();

            // Font Presets
            if (ImGui.Button(SR.Button_SmallFont))
            {
                EmberContext.BaseFontSize = 14.0f;
                EmberContext.FontScaleMain = 0.9f;
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_DefaultFont))
            {
                EmberContext.BaseFontSize = 16.0f;
                EmberContext.FontScaleMain = 1.0f;
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_LargeFont))
            {
                EmberContext.BaseFontSize = 18.0f;
                EmberContext.FontScaleMain = 1.2f;
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_ExtraLargeFont))
            {
                EmberContext.BaseFontSize = 20.0f;
                EmberContext.FontScaleMain = 1.4f;
            }

            ImGui.EndTabItem();
        }
    }

    private static void ShowBackgroundTab()
    {
        if (ImGui.BeginTabItem(SR.TabItem_Background))
        {
            ImGui.Text(SR.Label_BackgroundColor);

            float[] color =
            [
                EmberContext.ClearColor.R / 255.0f,
                EmberContext.ClearColor.G / 255.0f,
                EmberContext.ClearColor.B / 255.0f,
                EmberContext.ClearColor.A / 255.0f
            ];

            if (ImGui.ColorEdit4("##clear_color"u8, color, ImGuiColorEditFlags.AlphaPreviewHalf))
            {
                EmberContext.ClearColor = new(color[0], color[1], color[2], color[3]);
            }

            ImGui.Spacing();
            ImGui.Spacing();

            // Color Presets
            if (ImGui.Button(SR.Button_ColorTheme))
            {
                SysVec4 themeColor = SemanticColors.Primary.Background;
                EmberContext.ClearColor = new XnaColor(themeColor.X, themeColor.Y, themeColor.Z, themeColor.W);
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_ColorBlack))
            {
                EmberContext.ClearColor = XnaColor.Black;
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_ColorWhite))
            {
                EmberContext.ClearColor = XnaColor.White;
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_ColorCornflowerBlue))
            {
                EmberContext.ClearColor = XnaColor.CornflowerBlue;
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_ColorMonoGameOrange))
            {
                EmberContext.ClearColor = XnaColor.MonoGameOrange;
            }


            ImGui.EndTabItem();
        }
    }
}
