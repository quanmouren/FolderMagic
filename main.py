import tkinter as tk
from tkinter import filedialog, messagebox
from tkinterdnd2 import TkinterDnD, DND_FILES
import os
import shutil
from src.ImageToIco import image_to_ico

def select_folder(folder=None):
    # 清空所有输入框内容
    folder_entry.delete(0, tk.END)
    icon_entry.delete(0, tk.END)
    real_name_entry.delete(0, tk.END)
    name_entry.delete(0, tk.END)
    tip_entry.delete(0, tk.END)
    if not folder:
        folder = filedialog.askdirectory()
    if folder:
        folder_entry.insert(0, folder)
        # 获取ini文件内容
        ini_path = os.path.join(folder, "desktop.ini")
        if os.path.exists(ini_path):
            try:
                os.system(f'attrib -h -s "{ini_path}"')
                with open(ini_path, "r", encoding="mbcs") as f:
                    content = f.read()
                    lines = content.splitlines()
                    localized_name = ""
                    info_tip = ""
                    icon_resource = ""
                    for line in lines:
                        if line.startswith("LocalizedResourceName="):
                            localized_name = line.split("=", 1)[1]
                        elif line.startswith("InfoTip="):
                            info_tip = line.split("=", 1)[1]
                        elif line.startswith("IconResource="):
                            icon_resource = line.split("=", 1)[1].split(",")[0]
                    name_entry.insert(0, localized_name)
                    tip_entry.insert(0, info_tip)
                    if icon_resource:
                        icon_path = os.path.join(folder, icon_resource)
                        icon_entry.insert(0, icon_path)
                os.system(f'attrib +h +s "{ini_path}"')
            except Exception as e:
                messagebox.showerror("错误", f"读取 desktop.ini 文件失败: {str(e)}")
        # 获取真实文件名
        real_name = os.path.basename(folder)
        real_name_entry.insert(0, real_name)

def select_icon(icon_path=None):
    # 清空图标输入框内容
    icon_entry.delete(0, tk.END)
    if not icon_path:
        icon_path = filedialog.askopenfilename(
            filetypes=[("图标文件", "*.ico;*.exe;*.dll;*.png;*.jpg")]
        )
    if icon_path:
        icon_entry.insert(0, icon_path)

def handle_drop(event):
    # 处理拖拽事件
    data = event.data
    # 处理路径格式
    if data.startswith('{') and data.endswith('}'):
        files = [data[1:-1]]
    else:
        files = data.split()
    for path in files:
        path = path.strip()
        if os.path.isdir(path):
            select_folder(path)
            return
        elif os.path.isfile(path) and path.lower().endswith(
                ('.ico', '.exe', '.dll', '.png', '.jpg')):
            select_icon(path)
            return

def generate_ini():
    folder_path = folder_entry.get()
    icon_path = icon_entry.get()
    real_name = real_name_entry.get()
    localized_name = name_entry.get()
    info_tip = tip_entry.get()

    if not folder_path:
        messagebox.showerror("错误", "请先选择文件夹")
        return
    try:
        icon_file = ""
        if icon_path:
            ext = os.path.splitext(icon_path)[1].lower()  # 获取文件扩展名
            # 处理图片格式转换为ICO
            if ext in ('.png', '.jpg', '.jpeg'):
                base_name = os.path.splitext(os.path.basename(icon_path))[0]
                icon_file = base_name + '.ico'
                dest_icon = os.path.join(folder_path, icon_file)
                
                # 处理已存在的ICO文件
                if os.path.exists(dest_icon):
                    try:
                        os.system(f'attrib -h -r "{dest_icon}"')  # 去除隐藏和只读属性
                        os.remove(dest_icon)
                    except Exception as e:
                        messagebox.showerror("错误", f"无法删除旧图标文件: {str(e)}")
                        return
                
                # 生成并保存ICO文件
                try:
                    byte_stream = image_to_ico(icon_path)
                    with open(dest_icon, 'wb') as f:
                        f.write(byte_stream.read())  # 写入字节流内容
                except Exception as e:
                    messagebox.showerror("错误", f"ICO转换失败: {str(e)}")
                    return
                
                # 设置文件属性
                os.system(f'attrib -r "{dest_icon}"')  # 确保文件可写
                os.system(f'attrib +h +r "{dest_icon}"')  # 隐藏+只读
            else:
                # 处理ICO或其他格式文件
                icon_file = os.path.basename(icon_path)
                dest_icon = os.path.join(folder_path, icon_file)
                
                # 处理已存在文件的权限问题
                if os.path.exists(dest_icon):
                    try:
                        os.system(f'attrib -h -r "{dest_icon}"')
                    except Exception as attr_error:
                        messagebox.showerror("错误", f"无法修改文件属性: {str(attr_error)}")
                        return
                
                # 检查是否需要复制
                if os.path.abspath(icon_path) != os.path.abspath(dest_icon):
                    try:
                        shutil.copyfile(icon_path, dest_icon)
                    except PermissionError:
                        try:
                            os.remove(dest_icon)
                            shutil.copyfile(icon_path, dest_icon)
                        except Exception as e:
                            messagebox.showerror("错误", f"文件复制失败: {str(e)}\n请手动删除旧图标文件")
                            return
                # 设置文件属性
                os.system(f'attrib -r "{dest_icon}"')
                os.system(f'attrib +h +r "{dest_icon}"')

        # 创建desktop.ini内容（以下保持原样）
        ini_content = f"""[.ShellClassInfo]
LocalizedResourceName={localized_name}
InfoTip={info_tip}
IconResource={icon_file},0
"""
        # 写入文件
        ini_path = os.path.join(folder_path, "desktop.ini")
        if os.path.exists(ini_path):
            os.system(f'attrib -h -s "{ini_path}"')
        # ANSI编码
        with open(ini_path, "w", encoding="mbcs") as f:
            f.write(ini_content)
        os.system(f'attrib +h +s "{ini_path}"')
        os.system(f'attrib +s "{folder_path}"')
        # 重命名文件夹
        if real_name:
            parent_dir = os.path.dirname(folder_path)
            new_folder_path = os.path.join(parent_dir, real_name)
            os.rename(folder_path, new_folder_path)
        messagebox.showinfo("成功", "配置完成！\n可能需要刷新文件夹查看效果")
    except Exception as e:
        messagebox.showerror("错误", f"操作失败: {str(e)}")

def restore_default():
    # 恢复默认设置
    folder_path = folder_entry.get()
    if not folder_path:
        messagebox.showerror("错误", "请先选择要恢复的文件夹")
        return
    try:
        if not os.path.isdir(folder_path):
            messagebox.showerror("错误", "目标文件夹不存在或不是有效目录")
            return
        # 处理desktop.ini
        ini_path = os.path.join(folder_path, "desktop.ini")
        icon_resource = None
        # 读取现有配置获取图标文件
        if os.path.exists(ini_path):
            try:
                # 去除文件属性并读取
                os.system(f'attrib -h -s -r "{ini_path}"')
                with open(ini_path, "r", encoding="mbcs") as f:
                    for line in f:
                        if line.startswith("IconResource="):
                            icon_resource = line.split("=", 1)[1].split(",", 1)[0].strip()
                            break
                # 删除ini文件
                os.remove(ini_path)
            except Exception as e:
                messagebox.showerror("错误", f"删除配置文件失败: {str(e)}")
                return
        # 处理关联的图标文件
        if icon_resource:
            icon_path = os.path.join(folder_path, icon_resource)
            if os.path.exists(icon_path):
                try:
                    os.system(f'attrib -h -r "{icon_path}"')  # 去除属性
                    os.remove(icon_path)
                except Exception as e:
                    messagebox.showwarning("警告", f"图标文件删除失败: {str(e)}")
        # 恢复文件夹属性
        os.system(f'attrib -s "{folder_path}"')
        messagebox.showinfo("完成", "已恢复默认设置\n可能需要刷新文件夹查看效果")
    except Exception as e:
        messagebox.showerror("错误", f"恢复过程中发生意外错误: {str(e)}")

root = TkinterDnD.Tk()
root.title("文件夹图标配置工具")
root.geometry("600x270")
# 注册拖放事件
root.drop_target_register(DND_FILES)
root.dnd_bind('<<Drop>>', handle_drop)

tk.Label(root, text="目标文件夹:").grid(row=0, column=0, padx=5, pady=5, sticky="e")
folder_entry = tk.Entry(root, width=50)
folder_entry.grid(row=0, column=1, padx=5, pady=5)
folder_entry.drop_target_register(DND_FILES)
folder_entry.dnd_bind('<<Drop>>', lambda e: select_folder(e.data.strip("{}")))
tk.Button(root, text="浏览...", command=select_folder).grid(row=0, column=2, padx=5)
tk.Label(root, text="图标文件:").grid(row=1, column=0, padx=5, pady=5, sticky="e")
icon_entry = tk.Entry(root, width=50)
icon_entry.grid(row=1, column=1, padx=5, pady=5)
icon_entry.drop_target_register(DND_FILES)
icon_entry.dnd_bind('<<Drop>>', lambda e: select_icon(e.data.strip("{}")))
tk.Button(root, text="浏览...", command=select_icon).grid(row=1, column=2, padx=5)
tk.Label(root, text="真实文件名:").grid(row=2, column=0, padx=5, pady=5, sticky="e")
real_name_entry = tk.Entry(root, width=50)
real_name_entry.grid(row=2, column=1, columnspan=2, padx=5, pady=5, sticky="we")
tk.Label(root, text="显示名称:").grid(row=3, column=0, padx=5, pady=5, sticky="e")
name_entry = tk.Entry(root, width=50)
name_entry.grid(row=3, column=1, columnspan=2, padx=5, pady=5, sticky="we")
tk.Label(root, text="提示信息:").grid(row=4, column=0, padx=5, pady=5, sticky="e")
tip_entry = tk.Entry(root, width=50)
tip_entry.grid(row=4, column=1, columnspan=2, padx=5, pady=5, sticky="we")
tk.Button(root, text="生成配置文件", command=generate_ini,
          bg="#4CAF50", fg="white").grid(row=5, column=1, pady=(15, 10), sticky="we")
tk.Button(root, text="恢复默认设置", command=restore_default,
          bg="#f44336", fg="white").grid(row=6, column=1, pady=(0, 5), sticky="we")

root.columnconfigure(1, weight=1)
root.mainloop()