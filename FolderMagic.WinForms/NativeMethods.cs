using System.Runtime.InteropServices;

namespace FolderMagic.WinForms;

internal static class NativeMethods
{
    public const uint ShcneUpdateDir = 0x00001000;
    public const uint ShcneUpdateItem = 0x00002000;
    public const uint ShcneAssocChanged = 0x08000000;
    public const uint ShcnfPathW = 0x0005;
    public const uint ShcnfIdList = 0x0000;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern void SHChangeNotify(
        uint wEventId,
        uint uFlags,
        string? dwItem1,
        string? dwItem2);

}
