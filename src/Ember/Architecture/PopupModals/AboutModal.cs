// // Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// // Licensed under the MIT license.
// // See LICENSE file in the project root for full license information.

// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using System.Reflection;
// using Ember.Graphics;
// using Hexa.NET.ImGui;
// using Microsoft.Xna.Framework.Graphics;
// using static Hexa.NET.ImGui.ImGui;
// using SysVec2 = System.Numerics.Vector2;
// using SysVec4 = System.Numerics.Vector4;

// namespace Ember.Architecture.PopupModals;

// /// <summary>
// /// Displays application information including version, copyright, and sponsor link.
// /// </summary>
// public sealed class AboutModal
// {
//     private static readonly Dictionary<object, AboutModal> s_instances = [];
//     private static readonly string s_version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
//     private static readonly string s_copyright = $"Copyright © {DateTime.Now.Year} Christopher Whitley and Contributors";
//     private const string GITHUB_SPONSOR_URL = "https://github.com/sponsors/aristurtledev";

//     private readonly EditorContext _context;
//     private Texture2D _iconTexture;
//     private ImTextureRef _iconTextureRef;
//     private bool _iconLoaded;

//     /// <summary>
//     /// Gets or creates an <see cref="AboutModal"/> instance for the specified requestor.
//     /// </summary>
//     /// <param name="requestor">The object requesting the modal.</param>
//     /// <returns>The <see cref="AboutModal"/> instance.</returns>
//     public static AboutModal GetAboutModal(object requestor)
//     {
//         ArgumentNullException.ThrowIfNull(requestor);

//         if (s_instances.TryGetValue(requestor, out AboutModal modal))
//         {
//             return modal;
//         }

//         modal = new AboutModal();
//         s_instances.Add(requestor, modal);
//         return modal;
//     }

//     /// <summary>
//     /// Removes the <see cref="AboutModal"/> instance associated with the specified requestor.
//     /// </summary>
//     /// <param name="requestor">The object that requested the modal.</param>
//     /// <returns><see langword="true"/> if the modal was removed; otherwise, <see langword="false"/>.</returns>
//     public static bool RemoveAboutModal(object requestor)
//     {
//         ArgumentNullException.ThrowIfNull(requestor);

//         if (!s_instances.TryGetValue(requestor, out AboutModal _))
//         {
//             return false;
//         }

//         return s_instances.Remove(requestor);
//     }

//     private static bool RemoveModal(AboutModal modal)
//     {
//         object requestor = null;

//         foreach (var kvp in s_instances)
//         {
//             if (kvp.Value == modal)
//             {
//                 requestor = kvp.Key;
//                 break;
//             }
//         }

//         return RemoveModal(requestor);
//     }

//     /// <summary>
//     /// Draws the about modal.
//     /// </summary>
//     public void Draw()
//     {
//         ImGuiStylePtr stylePtr = GetStyle();
//         SysVec2 textPadding = new(0, stylePtr.ItemSpacing.Y);

//         // Application name and version
//         PushFont(Fonts.Medium);
//         Text("Ember"u8);
//         PopFont();

//         TextDisabled($"Version {s_version}");
//         Dummy(textPadding);

//         // Description
//         TextWrapped("A particle effect editor for MonoGame Extended");
//         Dummy(textPadding);

//         // License
//         Text("Licensed under the MIT license");
//         Dummy(textPadding);

//         // Copyright
//         Text(s_copyright);
//         Dummy(textPadding);

//         Separator();
//         Dummy(textPadding);

//         // GitHub sponsor link with subtle styling
//         TextDisabled("If you find this tool useful:");
//         SameLine();

//         // Use button with link styling
//         PushStyleColor(ImGuiCol.Button, stylePtr.Colors[(int)ImGuiCol.WindowBg]);
//         PushStyleColor(ImGuiCol.ButtonHovered, stylePtr.Colors[(int)ImGuiCol.HeaderHovered]);
//         PushStyleColor(ImGuiCol.ButtonActive, stylePtr.Colors[(int)ImGuiCol.HeaderActive]);
//         PushStyleColor(ImGuiCol.Text, stylePtr.Colors[(int)ImGuiCol.Header]);

//         if (Button("Sponsor on GitHub"u8))
//         {
//             try
//             {
//                 Process.Start(new ProcessStartInfo
//                 {
//                     FileName = GITHUB_SPONSOR_URL,
//                     UseShellExecute = true
//                 });
//             }
//             catch
//             {
//                 // Fail silently if unable to open browser
//             }
//         }

//         PopStyleColor(4);

//         if (IsItemHovered())
//         {
//             SetMouseCursor(ImGuiMouseCursor.Hand);
//         }

//         Dummy(textPadding);
//         Separator();

//         // Close button
//         SysVec2 buttonSize = new(120.0f, 0);
//         float cursorX = GetCursorPosX() + GetContentRegionAvail().X - buttonSize.X;
//         SetCursorPosX(cursorX);

//         if (Button("Close"u8, buttonSize))
//         {
//             RemoveModal(this);
//             CloseCurrentPopup();
//         }
//     }
// }
