namespace Ember.Architecture.Services;

/// <summary>
/// Provides path operations relative to a configurable working directory.
/// </summary>
public interface IPathService
{
    /// <summary>
    /// Gets the current working directory.
    /// </summary>
    /// <returns>
    /// The absolute path of the current working directory.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The operating system does not support current directory functionality.
    /// </exception>
    string GetWorkingDirectory();

    /// <summary>
    /// Sets the current working directory for this service and the application.
    /// </summary>
    /// <param name="directory">
    /// The absolute or relative path to the directory to use as the working directory.
    /// </param>
    /// <exception cref="IOException">
    /// An I/O error occurred.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="directory"/> is a zero-length string, contains only white space, or contains
    /// one or more invalid characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="directory"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="PathTooLongException">
    /// The specified path exceeds the system-defined maximum length.
    /// </exception>
    /// <exception cref="System.Security.SecurityException">
    /// The caller does not have the required permission to access unmanaged code.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// The specified path was not found.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// The specified directory was not found.
    /// </exception>
    void SetWorkingDirectory(string directory);

    /// <summary>
    /// Copies a file to the working directory.
    /// </summary>
    /// <param name="filePath">
    /// The absolute or relative path to the source file to copy.
    /// </param>
    /// <param name="overwrite">
    /// <see langword="true"/> to allow overwriting an existing file; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// The absolute path to the copied file in the working directory.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="filePath"/> is a zero-length string, contains only white space, contains one or
    /// more invalid characters, or specifies a directory.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission, or the destination file is read-only, or
    /// <paramref name="overwrite"/> is <see langword="true"/> and the destination exists and is hidden
    /// but the source is not hidden.
    /// </exception>
    /// <exception cref="PathTooLongException">
    /// The specified path, file name, or both exceed the system-defined maximum length.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// The path specified in <paramref name="filePath"/> is invalid, or the working directory does not exist.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// <paramref name="filePath"/> was not found.
    /// </exception>
    /// <exception cref="IOException">
    /// The destination file exists and <paramref name="overwrite"/> is <see langword="false"/>, or an I/O error occurred.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="filePath"/> is in an invalid format.
    /// </exception>
    string CopyTo(string filePath, bool overwrite = false);

    /// <summary>
    /// Returns the relative path from the working directory to the specified file path.
    /// </summary>
    /// <param name="filePath">
    /// The absolute or relative path to convert.
    /// </param>
    /// <returns>
    /// The relative path from the working directory to <paramref name="filePath"/>, or <paramref name="filePath"/>
    /// if the paths do not share a common base.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="filePath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="filePath"/> is effectively empty.
    /// </exception>
    string GetRelativePath(string filePath);

    /// <summary>
    /// Determines whether the specified file path is located within or relative to the working directory.
    /// </summary>
    /// <param name="filePath">
    /// The file path to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="filePath"/> is within the working directory tree;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="filePath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="filePath"/> is effectively empty.
    /// </exception>
    bool IsRelativeTo(string filePath);
}
