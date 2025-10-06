using System.Diagnostics.CodeAnalysis;

namespace ImGuiFileDialog;

internal class FileDialogState
{
    public string CurrentDirectory;
    public string FileFilter;
    public FileDialogMode Mode;
    public List<FileSystemEntry> Entries;
    public string SelectedEntry;
    public string FileName = string.Empty;

    public FileDialogState(string initialDirectory, string fileFilter, FileDialogMode mode)
    {
        CurrentDirectory = initialDirectory;
        FileFilter = fileFilter;
        Mode = mode;
        RefreshEntries();
    }

    [MemberNotNull(nameof(Entries))]
    public void RefreshEntries()
    {
        Entries = [];

        try
        {
            var directories = Directory.GetDirectories(CurrentDirectory);
            foreach (var dir in directories.OrderBy(d => d))
            {
                FileSystemEntry entry = new FileSystemEntry
                {
                    Name = Path.GetFileName(dir),
                    FullPath = dir,
                    IsDirectory = true
                };
                Entries.Add(entry);
            }

            if (Mode != FileDialogMode.SelectFolder)
            {
                var files = Directory.GetFiles(CurrentDirectory);
                foreach (var file in files.OrderBy(f => f))
                {
                    if (FileFilter != null && file.EndsWith(FileFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    Entries.Add(new FileSystemEntry
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false
                    });
                }
            }
        }
        catch
        {

        }

    }
}
