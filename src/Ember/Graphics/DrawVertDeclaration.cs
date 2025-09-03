// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;

namespace Ember.Graphics;

public static class DrawVertDeclaration
{
    public static readonly VertexDeclaration Declaration;

    public static readonly int Size;

    static unsafe DrawVertDeclaration()
    {
        Size = sizeof(ImDrawVert);

        VertexElement position = new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0);
        VertexElement uv = new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);
        VertexElement color = new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0);

        Declaration = new VertexDeclaration(Size, position, uv, color);
    }
}
