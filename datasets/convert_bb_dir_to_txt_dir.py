import os
import subprocess

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

                # Вызываем утилиту odcey для конвертации файла
                subprocess.run(['odcey', 'text', source_file, dest_file])

# Пример использования функции
source_directory = './'
destination_directory = './out'
convert_files(source_directory, destination_directory)