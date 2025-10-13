namespace Ember.Architecture.Style;

public sealed class CatppuccinLatteTheme : CatppuccinTheme
{
    protected override ThemeColors CreateThemeColors()
    {
        return new ThemeColors
        {
            WindowBackground = new(0.937f, 0.945f, 0.961f, 1.0f),
            ChildBackground = new(0.902f, 0.914f, 0.937f, 1.0f),
            PopupBackground = new(0.800f, 0.816f, 0.855f, 1.0f),
            MenuBarBackground = new(0.902f, 0.914f, 0.937f, 1.0f),

            Text = new(0.298f, 0.310f, 0.412f, 1.0f),
            TextDisabled = new(0.424f, 0.435f, 0.522f, 1.0f),

            TextSelectedBackground = new(0.118f, 0.400f, 0.961f, 0.3f),

            FrameBackground = new(0.737f, 0.753f, 0.800f, 1.0f),
            FrameBackgroundHovered = new(0.675f, 0.694f, 0.745f, 1.0f),
            FrameBackgroundActive = new(0.612f, 0.627f, 0.690f, 1.0f),

            Button = new(0.118f, 0.400f, 0.961f, 0.3f),
            ButtonHovered = new(0.118f, 0.400f, 0.961f, 0.5f),
            ButtonActive = new(0.118f, 0.400f, 0.961f, 0.7f),

            Header = new(0.451f, 0.529f, 0.992f, 0.3f),
            HeaderHovered = new(0.451f, 0.529f, 0.992f, 0.5f),
            HeaderActive = new(0.451f, 0.529f, 0.992f, 0.7f),

            CheckMark = new(0.251f, 0.627f, 0.169f, 1.0f),

            TitleBackground = new(0.863f, 0.878f, 0.910f, 1.0f),
            TitleBackgroundActive = new(0.902f, 0.914f, 0.937f, 1.0f),
            TitleBackgroundCollapsed = new(0.863f, 0.878f, 0.910f, 1.0f),

            Border = new(0.612f, 0.627f, 0.690f, 1.0f),
            BorderShadow = new(0.937f, 0.945f, 0.961f, 0.0f),
            Separator = new(0.612f, 0.627f, 0.690f, 1.0f),
            SeparatorHovered = new(0.451f, 0.529f, 0.992f, 1.0f),
            SeparatorActive = new(0.451f, 0.529f, 0.992f, 1.0f),

            ScrollbarBackground = new(0.937f, 0.945f, 0.961f, 1.0f),
            ScrollbarGrab = new(0.800f, 0.816f, 0.855f, 1.0f),
            ScrollbarGrabHovered = new(0.737f, 0.753f, 0.800f, 1.0f),
            ScrollbarGrabActive = new(0.675f, 0.694f, 0.745f, 1.0f),

            SliderGrab = new(0.118f, 0.400f, 0.961f, 1.0f),
            SliderGrabActive = new(0.129f, 0.624f, 0.710f, 1.0f),

            Tab = new(0.800f, 0.816f, 0.855f, 1.0f),
            TabHovered = new(0.451f, 0.529f, 0.992f, 0.4f),
            TabSelected = new(0.451f, 0.529f, 0.992f, 0.6f),
            TabSelectedOverline = new(0.451f, 0.529f, 0.992f, 1.0f),
            TabDimmed = new(0.800f, 0.816f, 0.855f, 1.0f),
            TabDimmedSelected = new(0.451f, 0.529f, 0.992f, 0.3f),
            TabDimmedSelectedOverline = new(0.451f, 0.529f, 0.992f, 0.6f),

            ResizeGrip = new(0.486f, 0.498f, 0.576f, 0.5f),
            ResizeGripHovered = new(0.118f, 0.400f, 0.961f, 0.7f),
            ResizeGripActive = new(0.118f, 0.400f, 0.961f, 0.9f),

            PlotLines = new(0.118f, 0.400f, 0.961f, 1.0f),
            PlotLinesHovered = new(0.016f, 0.647f, 0.898f, 1.0f),
            PlotHistogram = new(0.094f, 0.573f, 0.600f, 1.0f),
            PlotHistogramHovered = new(0.251f, 0.627f, 0.169f, 1.0f),

            TableHeaderBackground = new(0.800f, 0.816f, 0.855f, 1.0f),
            TableBorderStrong = new(0.549f, 0.561f, 0.631f, 1.0f),
            TableBorderLight = new(0.612f, 0.627f, 0.690f, 1.0f),
            TableRowBackground = new(0.937f, 0.945f, 0.961f, 0.0f),
            TableRowBackgroundAlt = new(0.800f, 0.816f, 0.855f, 0.3f),

            DockingPreview = new(0.118f, 0.400f, 0.961f, 0.7f),
            DockingEmptyBackground = new(0.937f, 0.945f, 0.961f, 1.0f),

            DragDropTarget = new(0.875f, 0.557f, 0.114f, 0.9f),

            NavWindowHighlight = new(0.298f, 0.310f, 0.412f, 0.7f),
            NavWindowDimBackground = new(0.937f, 0.945f, 0.961f, 0.2f),
            ModalWindowDimBackground = new(0.937f, 0.945f, 0.961f, 0.35f),
        };
    }
}
