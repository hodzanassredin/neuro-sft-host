import os
import json
import requests
import argparse

# Define the prompt for generating Q&A pairs
prompt = '''
Read the following documentation and generate the maximum number of question-answer pairs based on the information in the text. Present the result in a JSON array format with objects containing `question` and `answer` fields.
Do not add markdown formatting. The output should be machine-readable JSON. Answers should include code examples and be highlighted using markdown. Escape quotes.

**Documentation:**

Python is a high-level, general-purpose programming language. It was created by Guido van Rossum and first released in 1991. Python supports multiple programming paradigms, including object-oriented, imperative, and functional programming. It is known for its simplicity and code readability.
A simple Python program:

print("Hello, world!")

**Output format:**
[
    {"question": "What is Python?", "answer": "Python is a high-level, general-purpose programming language."},
    {"question": "Who created Python?", "answer": "Guido van Rossum"},
    {"question": "When was Python first released?", "answer": "In 1991"},
    {"question": "What programming paradigms does Python support?", "answer": "Object-oriented, imperative, and functional programming"},
    {"question": "What is Python known for?", "answer": "Simplicity and code readability"}
]
'''

# Function to split text into chunks based on empty lines
def split_text_into_chunks(text, split_lines=2, min_lines=3):
    """
    Splits the input text into chunks based on empty lines.

    Args:
        text (str): The input text to be split.
        split_lines (int): Number of consecutive empty lines to split on.
        min_lines (int): Minimum number of lines required for a chunk to be valid.

    Returns:
        list: A list of text chunks.
    """
    lines = text.split('\n')  # Split text into lines
    chunks = []  # List to store chunks
    current_chunk = []  # Temporary list for the current chunk
    empty_line_count = 0  # Counter for consecutive empty lines

    for line in lines:
        if line.strip() == '':
            empty_line_count += 1
        else:
            empty_line_count = 0

        if empty_line_count == split_lines:
            # If we find `split_lines` consecutive empty lines, finalize the current chunk
            if current_chunk:
                if len(current_chunk) >= min_lines:
                    chunks.append('\n'.join(current_chunk))
                current_chunk = []
            empty_line_count = 0
        else:
            current_chunk.append(line)  # Add line to the current chunk

    # Add the last chunk if it's not empty
    if current_chunk and len(current_chunk) >= min_lines:
        chunks.append('\n'.join(current_chunk))

    return chunks

# Function to generate Q&A pairs using an API
def generate_qa_pairs(text, api_key, endpoint, model_name):
    """
    Sends a request to the API to generate Q&A pairs from the input text.

    Args:
        text (str): The input text to generate Q&A pairs from.
        api_key (str): The API key for authentication.
        endpoint (str): The API endpoint URL.
        model_name (str): The name of the model to use.

    Returns:
        dict: The JSON response from the API.

    Raises:
        Exception: If the API request fails.
    """
    url = endpoint  # API endpoint URL
    headers = {
        "Authorization": f"Bearer {api_key}",
        "Content-Type": "application/json"
    }
    data = {
        "model": model_name,  # Model to use
        "messages": [
            {"role": "system", "content": prompt},
            {"role": "user", "content": text}
        ],
        "response_format": {"type": "json_object"}
    }
    response = requests.post(url, headers=headers, json=data)
    if response.status_code == 200:
        return response.json()  # Return the JSON response
    else:
        raise Exception(f"Error {response.status_code}: {response.text}")

# Function to process text files in a directory
def process_text_files(directory, api_key, endpoint, model_name, output_dir):
    """
    Processes all text files in the specified directory and generates Q&A pairs.

    Args:
        directory (str): Path to the directory containing text files.
        api_key (str): The API key for authentication.
        endpoint (str): The API endpoint URL.
        model_name (str): The name of the model to use.
        output_dir (str): Directory to save the output JSON files.
    """

    os.makedirs(output_dir, exist_ok=True)  # Create output directory if it doesn't exist
    for filename in os.listdir(directory):
        if filename.endswith(".odc"):  # Process only .odc files
            filepath = os.path.join(directory, filename)
            with open(filepath, 'r', encoding='utf-8') as text_file:
                text = text_file.read()
                out = f'{output_dir}/{filename}.jsonl'  # Output file path
                with open(out, 'w', encoding='utf-8') as file:
                    chunks = split_text_into_chunks(text)
                    for i, chunk in enumerate(chunks):
                        try:
                            response = generate_qa_pairs(chunk, api_key, endpoint, model_name)
                            qa_pairs = response['choices'][0]['message']['content']
                            file.write(qa_pairs)  # Write Q&A pairs to the output file
                        except Exception as e:
                            print(f"Error processing file {filepath} chunk {i}: {e}")
            print(f"Wrote {out}")

def main():
    # Set up argument parser
    parser = argparse.ArgumentParser(description="Process text files to generate Q&A pairs.")
    parser.add_argument("directory", help="Path to the directory containing text files.")
    parser.add_argument("output_dir", help="Path to the output directory.")
    parser.add_argument("model_name", help="Name of the model to use.")
    parser.add_argument("endpoint", help="Api endpoint.", default="https://localhost:9999/v1")
    parser.add_argument("api_key", help="Api key.", default="key")
    args = parser.parse_args()

    # Process all text files in the directory
    process_text_files(args.directory, args.api_key, args.endpoint, args.model_name, args.output_dir)

if __name__ == "__main__":
    main()
