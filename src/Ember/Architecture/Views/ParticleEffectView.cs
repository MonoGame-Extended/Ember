using System;
using System.IO;
using Ember.Architecture.PopupModals;
using Ember.Graphics;
using Hexa.NET.ImGui;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Data;
using MonoGame.Extended.Particles.Profiles;
using static Hexa.NET.ImGui.ImGui;

namespace Ember.Architecture.Views;

public sealed class ParticleEffectView
{
    public const string ViewName = "Particle Effect";

    private readonly EditorContext _context;

    private int _emitterDragFromIndex = -1;
    private int _emitterDragToIndex = -1;
    private bool _selectTexture;
    private string _pendingTextureFilePath = string.Empty;
    private bool _confirmOverwrite;
    private HslColor _userFromColor;
    private HslColor _userToColor;
    private bool _isColorReleaseParameterInitialized;
    private ParticleEmitter _lastSelectedEmitter;


    public ParticleEffectView(EditorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public void Draw()
    {
        if (_context.ParticleEffect == null)
        {
            return;
        }
        if (Begin(ViewName))
        {
            DrawParticleEffectProperties();
            DrawParticleEmitterList();
            DrawSelectedEmitterProperties();
            DrawSelectedEmitterProfile();
            DrawSelectedEmitterReleaseParameters();
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
                    bool autoTrigger = _context.ParticleEffect.AutoTrigger;
                    if (Checkbox("##particle-effect-auto-trigger"u8, ref autoTrigger))
                    {
                        _context.ParticleEffect.AutoTrigger = autoTrigger;
                        _context.HasUnsavedChanges = true;
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
                    BeginDisabled(!_context.ParticleEffect.AutoTrigger);
                    SetNextItemWidth(-1);
                    float frequency = _context.ParticleEffect.AutoTriggerFrequency;
                    if (DragFloat("##particle-effect-auto-trigger-frequency"u8, ref frequency, 0.1f, 0.1f, float.MaxValue, "%.2f"u8))
                    {
                        _context.ParticleEffect.AutoTriggerFrequency = frequency;
                        _context.HasUnsavedChanges = true;
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

            if (Button(SR.Button_AddNewEmitter, new SysVec2(-1, 0)))
            {
                _context.AddEmitter();
            }

            // If there are no emitters, just display a child window with the
            // the text stating so and return back
            if (_context.ParticleEffect.Emitters.Count == 0)
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

                    for (int i = 0; i < _context.ParticleEffect.Emitters.Count; i++)
                    {
                        TableNextRow();
                        PushID(i);

                        ParticleEmitter emitter = _context.ParticleEffect.Emitters[i];
                        bool isLocked = _context.IsLocked(emitter);
                        bool isSelected = emitter == _context.SelectedEmitter;

                        // Name Column
                        TableNextColumn();
                        SysVec2 nameButtonSize = new SysVec2(-1, GetFrameHeight());
                        PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new SysVec2(0.0f, 0.5f));
                        if (Button(emitter.Name, nameButtonSize))
                        {
                            _context.SelectEmitter(i);
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
                            _context.ToggleLock(emitter);
                        }

                        // Visibility Column
                        TableNextColumn();
                        BeginDisabled(isLocked);
                        Text(emitter.Visible ? Fonts.VisibleIcon : Fonts.NotVisibleIcon);
                        if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                        {
                            emitter.Visible = !emitter.Visible;
                            _context.HasUnsavedChanges = true;
                        }
                        EndDisabled();

                        // Delete column
                        TableNextColumn();
                        BeginDisabled(isLocked);
                        Text(Fonts.DeleteIcon);
                        if (IsItemHovered() && IsItemClicked(ImGuiMouseButton.Left))
                        {
                            _context.RemoveEmitter(i);
                        }
                        EndDisabled();

                        // Reorder emitters if a drag/drop occured
                        if (_emitterDragFromIndex != -1 && _emitterDragToIndex != -1 && _emitterDragFromIndex != _emitterDragToIndex)
                        {
                            _context.ReorderEmitters(_emitterDragFromIndex, _emitterDragToIndex);
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
        if (CollapsingHeader("Emitter Properties"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            ParticleEmitter emitter = _context.SelectedEmitter;



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
                BeginDisabled(_context.IsLocked(emitter));
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
                        _context.HasUnsavedChanges = true;
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
                            _context.HasUnsavedChanges = true;
                        }

                        TableNextRow();
                        TableNextColumn();
                        TableNextColumn();
                        if (Button("Reset source rectangle"u8, -SysVec2.UnitX))
                        {
                            emitter.TextureRegion = new Texture2DRegion(region.Texture, resetBounds, region.Name);
                            _context.HasUnsavedChanges = true;
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
                        _context.HasUnsavedChanges = true;
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
                        _context.HasUnsavedChanges = true;
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
                            _context.HasUnsavedChanges = true;
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
                            _context.HasUnsavedChanges = true;
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

    private void DrawSelectedEmitterProfile()
    {
        if (CollapsingHeader("Emitter Profile"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            ParticleEmitter emitter = _context.SelectedEmitter;



            // If there is no selected emitter, just display a child window with
            // the text stating so and return back.
            if (emitter == null)
            {
                if (BeginChild("##selected-emitter-profile-child-window"u8, SysVec2.Zero, childFlags))
                {
                    TextDisabled("No particle emitter selected"u8);
                }
                EndChild();
                return;
            }

            if (BeginChild("##selected-emitter-profile-child-window"u8, SysVec2.Zero, childFlags))
            {
                BeginDisabled(_context.IsLocked(emitter));

                if (BeginTable("##selected-emitter-profile-properties-table"u8, columns: 2, ImGuiTableFlags.SizingStretchProp))
                {
                    TableSetupColumn("##selected-emitter-profile-properties-label-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                    TableSetupColumn("##selected-emitter-profile-properties-value-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

                    // Profile Type Row
                    TableNextRow();
                    TableNextColumn();
                    AlignTextToFramePadding();
                    Text("Profile Type"u8);
                    if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                    {
                        SetTooltip("Profiles define the emission pattern, such as points, lines, rings, or areas from which particles originate"u8);
                    }

                    TableNextColumn();
                    SetNextItemWidth(-1);
                    ReadOnlySpan<byte> profileTypePreview = emitter.Profile switch
                    {
                        BoxFillProfile => "Box Fill"u8,
                        BoxProfile => "Box"u8,
                        BoxUniformProfile => "Box Uniform"u8,
                        CircleProfile => "Circle"u8,
                        LineProfile => "Line"u8,
                        PointProfile => "Point"u8,
                        RingProfile => "Ring"u8,
                        SprayProfile => "Spray"u8,
                        _ => throw new InvalidOperationException($"Unknown profile type '{emitter.Profile.GetType()}")
                    };
                    if (BeginCombo("##selected-emitter-profile-type"u8, profileTypePreview))
                    {
                        AddProfileComboboxItem(typeof(BoxFillProfile), "Box Fill"u8, "Randomly distributes particles throughout a rectangular area"u8, emitter);
                        AddProfileComboboxItem(typeof(BoxProfile), "Box"u8, "Distributes particles along the edges of a rectangular boundary"u8, emitter);
                        AddProfileComboboxItem(typeof(BoxUniformProfile), "Box Uniform"u8, "Distributes particles along the edges of a rectangular boundary with uniform density"u8, emitter);
                        AddProfileComboboxItem(typeof(CircleProfile), "Circle"u8, "Distributes particles throughout a circular area with controllable radiation patterns"u8, emitter);
                        AddProfileComboboxItem(typeof(LineProfile), "Line"u8, "Distributes particles uniformly along a line segment with random headings"u8, emitter);
                        AddProfileComboboxItem(typeof(PointProfile), "Point"u8, "Emits all particles from a single point with random headings"u8, emitter);
                        AddProfileComboboxItem(typeof(RingProfile), "Ring"u8, "Distributes particles along the perimeter of a circle with controllable radiation patterns"u8, emitter);
                        AddProfileComboboxItem(typeof(SprayProfile), "Spray"u8, "Emits particles from a single point in a directional cone pattern"u8, emitter);

                        EndCombo();
                    }

                    // Profile Properties Row(s)
                    switch (emitter.Profile)
                    {
                        case BoxFillProfile boxFill:
                            float boxFillWidth = boxFill.Width;
                            if (DrawProfileFloatProperty("Width"u8, "##box-fill-width"u8, ref boxFillWidth, 0.1f, 0.0f, float.MaxValue))
                            {
                                boxFill.Width = boxFillWidth;
                                _context.HasUnsavedChanges = true;
                            }

                            float boxFillHeight = boxFill.Height;
                            if (DrawProfileFloatProperty("Height"u8, "##box-fill-height"u8, ref boxFillHeight, 0.1f, 0.0f, float.MaxValue))
                            {
                                boxFill.Height = boxFillHeight;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case BoxProfile box:
                            float boxWidth = box.Width;
                            if (DrawProfileFloatProperty("Width"u8, "##box-width"u8, ref boxWidth, 0.1f, 0.0f, float.MaxValue))
                            {
                                box.Width = boxWidth;
                                _context.HasUnsavedChanges = true;
                            }

                            float boxHeight = box.Height;
                            if (DrawProfileFloatProperty("Height"u8, "##box-height"u8, ref boxHeight, 0.1f, 0.0f, float.MaxValue))
                            {
                                box.Height = boxHeight;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case BoxUniformProfile boxUniform:
                            float boxUniformWidth = boxUniform.Width;
                            if (DrawProfileFloatProperty("Width"u8, "##box-uniform-width"u8, ref boxUniformWidth, 0.1f, 0.0f, float.MaxValue))
                            {
                                boxUniform.Width = boxUniformWidth;
                                _context.HasUnsavedChanges = true;
                            }

                            float boxUniformHeight = boxUniform.Height;
                            if (DrawProfileFloatProperty("Height"u8, "##box-uniform-height"u8, ref boxUniformHeight, 0.1f, 0.0f, float.MaxValue))
                            {
                                boxUniform.Height = boxUniformHeight;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case CircleProfile circle:
                            float circleRadius = circle.Radius;
                            if (DrawProfileFloatProperty("Radius"u8, "##circle-radius"u8, ref circleRadius, 0.1f, 0.0f, float.MaxValue))
                            {
                                circle.Radius = circleRadius;
                                _context.HasUnsavedChanges = true;
                            }

                            CircleRadiation circleRadiate = circle.Radiate;
                            if (DrawProfileCircleRadiationProperty("Radiate"u8, "##circle-radiate"u8, ref circleRadiate))
                            {
                                circle.Radiate = circleRadiate;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case LineProfile line:
                            XnaVec2 lineAxis = line.Axis;
                            if (DrawProfileVector2Property("Axis"u8, "##line-axis"u8, ref lineAxis, 1f, -1f, 1f))
                            {
                                line.Axis = lineAxis;
                                _context.HasUnsavedChanges = true;
                            }

                            float lineLength = line.Length;
                            if (DrawProfileFloatProperty("Length"u8, "##line-length"u8, ref lineLength, 0.1f, 0.0f, float.MaxValue))
                            {
                                line.Length = lineLength;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case PointProfile point:
                            // No additional properties
                            break;

                        case RingProfile ring:
                            float ringRadius = ring.Radius;
                            if (DrawProfileFloatProperty("Radius"u8, "##ring-radius"u8, ref ringRadius, 0.1f, 0.0f, float.MaxValue))
                            {
                                ring.Radius = ringRadius;
                                _context.HasUnsavedChanges = true;
                            }

                            CircleRadiation ringRadiate = ring.Radiate;
                            if (DrawProfileCircleRadiationProperty("Radiate"u8, "##ring-radiate"u8, ref ringRadiate))
                            {
                                ring.Radiate = ringRadiate;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case SprayProfile spray:
                            XnaVec2 sprayDirection = spray.Direction;
                            if (DrawProfileVector2Property("Direction"u8, "##spray-direction"u8, ref sprayDirection, 0.1f, float.MinValue, float.MaxValue))
                            {
                                spray.Direction = sprayDirection;
                                _context.HasUnsavedChanges = true;
                            }

                            float spraySpread = spray.Spread;
                            if (DrawProfileFloatProperty("Spread"u8, "##spray-spread"u8, ref spraySpread, 0.1f, 0.0f, float.MaxValue))
                            {
                                spray.Spread = spraySpread;
                                _context.HasUnsavedChanges = true;
                            }
                            break;
                    }
                    EndTable();
                }

                EndDisabled();
            }
            EndChild();
        }
    }

    private void AddProfileComboboxItem(Type profileType, ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, ParticleEmitter emitter)
    {
        bool isSelected = _context.SelectedEmitter.Profile.GetType() == profileType;
        if (Selectable(label, isSelected))
        {
            emitter.Profile = Activator.CreateInstance(profileType) as Profile;
            _context.HasUnsavedChanges = true;
        }
        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            SetTooltip(tooltip);
        }
        if (isSelected)
        {
            SetItemDefaultFocus();
        }
    }

    private bool DrawProfileFloatProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> id, ref float value, float step, float min, float max)
    {
        TableNextRow();
        TableNextColumn();
        AlignTextToFramePadding();
        Text(label);
        TableNextColumn();
        SetNextItemWidth(-1);
        return DragFloat(id, ref value, step, min, max, "%.2f"u8);
    }

    private bool DrawProfileVector2Property(ReadOnlySpan<byte> label, ReadOnlySpan<byte> id, ref XnaVec2 value, float step, float min, float max)
    {
        bool result = false;

        ImGuiStylePtr stylePtr = GetStyle();

        TableNextRow();
        TableNextColumn();
        AlignTextToFramePadding();
        Text(label);

        TableNextColumn();

        float availWidth = GetContentRegionAvail().X;
        float itemSpacingWidth = stylePtr.ItemSpacing.X;
        float dragWidth = (availWidth - itemSpacingWidth) * 0.5f;
        SetNextItemWidth(dragWidth);

        PushID(label);
        if (DragFloat("##x"u8, ref value.X, step, min, max, "X: %.2f"u8))
        {
            result = true;
        }

        SameLine();
        SetNextItemWidth(dragWidth);
        if (DragFloat("##y"u8, ref value.Y, step, min, max, "Y: %.2f"u8))
        {
            result = true;
        }
        PopID();

        return result;
    }

    private bool DrawProfileCircleRadiationProperty(ReadOnlySpan<byte> label, ReadOnlySpan<byte> id, ref CircleRadiation value)
    {
        bool result = false;

        TableNextRow();
        TableNextColumn();
        AlignTextToFramePadding();
        Text(label);

        TableNextColumn();
        SetNextItemWidth(-1);
        ReadOnlySpan<byte> circleRadiationPreview = value switch
        {
            CircleRadiation.None => "None"u8,
            CircleRadiation.In => "In"u8,
            CircleRadiation.Out => "Out"u8,
            _ => throw new InvalidOperationException($"Unknown circle radiation type '{value}'")
        };
        if (BeginCombo(id, circleRadiationPreview))
        {
            AddCircleRadiationComboboxItem(CircleRadiation.None, ref value, ref result, "None"u8, "Particles move in random directions unrelated to their positions"u8);
            AddCircleRadiationComboboxItem(CircleRadiation.In, ref value, ref result, "In"u8, "Particles move toward the center of the circle"u8);
            AddCircleRadiationComboboxItem(CircleRadiation.Out, ref value, ref result, "Out"u8, "Particles move away from the center of the circle"u8);

            EndCombo();
        }

        return result;
    }

    private void AddCircleRadiationComboboxItem(CircleRadiation enumValue, ref CircleRadiation value, ref bool changed, ReadOnlySpan<byte> label, ReadOnlySpan<byte> description)
    {
        bool isSelected = enumValue == value;
        if (Selectable(label, isSelected))
        {
            value = enumValue;
            changed = true;
        }
        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            SetTooltip(description);
        }
        if (isSelected)
        {
            SetItemDefaultFocus();
        }
    }

    private void DrawSelectedEmitterReleaseParameters()
    {
        if (CollapsingHeader("Emitter Release Parameters"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            ParticleEmitter emitter = _context.SelectedEmitter;

            if (emitter != _lastSelectedEmitter)
            {
                _isColorReleaseParameterInitialized = false;
                _lastSelectedEmitter = emitter;
            }

            // If there is no selected emitter, just display a child window with
            // the text stating so and return back.
            if (emitter == null)
            {
                if (BeginChild("##selected-emitter-release-parameters-child-window"u8, SysVec2.Zero, childFlags))
                {
                    TextDisabled("No particle emitter selected"u8);
                }
                EndChild();
                return;
            }

            if (BeginChild("##selected-emitter-release-parameters-child-window"u8, SysVec2.Zero, childFlags))
            {
                BeginDisabled(_context.IsLocked(emitter));

                if (BeginTable("##selected-emitter-release-parameters-table"u8, columns: 3, ImGuiTableFlags.SizingStretchProp))
                {
                    TableSetupColumn("##selected-emitter-release-parameters-label-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                    TableSetupColumn("##selected-emitter-release-parameters-kind-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
                    TableSetupColumn("##selected-emitter-release-parameters-value-column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);

                    ParticleInt32Parameter quantity = emitter.Parameters.Quantity;
                    if (DrawEmitterReleaseParameterRow("Quantity"u8, SR.ReleaseParameter_Quantity_Description, ref quantity))
                    {
                        emitter.Parameters.Quantity = quantity;
                        _context.HasUnsavedChanges = true;
                    }

                    ParticleFloatParameter speed = emitter.Parameters.Speed;
                    if (DrawEmitterReleaseParameterRow("Speed"u8, SR.ReleaseParameter_Speed_Description, ref speed))
                    {
                        emitter.Parameters.Speed = speed;
                        _context.HasUnsavedChanges = true;
                    }

                    ParticleColorParameter color = emitter.Parameters.Color;
                    if (DrawEmitterReleaseParameterRow("Color"u8, "The color of particles in HSL format"u8, ref color))
                    {
                        emitter.Parameters.Color = color;
                        _context.HasUnsavedChanges = true;
                    }

                    ParticleFloatParameter opacity = emitter.Parameters.Opacity;
                    if (DrawEmitterReleaseParameterRow("Opacity"u8, "The transparency of particles (0.0 = transparent, 1.0 = opaque)"u8, ref opacity))
                    {
                        emitter.Parameters.Opacity = opacity;
                        _context.HasUnsavedChanges = true;
                    }

                    ParticleVector2Parameter scale = emitter.Parameters.Scale;
                    if (DrawEmitterReleaseParameterRow("Scale"u8, "The size multiplier of particles"u8, ref scale))
                    {
                        emitter.Parameters.Scale = scale;
                        _context.HasUnsavedChanges = true;
                    }

                    ParticleFloatParameter rotation = emitter.Parameters.Rotation;
                    if (DrawEmitterReleaseParameterRow("Rotation"u8, "The initial rotation angle of particles in radians"u8, ref rotation))
                    {
                        emitter.Parameters.Rotation = rotation;
                        _context.HasUnsavedChanges = true;
                    }

                    ParticleFloatParameter mass = emitter.Parameters.Mass;
                    if (DrawEmitterReleaseParameterRow("Mass"u8, "The mass of particles (affects physics interactions)"u8, ref mass))
                    {
                        emitter.Parameters.Mass = mass;
                        _context.HasUnsavedChanges = true;
                    }

                    EndTable();
                }

                EndDisabled();
            }
            EndChild();
        }
    }

    private static void DrawParticleValueKindComboBox(ref ParticleValueKind kindValue, ref bool changed)
    {
        ReadOnlySpan<byte> kindPreview = kindValue switch
        {
            ParticleValueKind.Constant => "Constant"u8,
            ParticleValueKind.Random => "Random"u8,
            _ => throw new InvalidOperationException($"Unknown particle value kind '{kindValue}'")
        };

        if (BeginCombo("##kind"u8, kindPreview))
        {
            AddParticleValueKindComboBoxItem(ParticleValueKind.Constant, ref kindValue, ref changed, "Constant"u8, "All particles will be released with the same value for this property"u8);
            AddParticleValueKindComboBoxItem(ParticleValueKind.Random, ref kindValue, ref changed, "Random"u8, "Each particle will be released with a unique random value within the defined minimum and maximum bounds"u8);

            EndCombo();
        }
    }

    private static void AddParticleValueKindComboBoxItem(ParticleValueKind kind, ref ParticleValueKind kindValue, ref bool changed, ReadOnlySpan<byte> label, ReadOnlySpan<byte> description)
    {
        bool isSelected = kind == kindValue;
        if (Selectable(label, isSelected))
        {
            kindValue = kind;
            changed = true;
        }
        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            SetTooltip(description);
        }
        if (isSelected)
        {
            SetItemDefaultFocus();
        }
    }

    private static bool DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref ParticleInt32Parameter parameter)
    {
        bool changed = false;

        PushID(label);
        ImGuiStylePtr style = GetStyle();

        TableNextRow();
        TableNextColumn();
        AlignTextToFramePadding();
        Text(label);

        if (IsItemHovered())
        {
            SetTooltip(description);
        }

        TableNextColumn();
        SetNextItemWidth(-1);
        DrawParticleValueKindComboBox(ref parameter.Kind, ref changed);

        if (parameter.Kind == ParticleValueKind.Constant)
        {
            TableNextColumn();
            SetNextItemWidth(-1);
            int constant = parameter.Constant;
            if (DragInt("##constant"u8, ref parameter.Constant, 1, 0, int.MaxValue))
            {
                parameter.Constant = constant;
                changed = true;
            }
        }
        else
        {
            TableNextColumn();

            float availWidth = GetContentRegionAvail().X;
            float toWidth = CalcTextSize(" to "u8).X;
            float spacing = style.ItemSpacing.X * 2.0f;

            float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

            SetNextItemWidth(dragWidth);
            int randomMin = parameter.RandomMin;
            if (DragInt("##random-min"u8, ref randomMin, 1, 0, parameter.RandomMax))
            {
                parameter.RandomMin = randomMin;
                changed = true;
            }

            SameLine();
            Text(" to "u8);

            SameLine();
            SetNextItemWidth(dragWidth);
            int randomMax = parameter.RandomMax;
            if (DragInt("##random-max"u8, ref randomMax, 1, parameter.RandomMin, int.MaxValue))
            {
                parameter.RandomMax = randomMax;
                changed = true;
            }
        }

        PopID();

        return changed;
    }

    private bool DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref ParticleFloatParameter parameter)
    {
        bool changed = false;

        PushID(label);
        ImGuiStylePtr style = GetStyle();

        TableNextRow();
        TableNextColumn();
        AlignTextToFramePadding();
        Text(label);

        if (IsItemHovered())
        {
            SetTooltip(description);
        }

        TableNextColumn();
        SetNextItemWidth(-1);
        DrawParticleValueKindComboBox(ref parameter.Kind, ref changed);

        if (parameter.Kind == ParticleValueKind.Constant)
        {
            TableNextColumn();
            SetNextItemWidth(-1);
            float constant = parameter.Constant;
            if (DragFloat("##constant"u8, ref constant, 0.1f, 0.0f, float.MaxValue, "%.2f"u8))
            {
                parameter.Constant = constant;
                changed = true;
            }
        }
        else
        {
            TableNextColumn();

            float availWidth = GetContentRegionAvail().X;
            float toWidth = CalcTextSize(" to "u8).X;
            float spacing = style.ItemSpacing.X * 2.0f;

            float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

            SetNextItemWidth(dragWidth);
            float randomMin = parameter.RandomMin;
            if (DragFloat("##min-value"u8, ref randomMin, 0.1f, 0, parameter.RandomMax, "%.2f"u8))
            {
                parameter.RandomMin = randomMin;
                changed = true;
            }

            SameLine();
            Text(" to "u8);

            SameLine();
            SetNextItemWidth(dragWidth);
            float randomMax = parameter.RandomMax;
            if (DragFloat("##max-value"u8, ref randomMax, 1, parameter.RandomMin, float.MaxValue, "%.2f"u8))
            {
                parameter.RandomMax = randomMax;
                changed = true;
            }
        }

        PopID();

        return changed;
    }

    private bool DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref ParticleColorParameter parameter)
    {
        bool changed = false;

        PushID(label);
        ImGuiStylePtr style = GetStyle();

        TableNextRow();
        TableNextColumn();
        AlignTextToFramePadding();
        Text(label);

        if (IsItemHovered())
        {
            SetTooltip(description);
        }

        TableNextColumn();
        SetNextItemWidth(-1);
        DrawParticleValueKindComboBox(ref parameter.Kind, ref changed);

        if (parameter.Kind == ParticleValueKind.Constant)
        {
            TableNextColumn();

            HslColor constantHsl = new HslColor(parameter.Constant.X, parameter.Constant.Y, parameter.Constant.Z);
            XnaColor constantRgb = HslColor.ToRgb(constantHsl);
            SysVec4 constantColor = new SysVec4(constantRgb.R / 255.0f, constantRgb.G / 255.0f, constantRgb.B / 255.0f, 1.0f);

            float availWidth = GetContentRegionAvail().X;
            SysVec2 buttonSize = new SysVec2(availWidth, GetFrameHeight());

            if (ColorButton("##constant-button"u8, constantColor, ImGuiColorEditFlags.None, buttonSize))
            {
                OpenPopup("##constant-color-picker"u8);
            }

            if (BeginPopup("##constant-color-picker"u8))
            {
                float[] rgb = [constantColor.X, constantColor.Y, constantColor.Z];
                if (ColorPicker3("##constant-value"u8, rgb))
                {
                    XnaColor newConstantRgb = new XnaColor(rgb[0], rgb[1], rgb[2]);
                    HslColor newConstantHsl = HslColor.FromRgb(newConstantRgb);
                    parameter.Constant = new XnaVec3(newConstantHsl.H, newConstantHsl.S, newConstantHsl.L);
                    changed = true;
                }

                EndPopup();
            }
        }
        else
        {
            TableNextColumn();

            float availableWidth = GetContentRegionAvail().X;
            float toWidth = CalcTextSize(" to "u8).X;
            float spacing = style.ItemSpacing.X * 2.0f;
            float buttonWidth = (availableWidth - toWidth - spacing) * 0.5f;
            SysVec2 buttonSize = new SysVec2(buttonWidth, GetFrameHeight());

            HslColor randomHslMin = new HslColor(parameter.RandomMin.X, parameter.RandomMin.Y, parameter.RandomMin.Z);
            XnaColor randomRgbMin = HslColor.ToRgb(randomHslMin);
            SysVec4 randomColorMin = new SysVec4(randomRgbMin.R / 255.0f, randomRgbMin.G / 255.0f, randomRgbMin.B / 255.0f, 1.0f);

            HslColor randomHslMax = new HslColor(parameter.RandomMax.X, parameter.RandomMax.Y, parameter.RandomMax.Z);
            XnaColor randomRgbMax = HslColor.ToRgb(randomHslMax);
            SysVec4 randomColorMax = new SysVec4(randomRgbMax.R / 255.0f, randomRgbMax.G / 255.0f, randomRgbMax.B / 255.0f, 1.0f);

            if (ColorButton("##random-min-button"u8, randomColorMin, ImGuiColorEditFlags.None, buttonSize))
            {
                OpenPopup("##random-min-color-picker"u8);
            }

            if (BeginPopup("##random-min-color-picker"u8))
            {
                float[] rgb = [randomColorMin.X, randomColorMin.Y, randomColorMin.Z];
                if (ColorPicker3("##random-min"u8, rgb))
                {
                    XnaColor newRandomRgbMin = new XnaColor(rgb[0], rgb[1], rgb[2]);
                    HslColor newRandomHslMin = HslColor.FromRgb(newRandomRgbMin);
                    parameter.RandomMin = new XnaVec3(newRandomHslMin.H, newRandomHslMin.S, newRandomHslMin.L);
                    changed = true;
                }

                EndPopup();
            }

            SameLine();
            Text(SR.Label_To);

            SameLine();
            if (ColorButton("##random-max-button"u8, randomColorMax, ImGuiColorEditFlags.None, buttonSize))
            {
                OpenPopup("##random-max-color-picker"u8);
            }

            if (BeginPopup("##random-max-color-picker"u8))
            {
                float[] rgb = [randomColorMax.X, randomColorMax.Y, randomColorMax.Z];
                if (ColorPicker3("##random-max"u8, rgb))
                {
                    XnaColor newRandomRgbMax = new XnaColor(rgb[0], rgb[1], rgb[2]);
                    HslColor newRandomHslMax = HslColor.FromRgb(newRandomRgbMax);
                    parameter.RandomMax = new XnaVec3(newRandomHslMax.H, newRandomHslMax.S, newRandomHslMax.L);
                    changed = true;
                }

                EndPopup();
            }
        }

        PopID();

        return changed;
    }

    private bool DrawEmitterReleaseParameterRow(ReadOnlySpan<byte> label, ReadOnlySpan<byte> description, ref ParticleVector2Parameter parameter)
    {
        bool changed = false;

        PushID(label);
        ImGuiStylePtr style = GetStyle();

        TableNextRow();
        TableNextColumn();
        AlignTextToFramePadding();
        Text(label);

        if (IsItemHovered())
        {
            SetTooltip(description);
        }

        TableNextColumn();
        SetNextItemWidth(-1);
        DrawParticleValueKindComboBox(ref parameter.Kind, ref changed);

        if (parameter.Kind == ParticleValueKind.Constant)
        {
            TableNextColumn();

            float availWidth = GetContentRegionAvail().X;
            float spacing = style.ItemSpacing.X;
            float dragWidth = (availWidth - spacing) * 0.5f;

            SetNextItemWidth(dragWidth);
            float constantX = parameter.Constant.X;
            if (DragFloat("##constant-x"u8, ref constantX, 0.1f, 0.0f, float.MaxValue, "X: %.2f"u8))
            {
                parameter.Constant.X = constantX;
                changed = true;
            }

            SameLine();
            SetNextItemWidth(dragWidth);
            float constantY = parameter.Constant.Y;
            if (DragFloat("##constant-y"u8, ref constantY, 0.1f, 0.0f, float.MaxValue, "Y: %.2f"u8))
            {
                parameter.Constant.Y = constantY;
                changed = true;
            }

        }
        else
        {
            TableNextColumn();

            float availWidth = GetContentRegionAvail().X;
            float toWidth = CalcTextSize(" to "u8).X;
            float spacing = style.ItemSpacing.X * 2.0f;
            float dragWidth = (availWidth - toWidth - spacing) * 0.5f;

            SetNextItemWidth(dragWidth);
            float randomMinX = parameter.RandomMax.X;
            if (DragFloat("##random-min-x"u8, ref randomMinX, 0.1f, 0.0f, parameter.RandomMax.X, "X: %.2f"u8))
            {
                parameter.RandomMin.X = randomMinX;
                changed = true;
            }

            SameLine();
            Text(" to "u8);

            SameLine();
            SetNextItemWidth(dragWidth);
            float randomMaxX = parameter.RandomMax.X;
            if (DragFloat("##random-max-x"u8, ref randomMaxX, 0.1f, parameter.RandomMin.X, float.MaxValue, "X: %.2f"u8))
            {
                parameter.RandomMax.X = randomMaxX;
                changed = true;
            }


            TableNextRow();
            TableNextColumn();
            TableNextColumn();
            TableNextColumn();

            SetNextItemWidth(dragWidth);
            float randomMinY = parameter.RandomMin.Y;
            if (DragFloat("##random_min_y_value"u8, ref randomMinY, 0.1f, 0.0f, parameter.RandomMax.Y, "Y: %.2f"u8))
            {
                parameter.RandomMin.Y = randomMinY;
                changed = true;
            }

            SameLine();
            Text(" to "u8);

            SameLine();
            SetNextItemWidth(dragWidth);
            float randomMaxY = parameter.RandomMax.Y;
            if (DragFloat("##random_max_y_value"u8, ref randomMaxY, 0.1f, parameter.RandomMin.Y, float.MaxValue, "Y: %.2f"u8))
            {
                parameter.RandomMax.Y = randomMaxY;
                changed = true;
            }
        }

        PopID();

        return changed;
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
                if (_context.TextureExists(fileName))
                {
                    _pendingTextureFilePath = dialog.SelectedItem.FullName;
                    _confirmOverwrite = true;
                }
                else
                {
                    // No conflict, add directly
                    _context.AddTexture(dialog.SelectedItem.FullName);
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
                _context.AddTexture(_pendingTextureFilePath, overwrite: true);
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
        ParticleEmitter emitter = _context.SelectedEmitter;
        if (emitter == null)
        {
            return;
        }

        var texture = _context.GetTexture(fileName);
        if (texture != null)
        {
            emitter.TextureRegion = new Texture2DRegion(texture, texture.Bounds, fileName);
            _context.HasUnsavedChanges = true;
        }
    }
}
