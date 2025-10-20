namespace Ember.Architecture.Style;

public sealed class CatppuccinFrappeTheme : CatppuccinTheme
{
    protected override ThemeColors CreateThemeColors()
    {
        return new ThemeColors
        {
            WindowBackground = new(0.192f, 0.200f, 0.271f, 1.0f),
            ChildBackground = new(0.161f, 0.169f, 0.235f, 1.0f),
            PopupBackground = new(0.247f, 0.267f, 0.349f, 1.0f),
            MenuBarBackground = new(0.161f, 0.169f, 0.235f, 1.0f),

            Text = new(0.780f, 0.816f, 0.961f, 1.0f),
            TextDisabled = new(0.647f, 0.682f, 0.816f, 1.0f),

            TextSelectedBackground = new(0.792f, 0.624f, 0.902f, 0.3f),

            FrameBackground = new(0.314f, 0.337f, 0.427f, 1.0f),
            FrameBackgroundHovered = new(0.380f, 0.408f, 0.502f, 1.0f),
            FrameBackgroundActive = new(0.447f, 0.478f, 0.576f, 1.0f),

            Button = new(0.549f, 0.671f, 0.929f, 0.3f),
            ButtonHovered = new(0.549f, 0.671f, 0.929f, 0.5f),
            ButtonActive = new(0.549f, 0.671f, 0.929f, 0.7f),

            Header = new(0.729f, 0.729f, 0.949f, 0.3f),
            HeaderHovered = new(0.729f, 0.729f, 0.949f, 0.5f),
            HeaderActive = new(0.729f, 0.729f, 0.949f, 0.7f),

            CheckMark = new(0.651f, 0.812f, 0.537f, 1.0f),

            TitleBackground = new(0.137f, 0.145f, 0.200f, 1.0f),
            TitleBackgroundActive = new(0.161f, 0.169f, 0.235f, 1.0f),
            TitleBackgroundCollapsed = new(0.137f, 0.145f, 0.200f, 1.0f),

            Border = new(0.447f, 0.478f, 0.576f, 1.0f),
            BorderShadow = new(0.192f, 0.200f, 0.271f, 0.0f),
            Separator = new(0.447f, 0.478f, 0.576f, 1.0f),
            SeparatorHovered = new(0.729f, 0.729f, 0.949f, 1.0f),
            SeparatorActive = new(0.729f, 0.729f, 0.949f, 1.0f),

            ScrollbarBackground = new(0.192f, 0.200f, 0.271f, 1.0f),
            ScrollbarGrab = new(0.247f, 0.267f, 0.349f, 1.0f),
            ScrollbarGrabHovered = new(0.314f, 0.337f, 0.427f, 1.0f),
            ScrollbarGrabActive = new(0.380f, 0.408f, 0.502f, 1.0f),

            SliderGrab = new(0.549f, 0.671f, 0.929f, 1.0f),
            SliderGrabActive = new(0.518f, 0.761f, 0.863f, 1.0f),

            Tab = new(0.247f, 0.267f, 0.349f, 1.0f),
            TabHovered = new(0.729f, 0.729f, 0.949f, 0.4f),
            TabSelected = new(0.729f, 0.729f, 0.949f, 0.6f),
            TabSelectedOverline = new(0.729f, 0.729f, 0.949f, 1.0f),
            TabDimmed = new(0.247f, 0.267f, 0.349f, 1.0f),
            TabDimmedSelected = new(0.729f, 0.729f, 0.949f, 0.3f),
            TabDimmedSelectedOverline = new(0.729f, 0.729f, 0.949f, 0.6f),

            ResizeGrip = new(0.580f, 0.616f, 0.725f, 0.5f),
            ResizeGripHovered = new(0.549f, 0.671f, 0.929f, 0.7f),
            ResizeGripActive = new(0.549f, 0.671f, 0.929f, 0.9f),

            PlotLines = new(0.549f, 0.671f, 0.929f, 1.0f),
            PlotLinesHovered = new(0.600f, 0.812f, 0.863f, 1.0f),
            PlotHistogram = new(0.510f, 0.780f, 0.753f, 1.0f),
            PlotHistogramHovered = new(0.651f, 0.812f, 0.537f, 1.0f),

            TableHeaderBackground = new(0.247f, 0.267f, 0.349f, 1.0f),
            TableBorderStrong = new(0.514f, 0.549f, 0.651f, 1.0f),
            TableBorderLight = new(0.447f, 0.478f, 0.576f, 1.0f),
            TableRowBackground = new(0.192f, 0.200f, 0.271f, 0.0f),
            TableRowBackgroundAlt = new(0.247f, 0.267f, 0.349f, 0.3f),

            DockingPreview = new(0.549f, 0.671f, 0.929f, 0.7f),
            DockingEmptyBackground = new(0.192f, 0.200f, 0.271f, 1.0f),

            DragDropTarget = new(0.898f, 0.784f, 0.561f, 0.9f),

            NavWindowHighlight = new(0.780f, 0.816f, 0.961f, 0.7f),
            NavWindowDimBackground = new(0.192f, 0.200f, 0.271f, 0.2f),
            ModalWindowDimBackground = new(0.192f, 0.200f, 0.271f, 0.35f),
        };
    }
}
