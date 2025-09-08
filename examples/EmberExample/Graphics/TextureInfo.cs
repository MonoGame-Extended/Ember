// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Microsoft.Xna.Framework.Graphics;

namespace EmberExample.Graphics;

public sealed class TextureInfo
{
    public Texture2D Texture { get; set; }
    public bool IsManaged { get; set; }
}
