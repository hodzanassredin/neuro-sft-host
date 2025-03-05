import os
import argparse
import re


def fix(fname, resfname):
    """
    Fixes the encoding of a file by converting it from UTF-8 to CP1251,
    removing any characters with an ordinal value greater than 255.

    Args:
        fname (str): The path to the input file.
        resfname (str): The path to the output file.
    """
    with open(fname, 'rb') as f:
        content = f.read().decode('utf-8')

    # Filter out characters with ordinal values greater than 255
    fixed_content = bytes(
         [ord(char) for char in content if ord(char) < 256]).decode('cp1251')
    fixed_content = re.sub(
        r'[\x00-\x08\x0b\x0c\x0e-\x1f\x7f-\xff]', '', fixed_content)
    with open(resfname, 'w', encoding='utf-8') as f:
        f.write(fixed_content)


def convert_files(source_dir, dest_dir):
    """
    Converts all '.odc' files in the source directory 
    to the destination directory
    with fixed encoding.

    Args:
        source_dir (str): The path to the source directory.
        dest_dir (str): The path to the destination directory.
    """
    # Check if the destination directory exists, create it if it doesn't
    if not os.path.exists(dest_dir):
        os.makedirs(dest_dir)

    # Recursively walk through all files and subdirectories
    # in the source directory
    for root, _, files in os.walk(source_dir):
        for filename in files:
            if filename.endswith('.odc'):
                source_file = os.path.join(root, filename)
                print(f"Fix encoding {source_file}")

                # Create corresponding directory structure 
                # in the destination directory
                relative_path = os.path.relpath(root, source_dir)
                dest_subdir = os.path.join(dest_dir, relative_path)
                if not os.path.exists(dest_subdir):
                    os.makedirs(dest_subdir)

                dest_file = os.path.join(dest_subdir, filename)
                fix(source_file, dest_file)


def main():
    """
    Main function to parse command-line arguments and convert files.
    """
    parser = argparse.ArgumentParser(
        description='Convert .odc files with fixed encoding.')
    parser.add_argument(
        'source_dir',
        type=str,
        help='Source directory containing .odc files.')
    parser.add_argument(
        'dest_dir',
        type=str,
        help='Destination directory for converted files.')

    args = parser.parse_args()
    convert_files(args.source_dir, args.dest_dir)


if __name__ == "__main__":
    main()
