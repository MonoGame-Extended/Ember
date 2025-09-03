// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.IO;

namespace Ember.UI;

public struct FileItem
{
    public string Name;
    public string Path;
    public bool IsDirectory;

    public FileItem(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(path);

        if (string.IsNullOrEmpty(Name))
        {
            Name = path;
        }

        IsDirectory = Directory.Exists(path);
    }
}
