import os
import subprocess

def fix(fname, resfname):
    with open(fname, 'rb') as f:
        str = f.read().decode('utf-8')
    fixed = bytes([ord(a) for a in str if ord(a) < 256]).decode('cp1251')
    with open(resfname, 'w', encoding='utf-8') as f:
        f.write(fixed)

def convert_files(source_dir, dest_dir):
    # Проверяем, существует ли директория назначения, если нет, создаем ее
    if not os.path.exists(dest_dir):
        os.makedirs(dest_dir)

    # Рекурсивно обходим все файлы и поддиректории в исходной директории
    for root, dirs, files in os.walk(source_dir):
        for filename in files:
            if filename.endswith('.odc'):
                source_file = os.path.join(root, filename)
                print(f"converting {source_file}")
                # Создаем соответствующую структуру директорий в директории назначения
                relative_path = os.path.relpath(root, source_dir)
                dest_subdir = os.path.join(dest_dir, relative_path)
                if not os.path.exists(dest_subdir):
                    os.makedirs(dest_subdir)
                dest_file = os.path.join(dest_subdir, filename)
                print(f"converting {source_file}")
                fix(source_file, dest_file)

# Пример использования функции
source_directory = './out'
dest_directory = './fixed_out'
convert_files(source_directory, dest_directory)