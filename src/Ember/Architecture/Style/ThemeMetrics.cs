// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.


namespace Ember.Architecture.Style;

public class ThemeMetrics
{
    public float DpiScale { get; set; }
    public float Alpha { get; set; }
    public float DisabledAlpha { get; set; }
    public SysVec2 WindowPadding { get; set; }
    public float WindowRounding { get; set; }
    public float WindowBorderSize { get; set; }
    public float ChildRounding { get; set; }
    public float ChildBorderSize { get; set; }
    public float PopupRounding { get; set; }
    public float PopupBorderSize { get; set; }
    public SysVec2 FramePadding { get; set; }
    public float FrameRounding { get; set; }
    public float FrameBorderSize { get; set; }
    public SysVec2 ItemSpacing { get; set; }
    public SysVec2 ItemInnerSpacing { get; set; }
    public SysVec2 CellPadding { get; set; }
    public float IndentSpacing { get; set; }
    public float ScrollbarSize { get; set; }
    public float ScrollbarRounding { get; set; }
    public float GrabMinimumSize { get; set; }
    public float GrabRounding { get; set; }
    public float TabRounding { get; set; }
    public float TabBorderSize { get; set; }
    public bool AntiAliasedLines { get; set; }
    public bool AntiAliasedFill { get; set; }
}
