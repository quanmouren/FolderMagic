using System.Text;

namespace FolderMagic.WinForms;

internal static class FolderConfigService
{
    public static DesktopIniData ReadDesktopIni(string folderPath)
    {
        string iniPath = Path.Combine(folderPath, "desktop.ini");
        DesktopIniData data = new();

        if (!File.Exists(iniPath))
        {
            return data;
        }

        File.SetAttributes(iniPath, FileAttributes.Normal);
        string[] lines = File.ReadAllLines(iniPath, Encoding.Default);
        SetSystemHidden(iniPath);

        string localizedName = string.Empty;
        string infoTip = string.Empty;
        string iconResource = string.Empty;

        foreach (string line in lines)
        {
            if (line.StartsWith("LocalizedResourceName=", StringComparison.OrdinalIgnoreCase))
            {
                localizedName = line.Split('=', 2)[1];
            }
            else if (line.StartsWith("InfoTip=", StringComparison.OrdinalIgnoreCase))
            {
                infoTip = line.Split('=', 2)[1];
            }
            else if (line.StartsWith("IconResource=", StringComparison.OrdinalIgnoreCase))
            {
                iconResource = line.Split('=', 2)[1].Split(',', 2)[0];
            }
        }

        return new DesktopIniData
        {
            LocalizedResourceName = localizedName,
            InfoTip = infoTip,
            IconResource = iconResource
        };
    }

    public static string Apply(FolderIconOptions options)
    {
        string folderPath = Path.GetFullPath(options.FolderPath);
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException("目标文件夹不存在。");
        }

        string iconResource = string.Empty;
        if (!string.IsNullOrWhiteSpace(options.SourceIconPath))
        {
            iconResource = IconFileService.PrepareIconFile(
                options.SourceIconPath,
                folderPath,
                options.SaveIconToDriveRoot);
        }

        string finalFolderPath = RenameFolderIfNeeded(folderPath, options.RealFolderName);
        WriteDesktopIni(finalFolderPath, options.DisplayName, options.InfoTip, iconResource);
        NotifyShellChanged(finalFolderPath);
        return finalFolderPath;
    }

    public static void Restore(string folderPath)
    {
        string fullFolderPath = Path.GetFullPath(folderPath);
        if (!Directory.Exists(fullFolderPath))
        {
            throw new DirectoryNotFoundException("目标文件夹不存在。");
        }

        string iniPath = Path.Combine(fullFolderPath, "desktop.ini");
        string iconResource = string.Empty;

        if (File.Exists(iniPath))
        {
            DesktopIniData data = ReadDesktopIni(fullFolderPath);
            iconResource = data.IconResource;
            File.SetAttributes(iniPath, FileAttributes.Normal);
            File.Delete(iniPath);
        }

        if (!string.IsNullOrWhiteSpace(iconResource))
        {
            string iconPath = Path.GetFullPath(Path.Combine(fullFolderPath, iconResource));
            if (File.Exists(iconPath) && IsInsideDirectory(iconPath, fullFolderPath))
            {
                File.SetAttributes(iconPath, FileAttributes.Normal);
                File.Delete(iconPath);
            }
        }

        File.SetAttributes(fullFolderPath, File.GetAttributes(fullFolderPath) & ~FileAttributes.System);
        NotifyShellChanged(fullFolderPath);
    }

    private static void WriteDesktopIni(string folderPath, string? displayName, string? infoTip, string iconResource)
    {
        string iniPath = Path.Combine(folderPath, "desktop.ini");
        if (File.Exists(iniPath))
        {
            File.SetAttributes(iniPath, FileAttributes.Normal);
        }

        StringBuilder builder = new();
        builder.AppendLine("[.ShellClassInfo]");

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            builder.AppendLine($"LocalizedResourceName={displayName}");
        }

        if (!string.IsNullOrWhiteSpace(infoTip))
        {
            builder.AppendLine($"InfoTip={infoTip}");
        }

        if (!string.IsNullOrWhiteSpace(iconResource))
        {
            builder.AppendLine($"IconResource={iconResource},0");
        }

        File.WriteAllText(iniPath, builder.ToString(), Encoding.Default);
        SetSystemHidden(iniPath);
        File.SetAttributes(folderPath, File.GetAttributes(folderPath) | FileAttributes.System);
    }

    private static string RenameFolderIfNeeded(string folderPath, string realFolderName)
    {
        if (string.IsNullOrWhiteSpace(realFolderName)
            || Path.GetFileName(folderPath).Equals(realFolderName, StringComparison.OrdinalIgnoreCase))
        {
            return folderPath;
        }

        string? parentDirectory = Path.GetDirectoryName(folderPath);
        if (string.IsNullOrWhiteSpace(parentDirectory))
        {
            throw new InvalidOperationException("无法重命名驱动器根目录。");
        }

        string newFolderPath = Path.Combine(parentDirectory, realFolderName);
        Directory.Move(folderPath, newFolderPath);
        return newFolderPath;
    }

    private static void SetSystemHidden(string path)
    {
        File.SetAttributes(path, FileAttributes.Hidden | FileAttributes.System);
    }

    private static bool IsInsideDirectory(string path, string directory)
    {
        string fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string fullDirectory = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private static void NotifyShellChanged(string folderPath)
    {
        NativeMethods.SHChangeNotify(
            NativeMethods.ShcneUpdateDir,
            NativeMethods.ShcnfPathW,
            folderPath,
            null);
        NativeMethods.SHChangeNotify(
            NativeMethods.ShcneUpdateItem,
            NativeMethods.ShcnfPathW,
            folderPath,
            null);
        NativeMethods.SHChangeNotify(
            NativeMethods.ShcneAssocChanged,
            NativeMethods.ShcnfIdList,
            null,
            null);
    }
}
