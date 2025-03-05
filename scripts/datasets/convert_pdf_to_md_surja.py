import argparse
import os
import hashlib
from marker.converters.pdf import PdfConverter
from marker.models import create_model_dict
from marker.output import text_from_rendered


def calculate_hash(file_path):
    """Calculate the SHA-256 hash of a file."""
    sha256 = hashlib.sha256()
    with open(file_path, 'rb') as f:
        while chunk := f.read(8192):
            sha256.update(chunk)
    return sha256.hexdigest()


def pdf_to_text(file_path, converter):
    """Convert a PDF file to text using the provided converter."""
    rendered = converter(file_path)
    text, _, _ = text_from_rendered(rendered)
    return text


def process_pdf_folder(folder_path, output_folder, converter):
    """
    Process a folder of PDF files, converting them 
    to text and excluding duplicates.

    Args:
        folder_path (str): Path to the folder containing PDF files.
        output_folder (str): Path to the folder where text files will be saved.
        converter (PdfConverter): The converter instance 
        to use for PDF to text conversion.
    """
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)

    seen_hashes = set()
    for filename in os.listdir(folder_path):
        if filename.endswith('.pdf'):
            file_path = os.path.join(folder_path, filename)
            file_hash = calculate_hash(file_path)

            if file_hash not in seen_hashes:
                seen_hashes.add(file_hash)
                print(f"proccessing {file_path}")
                try:

                    text = pdf_to_text(file_path, converter)
                    output_file_path = os.path.join(
                        output_folder, f"{os.path.splitext(filename)[0]}.md")
                    with open(output_file_path, 'w', encoding='utf-8') as output_file:
                        output_file.write(text)
                    print(f"Processed: {filename}")
                except:
                    print(f"Cant parse, skipping: {filename}")
            else:
                print(f"Duplicate found, skipping: {filename}")


def main():
    parser = argparse.ArgumentParser(
        description="Process a folder of PDF files to text, excluding duplicates.")
    parser.add_argument(
        'folder_path',
        type=str,
        help="Path to the folder containing PDF files.")
    parser.add_argument(
        'output_folder',
        type=str,
        help="Path to the folder where text files will be saved.")

    args = parser.parse_args()

    converter = PdfConverter(artifact_dict=create_model_dict(),
                             config={})
    process_pdf_folder(args.folder_path, args.output_folder, converter)


if __name__ == "__main__":
    main()
