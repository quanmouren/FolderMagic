# FolderMagic

FolderMagic 是一个 Windows 文件夹图标配置工具，使用 C# 实现。
🖼️ 文件夹图标配置工具

![界面预览](./docs/image.png "FolderMagic 界面预览")

## 功能

- 支持选择或拖入目标文件夹。
- 支持选择或拖入 `.ico` 图标文件。
- 图标不是必填项；可以只设置文件夹显示名称和提示信息。
- 支持将图标保存到目标文件夹内，或保存到驱动器根目录的 `ICOconfig` 目录。
- 生成或恢复配置后会调用 Windows Shell API 触发图标刷新。

## 构建与运行

```powershell
dotnet build FolderMagic.sln
dotnet run --project FolderMagic.WinForms
```

## ⚠️ 注意事项
程序将会创建/修改`desktop.ini`文件，如需手动编辑需要在文件夹选项的查看中关闭`隐藏受保护的操作系统文件(推荐)`

生成配置文件将覆盖原有`desktop.ini`文件，不推荐修改`系统桌面`、`下载`、`应用`、`文档`、`音乐`、`视频`和`地图`默认保存位置的文件夹

## 实现原理

程序通过创建或修改目标文件夹中的 `desktop.ini` 来设置文件夹显示信息：

```ini
[.ShellClassInfo]
LocalizedResourceName=显示名称
InfoTip=提示信息
IconResource=图标文件,0
```

随后设置 `desktop.ini` 和目标文件夹属性，并通过 `SHChangeNotify` 通知 Windows Shell 刷新图标状态。
