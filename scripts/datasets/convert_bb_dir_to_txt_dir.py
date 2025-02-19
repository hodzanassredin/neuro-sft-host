import os
import subprocess
import argparse

def convert_files(source_dir, dest_dir):
    """
    Convert .odc files in the source directory to text files in the destination directory.

    Args:
    - source_dir (str): The path to the source directory containing .odc files.
    - dest_dir (str): The path to the destination directory where converted files will be saved.
    """
    # Check if the destination directory exists, if not, create it
    if not os.path.exists(dest_dir):
        os.makedirs(dest_dir)

    # Recursively walk through all files and subdirectories in the source directory
    for root, dirs, files in os.walk(source_dir):
        for filename in files:
            if filename.endswith('.odc'):
                source_file = os.path.join(root, filename)
                print(f"Converting {source_file}")

                # Create corresponding directory structure in the destination directory
                relative_path = os.path.relpath(root, source_dir)
                dest_subdir = os.path.join(dest_dir, relative_path)
                if not os.path.exists(dest_subdir):
                    os.makedirs(dest_subdir)

                dest_file = os.path.join(dest_subdir, filename  + '.txt')

                # Call the odcey utility to convert the file
                subprocess.run(['odcey', 'text', source_file, dest_file])

def main():
    # Set up argument parsing
    parser = argparse.ArgumentParser(description="Convert .odc files to text files.")
    parser.add_argument('source_dir', type=str, help="The source directory containing .odc files.")
    parser.add_argument('dest_dir', type=str, help="The destination directory for converted files.")

    # Parse the arguments
    args = parser.parse_args()

    # Call the conversion function with the provided arguments
    convert_files(args.source_dir, args.dest_dir)

if __name__ == "__main__":
    main()
