namespace FolderMagic.WinForms;

internal sealed class FolderIconOptions
{
    public required string FolderPath { get; init; }
    public string? SourceIconPath { get; init; }
    public required string RealFolderName { get; init; }
    public string? DisplayName { get; init; }
    public string? InfoTip { get; init; }
    public bool SaveIconToDriveRoot { get; init; }
}
