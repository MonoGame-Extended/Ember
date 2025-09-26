// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ember.Graphics;
using Ember.UI.Modals;
using Hexa.NET.ImGui;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Particles;

namespace Ember.UI.ChildWindows;

public static class SelectedEmitterPropertiesChildWindow
{
    private static readonly Dictionary<ParticleRenderingOrder, DisplayAttribute> s_renderingOrders = [];

    static SelectedEmitterPropertiesChildWindow()
    {
        // TODO: Hardcoding these for now. What I'd like to eventually do is move this to using the actual
        //       DisplayAttribute on the classes in MGE and then use reflection once to read them here and
        //       store them.  Then ultimately do the same thing for all class, properties, and fields throughout
        //       the particle system implementation in MGE, so that it could allow users to easily add new
        //       types from their own libraries.  Need to really think on this design more though before
        //       implementing it since it's going to rely on reflection further down in the actual draw calls.

        s_renderingOrders[ParticleRenderingOrder.FrontToBack] = new DisplayAttribute() { Name = nameof(SR.RenderingOrder_FrontToBack_Name), Description = nameof(SR.RenderingOrder_FrontToBack_Description) };
        s_renderingOrders[ParticleRenderingOrder.BackToFront] = new DisplayAttribute() { Name = nameof(SR.RenderingOrder_BackToFront_Name), Description = nameof(SR.RenderingOrder_BackToFront_Description) };
    }

    public static void Draw()
    {
        if (ImGui.CollapsingHeader(SR.CollapsingHeader_SelectedEmitterProperties, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY;

            if (ImGui.BeginChild("##selected_emitter_properties_child_window"u8, SysVec2.Zero, childFlags))
            {
                DrawContent();
            }
            ImGui.EndChild();
        }
    }

    private static void DrawContent()
    {
        if (EmberContext.SelectedParticleEmitter is not ParticleEmitter emitter)
        {
            ImGui.TextDisabled(SR.Message_NoParticleEmitterSelected);
            return;
        }

        ImGui.BeginDisabled(EmberContext.IsLocked(emitter));
        if (ImGui.BeginTable("##selected_emitter_properties_table"u8, 2, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("##selected_emitter_property_label_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##selected_emitter_property_value_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

            DrawSelectedEmitterNamePropertyRow(emitter);
            DrawSelectedEmitterTexturePropertyRow(emitter);
            DrawSelectedEmitterTextureSourceRectanglePropertyRow(emitter);
            DrawSelectedEmitterCapacityPropertyRow(emitter);
            DrawSelectedEmitterLifespanPropertyRow(emitter);
            DrawSelectedEmitterRenderingOrderPropertyRow(emitter);

            ImGui.EndTable();
        }
        ImGui.EndDisabled();
    }

    private static void DrawSelectedEmitterNamePropertyRow(ParticleEmitter emitter)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.ParticleEmitter_Property_Name_Name);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(SR.ParticleEmitter_Property_Name_Description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        string emitterName = emitter.Name;
        if (ImGui.InputText("##selected_emitter_name_value"u8, ref emitterName, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
        {
            emitter.Name = emitterName;
            EmberContext.HasUnsavedChanges = true;
        }
    }

    private static void DrawSelectedEmitterTexturePropertyRow(ParticleEmitter emitter)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.ParticleEmitter_Property_Texture_Name);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(SR.ParticleEmitter_Property_Texture_Description);
        }

        ImGui.TableNextColumn();

        if (emitter.TextureRegion is Texture2DRegion region)
        {
            // Get texture name - use the region's name, texture's name, or fallback to "Texture"
            string textureName = !string.IsNullOrEmpty(region.Name) ? region.Name
                               : !string.IsNullOrEmpty(region.Texture.Name) ? region.Texture.Name
                               : "Texture";

            // Show button with texture name
            if (ImGui.Button(textureName, new SysVec2(-1, 0)))
            {
                FileBrowserModal.OpenTextureSelector(EmberContext.ProjectDirectory, result =>
                {
                    if (result.Status == ModalResult.Success)
                    {
                        EmberContext.AddTexture(result.SelectedItem.FullName);
                    }
                });
            }

            // Show tooltip with image preview when hovering
            if (ImGui.IsItemHovered())
            {
                if (ImGui.BeginTooltip())
                {
                    // Calculate preview size while maintaining aspect ratio
                    float maxPreviewSize = 256.0f; // Maximum size for the tooltip preview
                    float textureWidth = region.Texture.Width;
                    float textureHeight = region.Texture.Height;
                    float aspectRatio = textureWidth / textureHeight;

                    SysVec2 previewSize;
                    if (aspectRatio > 1.0f) // Wider than tall
                    {
                        previewSize = new SysVec2(maxPreviewSize, maxPreviewSize / aspectRatio);
                    }
                    else // Taller than wide or square
                    {
                        previewSize = new SysVec2(maxPreviewSize * aspectRatio, maxPreviewSize);
                    }

                    // Bind texture for ImGui rendering
                    ImTextureRef textureId = ImGuiRenderer.BindTexture(region.Texture);

                    // Calculate UV coordinates for the texture region
                    SysVec2 uv0 = new(
                        (float)region.Bounds.Left / region.Texture.Width,
                        (float)region.Bounds.Top / region.Texture.Height
                    );

                    SysVec2 uv1 = new(
                        (float)region.Bounds.Right / region.Texture.Width,
                        (float)region.Bounds.Bottom / region.Texture.Height
                    );

                    // Display the texture preview
                    ImGui.Image(textureId, previewSize, uv0, uv1);

                    // Optional: Show texture dimensions info
                    ImGui.Text($"{region.Width}x{region.Height}px");

                    ImGui.EndTooltip();
                }
            }
        }
        else
        {
            // No texture is assigned, show placeholder button
            if (ImGui.Button(SR.Button_SelectTexture, new SysVec2(-1, 0)))
            {
                FileBrowserModal.OpenTextureSelector(EmberContext.ProjectDirectory, result =>
                {
                    if (result.Status == ModalResult.Success)
                    {
                        EmberContext.AddTexture(result.SelectedItem.FullName);
                    }
                });
            }
        }
    }

    private static void DrawSelectedEmitterTextureSourceRectanglePropertyRow(ParticleEmitter emitter)
    {
        if (emitter.TextureRegion is not Texture2DRegion textureRegion)
        {
            return;
        }

        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.ParticleEmitter_Property_Texture_SourceRectangle_Name);

        ImGui.TableNextColumn();

        XnaRect bounds = textureRegion.Bounds;
        XnaRect resetBounds = textureRegion.Texture.Bounds;
        int[] source = [bounds.X, bounds.Y, bounds.Width, bounds.Height];

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputInt4("##emitter_source_rectangle_value"u8, ref source[0], ImGuiInputTextFlags.None))
        {
            bounds.X = source[0];
            bounds.Y = source[1];
            bounds.Width = source[2];
            bounds.Height = source[3];
            emitter.TextureRegion = new Texture2DRegion(textureRegion.Texture, bounds, textureRegion.Name);

            EmberContext.HasUnsavedChanges = true;
        }

        ImGui.TableNextRow();

        ImGui.TableNextColumn();

        ImGui.TableNextColumn();
        if (ImGui.Button(SR.Button_ResetSourceRectangle, new SysVec2(-1, 0)))
        {
            emitter.TextureRegion = new Texture2DRegion(textureRegion.Texture, resetBounds, textureRegion.Name);

            EmberContext.HasUnsavedChanges = true;
        }
    }

    private static void DrawSelectedEmitterCapacityPropertyRow(ParticleEmitter emitter)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.ParticleEmitter_Property_Capacity_Name);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(SR.ParticleEmitter_Property_Capacity_Description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        int value = emitter.Capacity;
        if (ImGui.InputInt("##emitter_capacity_property_value"u8, ref value, 0, 0, ImGuiInputTextFlags.None))
        {
            emitter.ChangeCapacity(value);
            EmberContext.HasUnsavedChanges = true;
        }
    }

    private static void DrawSelectedEmitterLifespanPropertyRow(ParticleEmitter emitter)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.ParticleEmitter_Property_Lifespan_Name);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(SR.ParticleEmitter_Property_Lifespan_Description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        float emitterLifeSpan = emitter.LifeSpan;
        if (ImGui.DragFloat("##emitter_lifespan_property_value"u8, ref emitterLifeSpan, 0.1f, 0.0f, float.MaxValue, "%.2f"))
        {
            emitter.LifeSpan = emitterLifeSpan;
            EmberContext.HasUnsavedChanges = true;
        }
    }

    private static void DrawSelectedEmitterRenderingOrderPropertyRow(ParticleEmitter emitter)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.ParticleEmitter_Property_RenderingOrder_Name);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(SR.ParticleEmitter_Property_RenderingOrder_Description);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ReadOnlySpan<byte> preview = SR.GetResourceUtf8Bytes(s_renderingOrders[emitter.RenderingOrder].Name);
        if (ImGui.BeginCombo("##selected_emitter_rendering_order_value"u8, preview))
        {
            foreach (var kvp in s_renderingOrders)
            {
                ParticleRenderingOrder renderingOrder = kvp.Key;
                DisplayAttribute display = kvp.Value;

                bool isSelected = emitter.RenderingOrder == renderingOrder;

                if (ImGui.Selectable(SR.GetResourceUtf8Bytes(display.Name), isSelected))
                {
                    emitter.RenderingOrder = renderingOrder;
                    EmberContext.HasUnsavedChanges = true;
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                {
                    ImGui.SetTooltip(SR.GetResourceUtf8Bytes(display.Description));
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }
    }
}
