namespace FolderMagic.WinForms;

internal sealed class MainForm : Form
{
    private static readonly Font UiFont = new("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

    private readonly TextBox _folderTextBox = new();
    private readonly TextBox _iconTextBox = new();
    private readonly TextBox _realNameTextBox = new();
    private readonly TextBox _displayNameTextBox = new();
    private readonly TextBox _infoTipTextBox = new();

    public MainForm()
    {
        Text = "文件夹图标配置工具";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(820, 398);
        Size = new Size(900, 418);
        MaximumSize = Size;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        AllowDrop = true;
        Font = UiFont;
        Icon = LoadWindowIcon();

        DragEnter += HandleDragEnter;
        DragDrop += HandleDragDrop;

        BuildLayout();
    }

    private static Icon? LoadWindowIcon()
    {
        Stream? stream = typeof(MainForm).Assembly.GetManifestResourceStream("FolderMagic.WinForms.FolderMagic.ico");
        return stream is null ? null : new Icon(stream);
    }

    private void BuildLayout()
    {
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 8,
            BackColor = SystemColors.Control,
            Padding = new Padding(24, 22, 24, 20)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        AddRow(layout, 0, "目标文件夹:", _folderTextBox, "浏览...", SelectFolder, HandleDragDrop);
        AddRow(layout, 1, "图标文件:", _iconTextBox, "浏览...", SelectIcon, HandleDragDrop);
        AddFullWidthTextBox(layout, 2, "真实文件名:", _realNameTextBox);
        AddFullWidthTextBox(layout, 3, "显示名称:", _displayNameTextBox);
        AddFullWidthTextBox(layout, 4, "提示信息:", _infoTipTextBox);

        TableLayoutPanel buttonPanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        Button localButton = CreateCommandButton("生成配置文件", Color.FromArgb(76, 175, 80));
        localButton.Click += (_, _) => Apply(saveToRoot: false);
        Button rootButton = CreateCommandButton("生成到根目录", Color.FromArgb(33, 150, 243));
        rootButton.Click += (_, _) => Apply(saveToRoot: true);
        localButton.Margin = new Padding(0, 0, 8, 0);
        rootButton.Margin = new Padding(8, 0, 0, 0);
        buttonPanel.Controls.Add(localButton, 0, 0);
        buttonPanel.Controls.Add(rootButton, 1, 0);
        layout.Controls.Add(buttonPanel, 0, 6);
        layout.SetColumnSpan(buttonPanel, 3);

        Button restoreButton = CreateCommandButton("恢复默认设置", Color.FromArgb(244, 67, 54));
        restoreButton.Dock = DockStyle.Fill;
        restoreButton.Margin = new Padding(0, 10, 0, 0);
        restoreButton.Click += (_, _) => Restore();
        layout.Controls.Add(restoreButton, 0, 7);
        layout.SetColumnSpan(restoreButton, 3);

        Controls.Add(layout);
    }

    private static void AddRow(
        TableLayoutPanel layout,
        int row,
        string label,
        TextBox textBox,
        string buttonText,
        EventHandler buttonHandler,
        DragEventHandler dragDropHandler)
    {
        Label rowLabel = CreateLabel(label);
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 4, 10, 4);
        textBox.BorderStyle = BorderStyle.Fixed3D;
        textBox.AllowDrop = true;
        textBox.DragEnter += HandleDragEnter;
        textBox.DragDrop += dragDropHandler;

        Button button = new()
        {
            Text = buttonText,
            Dock = DockStyle.Fill,
            Margin = textBox.Margin,
            FlatStyle = FlatStyle.Standard,
            Font = UiFont
        };
        button.Click += buttonHandler;

        layout.Controls.Add(rowLabel, 0, row);
        layout.Controls.Add(textBox, 1, row);
        layout.Controls.Add(button, 2, row);
    }

    private void AddFullWidthTextBox(TableLayoutPanel layout, int row, string label, TextBox textBox)
    {
        layout.Controls.Add(CreateLabel(label), 0, row);
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 4, 0, 4);
        textBox.BorderStyle = BorderStyle.Fixed3D;
        layout.Controls.Add(textBox, 1, row);
        layout.SetColumnSpan(textBox, 2);
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            TextAlign = ContentAlignment.MiddleRight,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 10, 0),
            Margin = new Padding(0),
            Font = UiFont
        };
    }

    private static Button CreateCommandButton(string text, Color color)
    {
        return new Button
        {
            Text = text,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Dock = DockStyle.Fill,
            Height = 40,
            Margin = new Padding(0),
            Font = UiFont
        };
    }

    private void SelectFolder(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadFolder(dialog.SelectedPath);
        }
    }

    private void SelectIcon(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Filter = "图标文件|*.ico|所有文件|*.*"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _iconTextBox.Text = dialog.FileName;
        }
    }

    private void LoadFolder(string folderPath)
    {
        _folderTextBox.Text = folderPath;
        _iconTextBox.Clear();
        _realNameTextBox.Text = Path.GetFileName(Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar));
        _displayNameTextBox.Clear();
        _infoTipTextBox.Clear();

        try
        {
            DesktopIniData data = FolderConfigService.ReadDesktopIni(folderPath);
            _displayNameTextBox.Text = data.LocalizedResourceName;
            _infoTipTextBox.Text = data.InfoTip;

            if (!string.IsNullOrWhiteSpace(data.IconResource))
            {
                _iconTextBox.Text = Path.GetFullPath(Path.Combine(folderPath, data.IconResource));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"读取 desktop.ini 失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Apply(bool saveToRoot)
    {
        if (string.IsNullOrWhiteSpace(_folderTextBox.Text))
        {
            MessageBox.Show(this, "请先选择文件夹。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!string.IsNullOrWhiteSpace(_iconTextBox.Text) && !IconFileService.IsSupportedIconSource(_iconTextBox.Text))
        {
            MessageBox.Show(this, "图标文件需为 .ico。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            string finalFolderPath = FolderConfigService.Apply(new FolderIconOptions
            {
                FolderPath = _folderTextBox.Text,
                SourceIconPath = string.IsNullOrWhiteSpace(_iconTextBox.Text) ? null : _iconTextBox.Text,
                RealFolderName = _realNameTextBox.Text,
                DisplayName = _displayNameTextBox.Text,
                InfoTip = _infoTipTextBox.Text,
                SaveIconToDriveRoot = saveToRoot
            });

            _folderTextBox.Text = finalFolderPath;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"操作失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Restore()
    {
        if (string.IsNullOrWhiteSpace(_folderTextBox.Text))
        {
            MessageBox.Show(this, "请先选择要恢复的文件夹。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            FolderConfigService.Restore(_folderTextBox.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"恢复失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HandleDragDrop(object? sender, DragEventArgs e)
    {
        if (!e.Data?.GetDataPresent(DataFormats.FileDrop) ?? true)
        {
            return;
        }

        string[] files = (string[])e.Data!.GetData(DataFormats.FileDrop)!;
        foreach (string path in files)
        {
            if (Directory.Exists(path))
            {
                LoadFolder(path);
                return;
            }

            if (IconFileService.IsSupportedIconSource(path))
            {
                _iconTextBox.Text = path;
                return;
            }
        }
    }

    private static void HandleDragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }
}
