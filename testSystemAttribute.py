import ctypes
import os
from src.RainbowColorizer import RC
from anytree import Node, RenderTree


def Colorif(text,A):
    if A:
        return RC.color(f"[gn]{text}")
    else:
        return RC.color(f"[rd]{text}")

    

def get_file_attributes(file_path):
    FILE_ATTRIBUTE_READONLY = 0x00000001
    FILE_ATTRIBUTE_HIDDEN = 0x00000002
    FILE_ATTRIBUTE_SYSTEM = 0x00000004

    try:
        attributes = ctypes.windll.kernel32.GetFileAttributesW(file_path)
        if attributes == -1:
            raise Exception("无法获取文件属性")

        is_readonly = bool(attributes & FILE_ATTRIBUTE_READONLY)
        is_hidden = bool(attributes & FILE_ATTRIBUTE_HIDDEN)
        is_system = bool(attributes & FILE_ATTRIBUTE_SYSTEM)

        return is_readonly, is_hidden, is_system
    except Exception as e:
        print(f"错误: {e}")
        return False, False, False

def build_and_print_files(startpath):
    root = Node(startpath)
    nodes = {startpath: root}

    def add_file(path):
        rel_path = os.path.relpath(path, startpath)
        parts = rel_path.split(os.sep)
        current_node = root
        for part in parts:
            if part not in (current_node.children.keys()):
                new_node = Node(part, parent=current_node)
                current_node.children[part] = new_node
            current_node = current_node.children[part]
    c = ""
    d = ""
    e = 3
    for root_dir, dirs, files in os.walk(startpath):
        for file in files:
            e += 1
            file_path = os.path.join(root_dir, file)
            readonly, hidden, system = get_file_attributes(file_path)
            #print(file_path+Colorif("  只读 ",readonly)+Colorif("隐藏 ",hidden)+Colorif("系统",system))  # 直接打印文件路径
            c += file_path+"\n"
            d += Colorif("  只读 ",readonly)+Colorif("隐藏 ",hidden)+Colorif("系统          ",system)+"\n"
            
    print(RC.joinH(c,d)+("\033[A"*e)+"\x1b[?25l")


startpath = 'test'
# 之前用的时候遇到了错误将目标文件夹内所有文件设置为系统属性，但我没复现出来

while True:
    build_and_print_files(startpath)
