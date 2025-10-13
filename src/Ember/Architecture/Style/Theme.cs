// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Hexa.NET.ImGui;
using static Hexa.NET.ImGui.ImGui;

namespace Ember.Architecture.Style;

public static class Theme
{
    public static void Apply(ITheme theme)
    {
        ThemeData data = theme.GetThemeData();
        ApplyColors(data.Colors);
        ApplyMetrics(data.Metrics);
    }

    private static void ApplyColors(ThemeColors colors)
    {
        ImGuiStylePtr style = GetStyle();

        style.Colors[(int)ImGuiCol.Text] = colors.Text;
        style.Colors[(int)ImGuiCol.TextDisabled] = colors.TextDisabled;
        style.Colors[(int)ImGuiCol.WindowBg] = colors.WindowBackground;
        style.Colors[(int)ImGuiCol.ChildBg] = colors.WindowBackground;
        style.Colors[(int)ImGuiCol.PopupBg] = colors.PopupBackground;
        style.Colors[(int)ImGuiCol.Border] = colors.Border;
        style.Colors[(int)ImGuiCol.BorderShadow] = colors.BorderShadow;
        style.Colors[(int)ImGuiCol.FrameBg] = colors.FrameBackground;
        style.Colors[(int)ImGuiCol.FrameBgHovered] = colors.FrameBackgroundHovered;
        style.Colors[(int)ImGuiCol.FrameBgActive] = colors.FrameBackgroundActive;
        style.Colors[(int)ImGuiCol.TitleBg] = colors.TitleBackground;
        style.Colors[(int)ImGuiCol.TitleBgActive] = colors.TitleBackgroundActive;
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = colors.TitleBackground;
        style.Colors[(int)ImGuiCol.MenuBarBg] = colors.MenuBarBackground;
        style.Colors[(int)ImGuiCol.ScrollbarBg] = colors.ScrollbarBackground;
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = colors.ScrollbarGrab;
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = colors.ScrollbarGrabHovered;
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = colors.ScrollbarGrabActive;
        style.Colors[(int)ImGuiCol.CheckMark] = colors.CheckMark;
        style.Colors[(int)ImGuiCol.SliderGrab] = colors.SliderGrab;
        style.Colors[(int)ImGuiCol.SliderGrabActive] = colors.SliderGrabActive;
        style.Colors[(int)ImGuiCol.Button] = colors.Button;
        style.Colors[(int)ImGuiCol.ButtonHovered] = colors.ButtonHovered;
        style.Colors[(int)ImGuiCol.ButtonActive] = colors.ButtonActive;
        style.Colors[(int)ImGuiCol.Header] = colors.Header;
        style.Colors[(int)ImGuiCol.HeaderHovered] = colors.HeaderHovered;
        style.Colors[(int)ImGuiCol.HeaderActive] = colors.HeaderActive;
        style.Colors[(int)ImGuiCol.Separator] = colors.Border;
        style.Colors[(int)ImGuiCol.SeparatorHovered] = colors.SeparatorHovered;
        style.Colors[(int)ImGuiCol.SeparatorActive] = colors.SeparatorActive;
        style.Colors[(int)ImGuiCol.ResizeGrip] = colors.ResizeGrip;
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = colors.ResizeGripHovered;
        style.Colors[(int)ImGuiCol.ResizeGripActive] = colors.ResizeGripActive;
        style.Colors[(int)ImGuiCol.Tab] = colors.TitleBackground;
        style.Colors[(int)ImGuiCol.TabHovered] = colors.ButtonHovered;
        style.Colors[(int)ImGuiCol.TabSelected] = colors.FrameBackground;
        style.Colors[(int)ImGuiCol.TabSelectedOverline] = colors.TabSelectedOverline;
        style.Colors[(int)ImGuiCol.TabDimmed] = colors.TabDimmed;
        style.Colors[(int)ImGuiCol.TabDimmedSelected] = colors.TabDimmedSelected;
        style.Colors[(int)ImGuiCol.TabDimmedSelectedOverline] = colors.TabDimmedSelectedOverline;
        style.Colors[(int)ImGuiCol.DockingPreview] = colors.DockingPreview;
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = colors.TitleBackground;
        style.Colors[(int)ImGuiCol.PlotLines] = colors.PlotLines;
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = colors.PlotLinesHovered;
        style.Colors[(int)ImGuiCol.PlotHistogram] = colors.PlotHistogram;
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = colors.PlotHistogramHovered;
        style.Colors[(int)ImGuiCol.TableHeaderBg] = colors.TableHeaderBackground;
        style.Colors[(int)ImGuiCol.TableBorderStrong] = colors.TableBorderStrong;
        style.Colors[(int)ImGuiCol.TableBorderLight] = colors.TableBorderLight;
        style.Colors[(int)ImGuiCol.TableRowBg] = colors.TableRowBackground;
        style.Colors[(int)ImGuiCol.TableRowBgAlt] = colors.TableRowBackgroundAlt;
        style.Colors[(int)ImGuiCol.TextSelectedBg] = colors.TextSelectedBackground;
        style.Colors[(int)ImGuiCol.DragDropTarget] = colors.DragDropTarget;
        style.Colors[(int)ImGuiCol.NavWindowingHighlight] = colors.NavWindowHighlight;
        style.Colors[(int)ImGuiCol.NavWindowingDimBg] = colors.NavWindowDimBackground;
        style.Colors[(int)ImGuiCol.ModalWindowDimBg] = colors.ModalWindowDimBackground;
    }

    private static void ApplyMetrics(ThemeMetrics metrics)
    {
        ImGuiStylePtr style = GetStyle();

        style.Alpha = metrics.Alpha;
        style.DisabledAlpha = metrics.DisabledAlpha;

        style.WindowPadding = metrics.WindowPadding;
        style.WindowRounding = metrics.WindowRounding;
        style.WindowBorderSize = metrics.WindowBorderSize;

        style.ChildRounding = metrics.ChildRounding;
        style.ChildBorderSize = metrics.ChildBorderSize;

        style.PopupRounding = metrics.PopupRounding;
        style.PopupBorderSize = metrics.PopupBorderSize;

        style.FramePadding = metrics.FramePadding;
        style.FrameRounding = metrics.FrameRounding;
        style.FrameBorderSize = metrics.FrameBorderSize;

        style.ItemSpacing = metrics.ItemSpacing;
        style.ItemInnerSpacing = metrics.ItemInnerSpacing;
        style.CellPadding = metrics.CellPadding;
        style.IndentSpacing = metrics.IndentSpacing;

        style.ScrollbarSize = metrics.ScrollbarSize;
        style.ScrollbarRounding = metrics.ScrollbarRounding;
        style.GrabMinSize = metrics.GrabMinimumSize;
        style.GrabRounding = metrics.GrabRounding;

        style.TabRounding = metrics.TabRounding;
        style.TabBorderSize = metrics.TabBorderSize;

        style.AntiAliasedLines = metrics.AntiAliasedLines;
        style.AntiAliasedFill = metrics.AntiAliasedFill;


        style.ScaleAllSizes(metrics.DpiScale);
    }
}
