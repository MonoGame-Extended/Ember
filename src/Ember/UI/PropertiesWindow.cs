// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Ember.UI.ChildWindows;
using Hexa.NET.ImGui;

namespace Ember.UI;

public static class PropertiesWindow
{
    public static void Draw()
    {
        if (ImGui.Begin(SR.Window_Properties))
        {
            ParticleEffectPropertiesChildWindow.Draw();
            ParticleEmitterListChildWindow.Draw();
            SelectedEmitterPropertiesChildWindow.Draw();
            SelectedEmitterProfileChildWindow.Draw();
            SelectedEmitterReleaseParametersChildWindow.Draw();
        }
        ImGui.End();
    }
}
