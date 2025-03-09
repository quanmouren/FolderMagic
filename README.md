# FolderMagic
🖼️ 文件夹图标配置工具

![界面预览](https://raw.githubusercontent.com/quanmouren/FolderMagic/main/res/image.png)

## 🚀 使用方法

1. 安装库
```
pip install tkinterdnd2
```
2. 运行主程序
```
python main.py
```
3. 将文件夹或图标文件（支持 `.dll[0]`/`.png`/`.jpg`/`.ico`）拖入窗口
4. 等待或手动刷新资源管理器（在任务管理器中重启`Windows资源管理器`）
5. 生成配置：
 - 生成配置文件：在目标文件夹内复制一份图标文件
 - 生成到根目录：图标将保存在驱动器根目录的 \ICOconfig\ 目录下（适合U盘便携使用）

## ⚠️ 注意事项

1. 程序将会创建/修改`desktop.ini`文件，如需手动编辑需要在文件夹选项的查看中关闭`隐藏受保护的操作系统文件(推荐)`
2. 生成配置文件将覆盖原有`desktop.ini`文件，不推荐修改系统`桌面`、`下载`、`应用`、`文档`、`音乐`、`视频`和`地图`默认保存位置的文件夹

## 🔧 实现原理

通过修改系统文件 desktop.ini 的 IconResource 属性：
```
[.ShellClassInfo]
LocalizedResourceName=当鼠标悬浮在文件夹上时显示的提示信息
InfoTip=在资源管理器中显示的名字
IconResource=图标文件,0
```