using System;
using System.IO;
using Ember.Architecture.PopupModals;
using Ember.Architecture.Services;
using Ember.Graphics;
using Hexa.NET.ImGui;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Particles;
using static Hexa.NET.ImGui.ImGui;

namespace Ember.Architecture.Views;

public sealed class ParticleEffectView
{
    public const string ViewName = "Particle Effect";

    private int _emitterDragFromIndex = -1;
    private int _emitterDragToIndex = -1;
    private bool _selectTexture;
    private string _pendingTextureFilePath = string.Empty;
    private bool _confirmOverwrite;

    private readonly IParticleEffectService _particleEffectService;
    private readonly IEmitterService _emitterService;
    private readonly ISelectionService _selectionService;
    private readonly IProjectService _projectService;
    private readonly ILockService _lockService;
    private readonly ITextureService _textureService;

    public ParticleEffectView(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _particleEffectService = services.GetService(typeof(IParticleEffectService)) as IParticleEffectService;
        _emitterService = services.GetService(typeof(IEmitterService)) as IEmitterService;
        _selectionService = services.GetService(typeof(ISelectionService)) as ISelectionService;
        _projectService = services.GetService(typeof(IProjectService)) as IProjectService;
        _lockService = services.GetService(typeof(ILockService)) as ILockService;
        _textureService = services.GetService(typeof(ITextureService)) as ITextureService;

    }

    public void Draw()
    {
        if (Begin(ViewName))
        {
            DrawParticleEffectProperties();
            DrawParticleEmitterList();
            DrawSelectedEmitterProperties();
        }
        End();

        DrawSelectTexturePopup();
        DrawOverwriteConfirmationPopup();
    }

    private void DrawParticleEffectProperties()
    {
        if (CollapsingHeader("Particle Effect Properties"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            if (BeginChild("##particle-effect-properties"u8, SysVec2.Zero, childFlags))
            {
                if (BeginTable("##particle-effect-properties-table"u8, columns: 2, ImGuiTableFlags.SizingStretchProp))
                {
                    TableSetupColumn("##particle-effect-property-label-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                    TableSetupColumn("##particle-effect-property-value-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

                    // Auto trigger property
                    TableNextRow();
                    TableNextColumn();
                    AlignTextToFramePadding();
                    Text("Auto Trigger"u8);

                    if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                    {
                        SetTooltip("Indicates whether this particle effect will automatically trigger its emitters"u8);
                    }

                    TableNextColumn();
                    bool autoTrigger = _particleEffectService.Current.AutoTrigger;
                    if (Checkbox("##particle-effect-auto-trigger"u8, ref autoTrigger))
                    {
                        _particleEffectService.Current.AutoTrigger = autoTrigger;
                        _projectService.HasUnsavedChanges = true;
                    }

                    // Auto Trigger Frequency
                    TableNextRow();
                    TableNextColumn();
                    AlignTextToFramePadding();
                    Text("Auto Trigger Frequency"u8);

                    if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                    {
                        SetTooltip("The frequency, in seconds, at which this particle effect automatically triggers emitters"u8);
                    }

                    TableNextColumn();
                    BeginDisabled(!_particleEffectService.Current.AutoTrigger);
                    SetNextItemWidth(-1);
                    float frequency = _particleEffectService.Current.AutoTriggerFrequency;
                    if (DragFloat("##particle-effect-auto-trigger-frequency"u8, ref frequency, 0.1f, 0.1f, float.MaxValue, "%.2f"u8))
                    {
                        _particleEffectService.Current.AutoTriggerFrequency = frequency;
                        _projectService.HasUnsavedChanges = true;
                    }
                    EndDisabled();
                    EndTable();
                }
            }
            EndChild();
        }
    }

    private unsafe void DrawParticleEmitterList()
    {
        if (CollapsingHeader("Particle Emitters"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            SysVec2 childWindowSize = new SysVec2(0.0f, 300.0f);

            // If there are no emitters, just display a child window with the
            // the text stating so and return back
            if (_particleEffectService.Current.Emitters.Count == 0)
            {
                if (BeginChild("##particle-emitter-list-child-window"u8, childWindowSize, childFlags))
                {
                    TextDisabled("No particle emitters added"u8);
                }
                EndChild();
                return;
            }

            if (BeginChild("##particle-emitter-list-child-window"u8, childWindowSize, childFlags))
            {
                float iconColumnWidth = 20.0f;
                ImGuiTableFlags tableFlags = ImGuiTableFlags.ScrollY
                                             | ImGuiTableFlags.RowBg
                                             | ImGuiTableFlags.SizingStretchProp;

                if (BeginTable("##particle-emitter-list"u8, columns: 4, tableFlags))
                {
                    TableSetupColumn("##particle-emitter-list-name-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                    TableSetupColumn("##particle-emitter-list-lock-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
                    TableSetupColumn("##particle-emitter-list-visibility-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
                    TableSetupColumn("##particle-emitter-list-delete-column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);

                    for (int i = 0; i < _particleEffectService.Current.Emitters.Count; i++)
                    {
                        TableNextRow();
                        PushID(i);

                        ParticleEmitter emitter = _particleEffectService.Current.Emitters[i];
                        bool isLocked = _lockService.IsLocked(emitter);
                        bool isSelected = emitter == _selectionService.SelectedEmitter;

                        // Name Column
                        TableNextColumn();
                        SysVec2 nameButtonSize = new SysVec2(-1, GetFrameHeight());
                        PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new SysVec2(0.0f, 0.5f));
                        if (Button(emitter.Name, nameButtonSize))
                        {
                            _selectionService.SelectEmitter(emitter, i);
                        }

                        if (BeginDragDropSource(ImGuiDragDropFlags.None))
                        {
                            int* indexPtr = &i;
                            SetDragDropPayload("emitter-reorder-payload"u8, &i, sizeof(int));
                            Text($"Moving: {emitter.Name}");
                            EndDragDropSource();
                        }

                        if (BeginDragDropTarget())
                        {
                            ImGuiPayloadPtr payloadPtr = AcceptDragDropPayload("emitter-reorder-payload"u8);
                            if (!payloadPtr.IsNull)
                            {
                                _emitterDragFromIndex = *(int*)payloadPtr.Data;
                                _emitterDragToIndex = i;
                            }

                            EndDragDropTarget();
                        }

                        PopStyleVar();

                        // Lock column
                        TableNextColumn();
                        Text(isLocked ? Fonts.LockIcon : Fonts.UnlockedIcon);
                        if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                        {
                            _lockService.ToggleLock(emitter);
                        }

                        // Visibility Column
                        TableNextColumn();
                        BeginDisabled(isLocked);
                        Text(emitter.Visible ? Fonts.VisibleIcon : Fonts.NotVisibleIcon);
                        if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                        {
                            emitter.Visible = !emitter.Visible;
                            _projectService.HasUnsavedChanges = true;
                        }
                        EndDisabled();

                        // Delete column
                        TableNextColumn();
                        BeginDisabled(isLocked);
                        Text(Fonts.DeleteIcon);
                        if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                        {
                            _emitterService.Remove(i);
                        }
                        EndDisabled();

                        // Reorder emitters if a drag/drop occured
                        if (_emitterDragFromIndex != -1 && _emitterDragToIndex != -1 && _emitterDragFromIndex != _emitterDragToIndex)
                        {
                            _emitterService.Reorder(_emitterDragFromIndex, _emitterDragToIndex);
                            _emitterDragFromIndex = -1;
                            _emitterDragToIndex = -1;
                        }

                        PopID();
                    }
                    EndTable();
                }
            }
            EndChild();
        }
    }

    private void DrawSelectedEmitterProperties()
    {
        if (CollapsingHeader("Particle Emitters"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            ParticleEmitter emitter = _selectionService.SelectedEmitter;



            // If there is no selected emitter, just display a child window with
            // the text stating so and return back.
            if (emitter == null)
            {
                if (BeginChild("##selected-emitter-properties-child-window"u8, SysVec2.Zero, childFlags))
                {
                    TextDisabled("No particle emitter selected"u8);
                }
                EndChild();
                return;
            }

            if (BeginChild("##selected-emitter-properties-child-window"u8, SysVec2.Zero, childFlags))
            {
                BeginDisabled(_lockService.IsLocked(emitter));
                if (BeginTable("##selected-emitter-properties-table"u8, columns: 2, ImGuiTableFlags.SizingStretchProp))
                {
                    TableSetupColumn("##selected-emitter-properties-label-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                    TableSetupColumn("##selected-emitter-properties-value-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

                    // Name Property
                    TableNextRow();
                    TableNextColumn();
                    AlignTextToFramePadding();
                    Text("Name"u8);
                    if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                    {
                        SetTooltip("The display name of the selected emitter");
                    }

                    TableNextColumn();
                    SetNextItemWidth(-1);
                    string emitterName = emitter.Name;
                    if (InputText("##selected-emitter-name-value"u8, ref emitterName, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
                    {
                        emitter.Name = emitterName;
                        _projectService.HasUnsavedChanges = true;
                    }

                    // Texture Property
                    TableNextRow();
                    TableNextColumn();
                    Text("Texture"u8);
                    if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                    {
                        SetTooltip("The texture used for the selected emitter"u8);
                    }

                    TableNextColumn();
                    if (emitter.TextureRegion is Texture2DRegion region)
                    {
                        string textureName = !string.IsNullOrEmpty(region.Name)
                                             ? region.Name
                                             : !string.IsNullOrEmpty(region.Texture.Name)
                                               ? region.Texture.Name
                                               : "Texture";

                        if (Button(textureName, -SysVec2.UnitX))
                        {
                            _selectTexture = true;
                        }

                        if (IsItemHovered())
                        {
                            if (BeginTooltip())
                            {
                                float maxPreviewSize = 256.0f;
                                float textureWidth = region.Texture.Width;
                                float textureHeight = region.Texture.Height;
                                float aspectRatio = textureWidth / textureHeight;

                                SysVec2 previewSize;
                                if (aspectRatio > 1.0f)
                                {
                                    previewSize = new SysVec2(maxPreviewSize, maxPreviewSize / aspectRatio);
                                }
                                else
                                {
                                    previewSize = new SysVec2(maxPreviewSize * aspectRatio, maxPreviewSize);
                                }

                                ImTextureRef textureId = ImGuiRenderer.BindTexture(region.Texture);
                                SysVec2 uv0 = new SysVec2()
                                {
                                    X = (float)region.Bounds.Left / region.Texture.Width,
                                    Y = (float)region.Bounds.Top / region.Texture.Height
                                };
                                SysVec2 uv1 = new SysVec2()
                                {
                                    X = (float)region.Bounds.Right / region.Texture.Width,
                                    Y = (float)region.Bounds.Bottom / region.Texture.Height
                                };

                                Image(textureId, previewSize, uv0, uv1);
                                Text($"{region.Width}x{region.Height}px");

                                EndTooltip();
                            }
                        }

                        // Source Rectangle Property
                        TableNextRow();
                        TableNextColumn();
                        AlignTextToFramePadding();
                        Text("Source Rectangle"u8);

                        TableNextColumn();
                        XnaRect bounds = region.Bounds;
                        XnaRect resetBounds = region.Texture.Bounds;
                        int[] source = [bounds.X, bounds.Y, bounds.Width, bounds.Height];

                        SetNextItemWidth(-1);
                        if (InputInt4("##selected-emitter-source-rectangle-value"u8, ref source[0], ImGuiInputTextFlags.None))
                        {
                            bounds.X = source[0];
                            bounds.Y = source[1];
                            bounds.Width = source[2];
                            bounds.Height = source[3];
                            emitter.TextureRegion = new Texture2DRegion(region.Texture, bounds, region.Name);
                            _projectService.HasUnsavedChanges = true;
                        }

                        TableNextRow();
                        TableNextColumn();
                        TableNextColumn();
                        if (Button("Reset source rectangle"u8, -SysVec2.UnitX))
                        {
                            emitter.TextureRegion = new Texture2DRegion(region.Texture, resetBounds, region.Name);
                            _projectService.HasUnsavedChanges = true;
                        }
                    }
                    else
                    {
                        if (Button("Select Texture", -SysVec2.UnitX))
                        {
                            _selectTexture = true;
                        }
                    }

                    // Capacity Property
                    TableNextRow();
                    TableNextColumn();
                    AlignTextToFramePadding();
                    Text("Capacity"u8);
                    if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                    {
                        SetTooltip("The maximum number of particles that this emitter can have active at a given time"u8);
                    }

                    TableNextColumn();
                    SetNextItemWidth(-1);
                    int emitterCapacity = emitter.Capacity;
                    if (InputInt("##selected-emitter-capacity-value"u8, ref emitterCapacity, 0, 0, ImGuiInputTextFlags.None))
                    {
                        emitter.ChangeCapacity(emitterCapacity);
                        _projectService.HasUnsavedChanges = true;
                    }

                    // Lifespan Property
                    TableNextRow();
                    TableNextColumn();
                    AlignTextToFramePadding();
                    Text("Lifespan"u8);
                    if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                    {
                        SetTooltip("The amount of time, in seconds, that each particle released from this emitter will live"u8);
                    }

                    TableNextColumn();
                    SetNextItemWidth(-1);
                    float emitterLifeSpan = emitter.LifeSpan;
                    if (DragFloat("##selected-emitter-lifespan-value"u8, ref emitterLifeSpan, 0.1f, 0.0f, float.MaxValue, "%.2f"))
                    {
                        emitter.LifeSpan = emitterLifeSpan;
                        _projectService.HasUnsavedChanges = true;
                    }

                    // Rendering Order Property
                    TableNextRow();
                    TableNextColumn();
                    AlignTextToFramePadding();
                    Text("Rendering Order"u8);
                    if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                    {
                        SetTooltip("The order in which the particles are rendered\n\n- Front To Back: Particles are rendered in front to back order\n- Back To Front: Particles are rendered in back to front order."u8);
                    }

                    TableNextColumn();
                    SetNextItemWidth(-1);
                    ReadOnlySpan<byte> renderingOrderPreview = emitter.RenderingOrder == ParticleRenderingOrder.FrontToBack
                                                               ? "Front To Back"u8
                                                               : "Back To Front"u8;

                    if (BeginCombo("##selected-emitter-rendering-order-value"u8, renderingOrderPreview))
                    {
                        // Front To Back
                        bool isSelected = emitter.RenderingOrder == ParticleRenderingOrder.FrontToBack;
                        if (Selectable("Front To Back"u8, isSelected))
                        {
                            emitter.RenderingOrder = ParticleRenderingOrder.FrontToBack;
                            _projectService.HasUnsavedChanges = true;
                        }

                        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                        {
                            SetTooltip("Particles are rendered from front to back"u8);
                        }

                        if (isSelected)
                        {
                            SetItemDefaultFocus();
                        }

                        // Back To Front
                        isSelected = emitter.RenderingOrder == ParticleRenderingOrder.BackToFront;
                        if (Selectable("Back To Front"u8, isSelected))
                        {
                            emitter.RenderingOrder = ParticleRenderingOrder.BackToFront;
                            _projectService.HasUnsavedChanges = true;
                        }

                        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                        {
                            SetTooltip("Particles are rendered from back to front"u8);
                        }

                        if (isSelected)
                        {
                            SetItemDefaultFocus();
                        }

                        EndCombo();
                    }

                    EndTable();
                }
                EndDisabled();
            }
            EndChild();
        }
    }

    private void DrawSelectTexturePopup()
    {
        // I really don't like this way of signalling to open the popup modal.
        // I'd like to find a better approach than storing a state from when
        // select texture is clicked in the main menu, then checking state here
        // and telling the popup to open and then changing state to false,
        // but because the modal needs to be **opened** and **rendered** outside
        // the scope of the child, here we are.
        if (_selectTexture)
        {
            OpenPopup("select-texture"u8);
            _selectTexture = false;
        }

        ImGuiViewportPtr viewportPtr = GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        SetNextWindowSize(viewportPtr.WorkSize * 0.9f, ImGuiCond.Appearing);
        SetNextWindowSizeConstraints(new SysVec2(600, 500), viewportPtr.WorkSize * 0.9f);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoMove
                                      | ImGuiWindowFlags.NoTitleBar;

        if (BeginPopupModal("select-texture"u8, modalFlags))
        {
            FileDialog dialog = FileDialog.GetFileDialog(this, null, ".png");
            if (dialog.Draw())
            {
                string fileName = Path.GetFileName(dialog.SelectedItem.FullName);

                // Check if this texture already exists in the project
                if (_textureService.TextureExists(fileName))
                {
                    _pendingTextureFilePath = dialog.SelectedItem.FullName;
                    _confirmOverwrite = true;
                }
                else
                {
                    // No conflict, add directly
                    _textureService.AddTexture(dialog.SelectedItem.FullName);
                    AssignTextureToSelectedEmitter(fileName);
                }

            }
            EndPopup();
        }
    }

    private void DrawOverwriteConfirmationPopup()
    {
        if (_confirmOverwrite)
        {
            OpenPopup("confirm-overwrite"u8);
            _confirmOverwrite = false;
        }

        ImGuiViewportPtr viewportPtr = GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        SetNextWindowSizeConstraints(new SysVec2(400, 0), viewportPtr.WorkSize);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.AlwaysAutoResize
                                      | ImGuiWindowFlags.NoResize
                                      | ImGuiWindowFlags.NoCollapse
                                      | ImGuiWindowFlags.NoMove;

        if (BeginPopupModal("confirm-overwrite"u8, modalFlags))
        {
            string fileName = Path.GetFileName(_pendingTextureFilePath);
            Text($"A texture named '{fileName}' already exists.");
            Spacing();
            Text("Do you want to overwrite it?"u8);
            Spacing();
            Spacing();

            ImGuiStylePtr stylePtr = GetStyle();
            SysVec2 buttonSize = new SysVec2(100.0f, 0);
            float widthNeeded = (buttonSize.X * 2) + stylePtr.ItemSpacing.X;
            float cursorX = GetCursorPosX() + GetContentRegionAvail().X - widthNeeded;
            SetCursorPosX(cursorX);

            if (Button("Yes"u8, buttonSize))
            {
                _textureService.AddTexture(_pendingTextureFilePath, overwrite: true);
                AssignTextureToSelectedEmitter(fileName);
                _pendingTextureFilePath = null;
                CloseCurrentPopup();
            }

            SameLine();
            if (Button("No"u8, buttonSize))
            {
                _pendingTextureFilePath = null;
                CloseCurrentPopup();
            }

            EndPopup();
        }
    }

    private void AssignTextureToSelectedEmitter(string fileName)
    {
        ParticleEmitter emitter = _selectionService.SelectedEmitter;
        if (emitter == null)
        {
            return;
        }

        var texture = _textureService.GetTexture(fileName);
        if (texture != null)
        {
            emitter.TextureRegion = new Texture2DRegion(texture, texture.Bounds, fileName);
            _projectService.HasUnsavedChanges = true;
        }
    }
}
