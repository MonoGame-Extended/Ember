using System;
using System.IO;
using Ember.Architecture.Components;
using Ember.Architecture.PopupModals;
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

            if (Button("Add New Emitter"u8, new SysVec2(-1, 0)))
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
                        uint buttonColor = isSelected ? GetColorU32(ImGuiCol.Button) : GetColorU32(SysVec4.Zero);
                        uint buttonHoverColor = GetColorU32(ImGuiCol.ButtonHovered);

                        // Name Column
                        TableNextColumn();
                        SysVec2 nameButtonSize = new SysVec2(-1, GetFrameHeight());
                        PushStyleColor(ImGuiCol.Button, buttonColor);
                        PushStyleColor(ImGuiCol.ButtonHovered, buttonHoverColor);
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

                        PopStyleColor(2);
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

                        // Reorder emitters if a drag/drop occurred
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
            if (_context.SelectedEmitter is not ParticleEmitter emitter)
            {
                return;
            }

            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            if (BeginChild("##selected-emitter-properties-child-window"u8, SysVec2.Zero, childFlags))
            {
                BeginDisabled(_context.IsLocked(emitter));
                if (PropertyTable.BeginPropertyTable("##selected-emitter-properties-table"u8))
                {
                    // Name property
                    string emitterName = emitter.Name;
                    if (PropertyTable.InputTextProperty("Name"u8, "The display name of the selected emitter"u8, ref emitterName))
                    {
                        emitter.Name = emitterName;
                        _context.HasUnsavedChanges = true;
                    }

                    // Texture Property
                    if (PropertyTable.TextureProperty("Texture"u8, "The texture used by the selected emitter"u8, emitter.TextureRegion))
                    {
                        _selectTexture = true;
                    }

                    if (emitter.TextureRegion != null)
                    {
                        // Source Rectangle
                        XnaRect sourceRectangle = emitter.TextureRegion.Bounds;
                        if (PropertyTable.InputRectProperty("Source Rectangle"u8, "The rectangular bounds within the texture to render"u8, ref sourceRectangle))
                        {
                            emitter.TextureRegion = new Texture2DRegion(emitter.TextureRegion.Texture, sourceRectangle);
                            _context.HasUnsavedChanges = true;
                        }

                        // Reset source rectangle
                        if (PropertyTable.ButtonProperty("Reset Source Rectangle"u8, "Resets the source rectangle back to the bounds of the texture"u8))
                        {
                            emitter.TextureRegion = new Texture2DRegion(emitter.TextureRegion.Texture);
                            _context.HasUnsavedChanges = true;
                        }
                    }

                    // Capacity Property
                    int emitterCapacity = emitter.Capacity;
                    if (PropertyTable.InputIntProperty("Capacity"u8, "The maximum number of particles that this emitter can have active at a given time"u8, ref emitterCapacity))
                    {
                        emitter.ChangeCapacity(emitterCapacity);
                        _context.HasUnsavedChanges = true;
                    }

                    // Lifespan Property
                    float emitterLifeSpan = emitter.LifeSpan;
                    if (PropertyTable.DragFloatProperty("Lifespan"u8, "The amount of time, in seconds, that each particle released from this emitter will live"u8, ref emitterLifeSpan, 0.1f, 0.0f, float.MaxValue))
                    {
                        emitter.LifeSpan = emitterLifeSpan;
                        _context.HasUnsavedChanges = true;
                    }

                    ReadOnlySpan<byte> renderingOrderPreview = emitter.RenderingOrder == ParticleRenderingOrder.FrontToBack
                                                               ? "Front to Back"u8
                                                               : "Back to Front"u8;
                    if (PropertyTable.BeginComboProperty("Rendering Order"u8, "The order in which the particles are rendered\n\n- Front To Back: Particles are rendered in front to back order\n- Back To Front: Particles are rendered in back to front order"u8, renderingOrderPreview))
                    {
                        if (PropertyTable.ComboItem("Front to Back"u8, "Particles are rendered front to back"u8, emitter.RenderingOrder == ParticleRenderingOrder.FrontToBack))
                        {
                            emitter.RenderingOrder = ParticleRenderingOrder.FrontToBack;
                            _context.HasUnsavedChanges = true;
                        }

                        if (PropertyTable.ComboItem("Back to Front"u8, "Particles are rendered back to front"u8, emitter.RenderingOrder == ParticleRenderingOrder.BackToFront))
                        {
                            emitter.RenderingOrder = ParticleRenderingOrder.BackToFront;
                            _context.HasUnsavedChanges = true;
                        }

                        PropertyTable.EndComboProperty();
                    }

                    // Offset Property
                    XnaVec2 emitterOffset = emitter.Offset;
                    if (PropertyTable.DragVector2Property("Offset"u8, "The position offset applied to this emitter from the effect position"u8, ref emitterOffset, 1.0f, float.MinValue, float.MaxValue))
                    {
                        emitter.Offset = emitterOffset;
                        _context.HasUnsavedChanges = true;
                    }

                    PropertyTable.EndPropertyTable();
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

            if (_context.SelectedEmitter is not ParticleEmitter emitter)
            {
                return;
            }

            if (BeginChild("##selected-emitter-profile-child-window"u8, SysVec2.Zero, childFlags))
            {
                BeginDisabled(_context.IsLocked(emitter));

                if (PropertyTable.BeginPropertyTable("##selected-emitter-profile-properties-table"u8))
                {
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

                    if (PropertyTable.BeginComboProperty("Profile Type"u8, "Profiles define the emission pattern, such as points, lines, rings, or areas, from which particles originate"u8, profileTypePreview))
                    {
                        if (PropertyTable.ComboItem("Box Fill"u8, "Randomly distributes particles throughout a rectangular area"u8, emitter.Profile is BoxFillProfile))
                        {
                            emitter.Profile = Activator.CreateInstance(typeof(BoxFillProfile)) as Profile;
                            _context.HasUnsavedChanges = true;
                        }

                        if (PropertyTable.ComboItem("Box"u8, "Distributes particles along the edges of a rectangular boundary"u8, emitter.Profile is BoxProfile))
                        {
                            emitter.Profile = Activator.CreateInstance(typeof(BoxProfile)) as Profile;
                            _context.HasUnsavedChanges = true;
                        }

                        if (PropertyTable.ComboItem("Box Uniform"u8, "Distributes particles along the edges of a rectangular boundary with uniform density"u8, emitter.Profile is BoxUniformProfile))
                        {
                            emitter.Profile = Activator.CreateInstance(typeof(BoxUniformProfile)) as Profile;
                            _context.HasUnsavedChanges = true;
                        }

                        if (PropertyTable.ComboItem("Circle"u8, "Distributes particles throughout a circular area with controllable radiation patterns"u8, emitter.Profile is CircleProfile))
                        {
                            emitter.Profile = Activator.CreateInstance(typeof(CircleProfile)) as Profile;
                            _context.HasUnsavedChanges = true;
                        }

                        if (PropertyTable.ComboItem("Line"u8, "Distributes particles uniformly along a line segment with random headings"u8, emitter.Profile is LineProfile))
                        {
                            emitter.Profile = Activator.CreateInstance(typeof(LineProfile)) as Profile;
                            _context.HasUnsavedChanges = true;
                        }

                        if (PropertyTable.ComboItem("Point"u8, "Emits all particles from a single point with random headings"u8, emitter.Profile is PointProfile))
                        {
                            emitter.Profile = Activator.CreateInstance(typeof(PointProfile)) as Profile;
                            _context.HasUnsavedChanges = true;
                        }

                        if (PropertyTable.ComboItem("Ring"u8, "Distributes particles along the perimeter of a circle with controllable radiation patterns"u8, emitter.Profile is RingProfile))
                        {
                            emitter.Profile = Activator.CreateInstance(typeof(RingProfile)) as Profile;
                            _context.HasUnsavedChanges = true;
                        }

                        if (PropertyTable.ComboItem("Spray"u8, "Emits particles from a single point in a directional cone pattern"u8, emitter.Profile is SprayProfile))
                        {
                            emitter.Profile = Activator.CreateInstance(typeof(SprayProfile)) as Profile;
                            _context.HasUnsavedChanges = true;
                        }

                        PropertyTable.EndComboProperty();
                    }

                    // Profile Properties Row(s)
                    switch (emitter.Profile)
                    {
                        case BoxFillProfile boxFill:
                            float boxFillWidth = boxFill.Width;
                            if (PropertyTable.DragFloatProperty("Width"u8, "The width of the rectangular area"u8, ref boxFillWidth, 0.1f, 0.0f, float.MaxValue))
                            {
                                boxFill.Width = boxFillWidth;
                                _context.HasUnsavedChanges = true;
                            }

                            float boxFillHeight = boxFill.Height;
                            if (PropertyTable.DragFloatProperty("Height"u8, "The height of the rectangular area"u8, ref boxFillHeight, 0.1f, 0.0f, float.MaxValue))
                            {
                                boxFill.Height = boxFillHeight;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case BoxProfile box:
                            float boxWidth = box.Width;
                            if (PropertyTable.DragFloatProperty("Width"u8, "The width of the rectangular perimeter"u8, ref boxWidth, 0.1f, 0.0f, float.MaxValue))
                            {
                                box.Width = boxWidth;
                                _context.HasUnsavedChanges = true;
                            }

                            float boxHeight = box.Height;
                            if (PropertyTable.DragFloatProperty("Height"u8, "The height of the rectangular perimeter"u8, ref boxHeight, 0.1f, 0.0f, float.MaxValue))
                            {
                                box.Height = boxHeight;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case BoxUniformProfile boxUniform:
                            float boxUniformWidth = boxUniform.Width;
                            if (PropertyTable.DragFloatProperty("Width"u8, "The width of the rectangular perimeter"u8, ref boxUniformWidth, 0.1f, 0.0f, float.MaxValue))
                            {
                                boxUniform.Width = boxUniformWidth;
                                _context.HasUnsavedChanges = true;
                            }

                            float boxUniformHeight = boxUniform.Height;
                            if (PropertyTable.DragFloatProperty("Height"u8, "The height of the rectangular perimeter"u8, ref boxUniformHeight, 0.1f, 0.0f, float.MaxValue))
                            {
                                boxUniform.Height = boxUniformHeight;
                                _context.HasUnsavedChanges = true;
                            }
                            break;

                        case CircleProfile circle:
                            float circleRadius = circle.Radius;
                            if (PropertyTable.DragFloatProperty("Radius"u8, "The radius of the circular area"u8, ref circleRadius, 0.1f, 0.0f, float.MaxValue))
                            {
                                circle.Radius = circleRadius;
                                _context.HasUnsavedChanges = true;
                            }

                            ReadOnlySpan<byte> circleRadiatePreview = circle.Radiate switch
                            {
                                CircleRadiation.None => "None"u8,
                                CircleRadiation.In => "In"u8,
                                CircleRadiation.Out => "Out"u8,
                                _ => throw new InvalidOperationException($"Unknown circle radiation '{circle.Radiate}")
                            };
                            if (PropertyTable.BeginComboProperty("Radiate"u8, "The radiation mode that determines how particle headings are calculate"u8, circleRadiatePreview))
                            {
                                if (PropertyTable.ComboItem("None"u8, "Particles move toward the center of the circle"u8, circle.Radiate == CircleRadiation.None))
                                {
                                    circle.Radiate = CircleRadiation.None;
                                    _context.HasUnsavedChanges = true;
                                }

                                if (PropertyTable.ComboItem("In"u8, "Particles move in random directions unrelated to their positions"u8, circle.Radiate == CircleRadiation.In))
                                {
                                    circle.Radiate = CircleRadiation.In;
                                    _context.HasUnsavedChanges = true;
                                }

                                if (PropertyTable.ComboItem("Out"u8, "Particles move away from the center of the circle"u8, circle.Radiate == CircleRadiation.Out))
                                {
                                    circle.Radiate = CircleRadiation.Out;
                                    _context.HasUnsavedChanges = true;
                                }

                                PropertyTable.EndComboProperty();
                            }
                            break;

                        case LineProfile line:
                            XnaVec2 lineAxis = line.Axis;
                            if (PropertyTable.DragVector2Property("Axis"u8, "The direction vector of the line axis"u8, ref lineAxis, 1f, -1f, 1f))
                            {
                                line.Axis = lineAxis;
                                _context.HasUnsavedChanges = true;
                            }

                            float lineLength = line.Length;
                            if (PropertyTable.DragFloatProperty("Length"u8, "The length of the line segment"u8, ref lineLength, 0.1f, 0.0f, float.MaxValue))
                            {
                                line.Length = lineLength;
                                _context.HasUnsavedChanges = true;
                            }


                            ReadOnlySpan<byte> lineRadiatePreview = line.Radiate switch
                            {
                                LineRadiation.None => "None"u8,
                                LineRadiation.Directional => "Directional"u8,
                                LineRadiation.PerpendicularUp => "Perpendicular Up"u8,
                                LineRadiation.PerpendicularDown => "Perpendicular Down"u8,
                                _ => throw new InvalidOperationException($"Unknown circle radiation '{line.Radiate}")
                            };
                            if (PropertyTable.BeginComboProperty("Radiate"u8, "Determines the initial particle headings when radiating from the line axis"u8, lineRadiatePreview))
                            {
                                if (PropertyTable.ComboItem("None"u8, "The initial heading of particles is completely random and has no relationship to their position along the line"u8, line.Radiate == LineRadiation.None))
                                {
                                    line.Radiate = LineRadiation.None;
                                    _context.HasUnsavedChanges = true;
                                }

                                if (PropertyTable.ComboItem("Directional"u8, "All particles are given the same initial heading as specified by the Direction vector"u8, line.Radiate == LineRadiation.Directional))
                                {
                                    line.Radiate = LineRadiation.Directional;
                                    line.Direction = XnaVec2.UnitY;
                                    _context.HasUnsavedChanges = true;
                                }

                                if (PropertyTable.ComboItem("Perpendicular Up"u8, "All particles are given initial headings perpendicular to the line's axis, pointing upward in screen coordinates (negative Y direction)"u8, line.Radiate == LineRadiation.PerpendicularUp))
                                {
                                    line.Radiate = LineRadiation.PerpendicularUp;
                                    line.Direction = XnaVec2.Zero;
                                    _context.HasUnsavedChanges = true;
                                }

                                if (PropertyTable.ComboItem("Perpendicular Down"u8, "All particles are given initial headings perpendicular to the line's axis, pointing downward in screen coordinates (positive Y direction)"u8, line.Radiate == LineRadiation.PerpendicularDown))
                                {
                                    line.Radiate = LineRadiation.PerpendicularDown;
                                    line.Direction = XnaVec2.Zero;
                                    _context.HasUnsavedChanges = true;
                                }

                                PropertyTable.EndComboProperty();
                            }

                            if (line.Radiate == LineRadiation.Directional)
                            {
                                XnaVec2 lineDirection = line.Direction;
                                if (PropertyTable.DragVector2Property("Direction"u8, ""u8, ref lineDirection, 0.1f, -1.0f, 1.0f))
                                {
                                    line.Direction = lineDirection;
                                    _context.HasUnsavedChanges = true;
                                }
                            }
                            break;

                        case PointProfile point:
                            // No additional properties
                            break;

                        case RingProfile ring:
                            float ringRadius = ring.Radius;
                            if (PropertyTable.DragFloatProperty("Radius"u8, "The radius if the ring."u8, ref ringRadius, 0.1f, 0.0f, float.MaxValue))
                            {
                                ring.Radius = ringRadius;
                                _context.HasUnsavedChanges = true;
                            }

                            ReadOnlySpan<byte> ringRadiatePreview = ring.Radiate switch
                            {
                                CircleRadiation.None => "None"u8,
                                CircleRadiation.In => "In"u8,
                                CircleRadiation.Out => "Out"u8,
                                _ => throw new InvalidOperationException($"Unknown circle radiation '{ring.Radiate}")
                            };
                            if (PropertyTable.BeginComboProperty("Radiate"u8, "The radiation mode that determines how particle headings are calculate"u8, ringRadiatePreview))
                            {
                                if (PropertyTable.ComboItem("None"u8, "Particles move in random directions unrelated to their positions"u8, ring.Radiate == CircleRadiation.None))
                                {
                                    ring.Radiate = CircleRadiation.None;
                                    _context.HasUnsavedChanges = true;
                                }

                                if (PropertyTable.ComboItem("In"u8, "Particles move toward the center of the circle"u8, ring.Radiate == CircleRadiation.In))
                                {
                                    ring.Radiate = CircleRadiation.In;
                                    _context.HasUnsavedChanges = true;
                                }

                                if (PropertyTable.ComboItem("Out"u8, "Particles move away from the center of the circle"u8, ring.Radiate == CircleRadiation.Out))
                                {
                                    ring.Radiate = CircleRadiation.Out;
                                    _context.HasUnsavedChanges = true;
                                }

                                PropertyTable.EndComboProperty();
                            }
                            break;

                        case SprayProfile spray:
                            XnaVec2 sprayDirection = spray.Direction;
                            if (PropertyTable.DragVector2Property("Direction"u8, "The central direction vector of the spray"u8, ref sprayDirection, 0.1f, float.MinValue, float.MaxValue))
                            {
                                spray.Direction = sprayDirection;
                                _context.HasUnsavedChanges = true;
                            }

                            float spraySpread = spray.Spread;
                            if (PropertyTable.DragFloatProperty("Spread"u8, "The angular spread of the spray cone (in radians)"u8, ref spraySpread, 0.1f, 0.0f, float.MaxValue))
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

    private void DrawSelectedEmitterReleaseParameters()
    {
        if (CollapsingHeader("Emitter Release Parameters"u8, ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (_context.SelectedEmitter is not ParticleEmitter emitter)
            {
                return;
            }

            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders
                                         | ImGuiChildFlags.AutoResizeY;

            if (BeginChild("##selected-emitter-release-parameters-child-window"u8, SysVec2.Zero, childFlags))
            {
                BeginDisabled(_context.IsLocked(emitter));

                if (PropertyTable.BeginReleaseParameterPropertyTable("##selected-emitter-release-parameters-table"u8))
                {
                    if (PropertyTable.ReleaseParameter("Quantity"u8, "The number of particles released per emission"u8, ref emitter.Parameters.Quantity))
                    {
                        _context.HasUnsavedChanges = true;
                    }

                    if (PropertyTable.ReleaseParameter("Speed"u8, "The initial speed of particles when released"u8, ref emitter.Parameters.Speed))
                    {
                        _context.HasUnsavedChanges = true;
                    }

                    if (PropertyTable.ReleaseParameter("Color"u8, "The color of the particles"u8, ref emitter.Parameters.Color))
                    {
                        _context.HasUnsavedChanges = true;
                    }

                    if (PropertyTable.ReleaseParameter("Opacity"u8, "The transparency of particles (0.0 = transparent, 1.0 = opaque)"u8, ref emitter.Parameters.Opacity))
                    {
                        _context.HasUnsavedChanges = true;
                    }


                    if (PropertyTable.ReleaseParameter("Scale"u8, "The size multiplier of particles"u8, ref emitter.Parameters.Scale))
                    {
                        _context.HasUnsavedChanges = true;
                    }

                    if (PropertyTable.ReleaseParameter("Rotation"u8, "The initial rotation angle of particles in radians"u8, ref emitter.Parameters.Rotation))
                    {
                        _context.HasUnsavedChanges = true;
                    }

                    if (PropertyTable.ReleaseParameter("Mass"u8, "The mass of particles (affects physics interactions)"u8, ref emitter.Parameters.Mass))
                    {
                        _context.HasUnsavedChanges = true;
                    }

                    PropertyTable.EndPropertyTable();
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
            Text(" to "u8);

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
