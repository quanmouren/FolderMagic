from PIL import Image
import io

def image_to_ico(image_path, icon_sizes=[(16, 16), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]):
    """
    将图标文件转换为多尺寸ICO格式,返回一个字节流对象

    :param image_path: 输入图像文件 png or jpg
    :param icon_sizes: ico尺寸列表
    :return: 包含ICO格式的字节流对象
    """
    with Image.open(image_path) as img:
        byte_stream = io.BytesIO()
        img.save(byte_stream, format='ICO', sizes=icon_sizes)
        byte_stream.seek(0)  # 重置字节流位置到开头
        return byte_stream

if __name__ == "__main__":
    import tkinter as tk
    from tkinter import filedialog
    def save_ico_file(byte_stream, file_name):
        """
        将ICO格式的字节流对象保存为文件
        
        :param byte_stream: 包含ICO格式的字节流对象
        :param file_name: 要保存的文件名
        """
        with open(f"{file_name}.ico", "wb") as ico_file:
            ico_file.write(byte_stream.read())
            print(f"Saved ICO file as {file_name}.ico")

    root = tk.Tk()
    root.withdraw()
    file_path = filedialog.askopenfilename(
        title="选择图片",
        filetypes=[("PNG,JPG", "*.png;*.jpg")]
    )

    if file_path:
        byte_stream = image_to_ico(file_path)
        save_ico_file(byte_stream, "output_icon")