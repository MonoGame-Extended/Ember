using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.ImGui.Backends.SDL2;
using Hexa.NET.SDL2;

namespace ImGuiFileDialog;

public static class ImGuiFileDialog
{
    public static string? ShowOpenFileDialog(string title = "Open File", string? initialDirectory = null, string? fileFilter = null)
    {
        return ShowDialog(title, initialDirectory, fileFilter, FileDialogMode.OpenFile);
    }

    public static string? ShowSaveFileDialog(string title = "Save File", string? initialDirectory = null, string? fileFilter = null)
    {
        return ShowDialog(title, initialDirectory, fileFilter, FileDialogMode.SaveFile);
    }

    public static string? ShowSelectFolderDialog(string title = "Select Folder", string? initialDirectory = null)
    {
        return ShowDialog(title, initialDirectory, null, FileDialogMode.SelectFolder);
    }

    private static string? ShowDialog(string title, string? initialDirectory, string? fileFilter, FileDialogMode mode)
    {
        string? result = null;
        Exception? threadException = null;

        var dialogThread = new Thread(() =>
        {
            try
            {
                result = RunDialogThread(title, initialDirectory ?? Environment.CurrentDirectory, fileFilter, mode);
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
        });

        dialogThread.Start();
        dialogThread.Join();

        if (threadException != null)
        {
            throw new Exception("Error in file dialog thread", threadException);
        }

        return result;
    }

    private static unsafe string RunDialogThread(string title, string initialDirectory, string fileFilter, FileDialogMode mode)
    {
        // if (SDL.Init(SDL.SDL_INIT_VIDEO) < 0)
        // {
        //     throw new Exception($"Failed to initialize SDL: {SDL.GetErrorS()}");
        // }

        try
        {
            SDL.GLSetAttribute(SDLGLattr.GlContextMajorVersion, 3);
            SDL.GLSetAttribute(SDLGLattr.GlContextMinorVersion, 3);
            SDL.GLSetAttribute(SDLGLattr.GlContextProfileMask, (int)SDLGLprofile.GlContextProfileCore);
            SDL.GLSetAttribute(SDLGLattr.GlDoublebuffer, 1);
            SDL.GLSetAttribute(SDLGLattr.GlDepthSize, 24);
            SDL.GLSetAttribute(SDLGLattr.GlStencilSize, 8);

            var window = SDL.CreateWindow(title,
                                          (int)SDL.SDL_WINDOWPOS_CENTERED_MASK,
                                          (int)SDL.SDL_WINDOWPOS_CENTERED_MASK,
                                          800,
                                          600,
                                          (uint)(SDLWindowFlags.Opengl | SDLWindowFlags.Shown));

            if (window == null)
            {
                throw new Exception($"Failed to create window: {SDL.GetErrorS()}");
            }

            try
            {
                var glContext = SDL.GLCreateContext(window);
                if (glContext == null)
                {
                    throw new Exception($"Failed to create OpenGL context: {SDL.GetErrorS()}");
                }

                try
                {
                    SDL.GLMakeCurrent(window, glContext);
                    SDL.GLSetSwapInterval(1);

                    var context = ImGui.CreateContext(null);
                    ImGui.SetCurrentContext(context);

                    try
                    {
                        var io = ImGui.GetIO();
                        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
                        ImGui.StyleColorsDark(null);

                        ImGuiImplSDL2.SetCurrentContext(context);
                        ImGuiImplSDL2.InitForOpenGL((Hexa.NET.ImGui.Backends.SDL2.SDLWindow*)window, (void*)glContext.Handle);

                        ImGuiImplOpenGL3.SetCurrentContext(context);
                        ImGuiImplOpenGL3.Init("#version 330");

                        var dialogState = new FileDialogState(initialDirectory, fileFilter, mode);
                        string? result = null;
                        bool running = true;

                        while (running)
                        {
                            Hexa.NET.SDL2.SDLEvent evt;
                            while (SDL.PollEvent(&evt) != 0)
                            {
                                ImGuiImplSDL2.ProcessEvent((Hexa.NET.ImGui.Backends.SDL2.SDLEvent*)&evt);

                                if (evt.Type == (uint)SDLEventType.Quit)
                                {
                                    running = false;
                                }
                            }

                            ImGuiImplOpenGL3.NewFrame();
                            ImGuiImplSDL2.NewFrame();
                            ImGui.NewFrame();

                            if (RenderFileDialog(dialogState, out result))
                            {
                                running = false;
                            }

                            ImGui.Render();
                            int displayW, displayH;
                            SDL.GLGetDrawableSize(window, &displayW, &displayH);

                            ImGuiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());
                            SDL.GLSwapWindow(window);
                        }

                        ImGuiImplOpenGL3.Shutdown();
                        ImGuiImplSDL2.Shutdown();

                        return result;
                    }
                    finally
                    {
                        ImGui.DestroyContext(context);
                    }
                }
                finally
                {
                    SDL.GLDeleteContext(glContext);
                }
            }
            finally
            {
                SDL.DestroyWindow(window);
            }
        }
        finally
        {
            SDL.Quit();
        }
    }

    private static bool RenderFileDialog(FileDialogState state, out string result)
    {
        result = null;
        var io = ImGui.GetIO();

        // Fullscreen window
        ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.None, Vector2.Zero);
        ImGui.SetNextWindowSize(io.DisplaySize, ImGuiCond.None);

        ImGui.Begin("##FileDialog", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);

        // Current path display
        ImGui.Text($"Location: {state.CurrentDirectory}");
        ImGui.Separator();

        // Navigation buttons
        if (ImGui.Button("Up"))
        {
            var parent = Directory.GetParent(state.CurrentDirectory);
            if (parent != null)
            {
                state.CurrentDirectory = parent.FullName;
                state.RefreshEntries();
            }
        }

        ImGui.SameLine(0, -1);

        if (ImGui.Button("Home"))
        {
            state.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            state.RefreshEntries();
        }

        ImGui.Separator();

        // File list
        ImGui.BeginChild("##FileList", new Vector2(0, -80), ImGuiChildFlags.Borders, ImGuiWindowFlags.None);

        foreach (var entry in state.Entries)
        {
            bool isDirectory = entry.IsDirectory;
            string displayName = isDirectory ? $"[DIR] {entry.Name}" : entry.Name;

            if (ImGui.Selectable(displayName, state.SelectedEntry == entry.FullPath, ImGuiSelectableFlags.AllowDoubleClick, Vector2.Zero))
            {
                state.SelectedEntry = entry.FullPath;

                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    if (isDirectory)
                    {
                        state.CurrentDirectory = entry.FullPath;
                        state.RefreshEntries();
                        state.SelectedEntry = null;
                    }
                    else if (state.Mode == FileDialogMode.OpenFile)
                    {
                        result = entry.FullPath;
                        ImGui.EndChild();
                        ImGui.End();
                        return true;
                    }
                }
            }
        }

        ImGui.EndChild();

        // Filename input for save mode
        if (state.Mode == FileDialogMode.SaveFile)
        {
            ImGui.Text("Filename:");
            ImGui.SameLine(0, -1);
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##Filename", ref state.FileName, 256, ImGuiInputTextFlags.None);
        }

        ImGui.Separator();

        // Action buttons
        string actionButtonText = state.Mode switch
        {
            FileDialogMode.OpenFile => "Open",
            FileDialogMode.SaveFile => "Save",
            FileDialogMode.SelectFolder => "Select",
            _ => "OK"
        };

        bool canConfirm = state.Mode switch
        {
            FileDialogMode.OpenFile => !string.IsNullOrEmpty(state.SelectedEntry) && File.Exists(state.SelectedEntry),
            FileDialogMode.SaveFile => !string.IsNullOrEmpty(state.FileName),
            FileDialogMode.SelectFolder => !string.IsNullOrEmpty(state.SelectedEntry) && Directory.Exists(state.SelectedEntry),
            _ => false
        };

        ImGui.BeginDisabled(!canConfirm);
        if (ImGui.Button(actionButtonText, new Vector2(120, 0)))
        {
            result = state.Mode switch
            {
                FileDialogMode.SaveFile => Path.Combine(state.CurrentDirectory, state.FileName),
                _ => state.SelectedEntry
            };
            ImGui.EndDisabled();
            ImGui.End();
            return true;
        }
        ImGui.EndDisabled();

        ImGui.SameLine(0, -1);

        if (ImGui.Button("Cancel", new Vector2(120, 0)))
        {
            result = null;
            ImGui.End();
            return true;
        }

        ImGui.End();
        return false;
    }
}
