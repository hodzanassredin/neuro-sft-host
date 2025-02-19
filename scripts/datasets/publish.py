import os
import argparse
from datasets import load_dataset, Dataset

def load_texts(dataset_path):
    """
    Load text files from the specified dataset path.

    Args:
        dataset_path (str): The path to the dataset directory.

    Returns:
        tuple: A tuple containing file names and their corresponding texts.
    """
    file_names = []
    for subdir, dirs, files in os.walk(dataset_path):
        if "/Rsrc/" in subdir:
            continue
        for file in files:
            file_names.append(os.path.join(subdir, file))

    texts = []
    for f in file_names:
        with open(f, 'r', encoding='utf-8') as file:
            text = file.read()
            texts.append(text)

    return file_names, texts

def create_dataset(dataset_path, dataset_name):
    """
    Create a dataset from text files and push it to the Hugging Face Hub.

    Args:
        dataset_path (str): The path to the dataset directory.
        dataset_name (str): The name of the dataset to push to the Hub.
    """
    names, texts = load_texts(dataset_path)
    names = [n[len(dataset_path)+1:] for n in names]
    text_dataset = Dataset.from_dict({"texts": texts, "names": names})
    text_dataset.push_to_hub(dataset_name)

def load_and_push_json_dataset(dataset_path, dataset_name):
    """
    Load a JSON dataset and push it to the Hugging Face Hub.

    Args:
        dataset_path (str): The path to the JSON dataset file.
        dataset_name (str): The name of the dataset to push to the Hub.
    """
    dataset = load_dataset("json", data_files=dataset_path)
    dataset['train'].push_to_hub(dataset_name)

def main(text_dataset_path, text_dataset_name, json_dataset_path, json_dataset_name):
    """
    Main function to create and push datasets to the Hugging Face Hub.

    Args:
        text_dataset_path (str): The path to the text dataset directory.
        text_dataset_name (str): The name of the text dataset to push to the Hub.
        json_dataset_path (str): The path to the JSON dataset file.
        json_dataset_name (str): The name of the JSON dataset to push to the Hub.
    """
    create_dataset(text_dataset_path, text_dataset_name)
    load_and_push_json_dataset(json_dataset_path, json_dataset_name)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Push datasets to the Hugging Face Hub.")
    parser.add_argument("--text_dataset_path", required=True, help="Path to the text dataset directory.")
    parser.add_argument("--text_dataset_name", required=True, help="Name of the text dataset to push to the Hub.")
    parser.add_argument("--json_dataset_path", required=True, help="Path to the JSON dataset file.")
    parser.add_argument("--json_dataset_name", required=True, help="Name of the JSON dataset to push to the Hub.")

    args = parser.parse_args()

    main(args.text_dataset_path, args.text_dataset_name, args.json_dataset_path, args.json_dataset_name)
