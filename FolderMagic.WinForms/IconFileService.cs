namespace FolderMagic.WinForms;

internal static class IconFileService
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ico"
    };

    public static bool IsSupportedIconSource(string path)
    {
        return File.Exists(path) && SupportedExtensions.Contains(Path.GetExtension(path));
    }

    public static string PrepareIconFile(string sourcePath, string folderPath, bool saveToDriveRoot)
    {
        string sourceFullPath = Path.GetFullPath(sourcePath);
        string folderFullPath = Path.GetFullPath(folderPath);
        string baseName = Path.GetFileNameWithoutExtension(sourceFullPath);
        string destinationDirectory = saveToDriveRoot
            ? GetDriveRootIconDirectory(folderFullPath)
            : folderFullPath;

        Directory.CreateDirectory(destinationDirectory);

        if (saveToDriveRoot)
        {
            File.SetAttributes(destinationDirectory, File.GetAttributes(destinationDirectory) | FileAttributes.Hidden);
        }

        string destinationFileName = baseName + ".ico";
        string destinationPath = Path.Combine(destinationDirectory, destinationFileName);

        ClearReadonlyHiddenIfExists(destinationPath);

        if (!Path.GetFullPath(sourceFullPath).Equals(Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(sourceFullPath, destinationPath, overwrite: true);
        }

        File.SetAttributes(destinationPath, FileAttributes.Hidden | FileAttributes.ReadOnly);
        return ToDesktopIniIconPath(destinationPath, folderFullPath);
    }

    private static string GetDriveRootIconDirectory(string folderPath)
    {
        string? root = Path.GetPathRoot(folderPath);
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("无法确定目标文件夹所在驱动器。");
        }

        return Path.Combine(root, "ICOconfig");
    }

    private static void ClearReadonlyHiddenIfExists(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        File.SetAttributes(path, FileAttributes.Normal);
        File.Delete(path);
    }

    private static string ToDesktopIniIconPath(string destinationPath, string folderPath)
    {
        string relativePath = Path.GetRelativePath(folderPath, destinationPath);
        return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }
}
