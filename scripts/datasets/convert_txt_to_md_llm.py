import os
import torch
import transformers
import argparse


def parse_arguments():
    parser = argparse.ArgumentParser(
        description="Process and convert text files to Markdown format using a language model.")
    parser.add_argument(
        '--dataset_path',
        type=str, 
        required=True, 
        help='Path to the dataset directory.')
    parser.add_argument(
        '--fixed_dataset_path',
        type=str,
        required=True,
        help='Path to save the formatted Markdown files.')
    parser.add_argument(
        '--model_name',
        type=str,
        default="Qwen/Qwen2.5-Coder-7B-Instruct",
        help='Name of the model to use.')
    return parser.parse_args()


def load_texts(dataset_path):
    """
    Load text files from the dataset directory.

    Args:
        dataset_path (str): Path to the dataset directory.

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


def initialize_model(model_name, device):
    """
    Initialize the language model and tokenizer.

    Args:
        model_name (str): Name of the model to use.
        device (torch.device): Device to use for the model.

    Returns:
        tuple: A tuple containing the tokenizer and the model.
    """
    bnb_config = transformers.BitsAndBytesConfig(
        load_in_4bit=True,
        bnb_4bit_quant_type="nf4",
        bnb_4bit_use_double_quant=True,
        bnb_4bit_compute_dtype=torch.bfloat16
    )

    tokenizer = transformers.AutoTokenizer.from_pretrained(model_name)
    model = transformers.AutoModelForCausalLM.from_pretrained(
        model_name, device_map=device, quantization_config=bnb_config)
    return tokenizer, model


def rewrite(text, tokenizer, model, device, system_prompt):
    """
    Rewrite the text using the language model.

    Args:
        text (str): The input text to rewrite.
        tokenizer (transformers.AutoTokenizer): The tokenizer to use.
        model (transformers.AutoModelForCausalLM): The model to use.
        device (torch.device): The device to use for the model.
        system_prompt (str): The system prompt to use.

    Returns:
        str: The rewritten text.
    """
    messages = [
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": text}
    ]
    text = tokenizer.apply_chat_template(
        messages,
        tokenize=False,
        add_generation_prompt=True
    )
    model_inputs = tokenizer([text], return_tensors="pt").to(device)

    generated_ids = model.generate(
        model_inputs.input_ids,
        max_new_tokens=4096,
        temperature=0.1
    )
    generated_ids = [
        output_ids[len(input_ids):]
        for input_ids, output_ids in zip(model_inputs.input_ids, generated_ids)
    ]

    response = tokenizer.batch_decode(
        generated_ids, skip_special_tokens=True)[0]
    return response


def main():
    args = parse_arguments()
    device = torch.device('cuda:0' if torch.cuda.is_available() else 'cpu')
    assert torch.cuda.is_available(), "CUDA is required for this script."

    names, texts = load_texts(args.dataset_path)
    tokenizer, model = initialize_model(args.model_name, device)

    system_prompt = "Перепеши документ в формате markdown. Будь точен, ничего не изменяй. Выделяй найденные блоки кода. Отформатируй код табуляцией"

    for i in range(len(names)):
        out_name = names[i].replace(args.dataset_path, args.fixed_dataset_path) + '.md'
        if os.path.isfile(out_name):
            print('Already done: ' + out_name)
            continue
        os.makedirs(os.path.dirname(out_name), exist_ok=True)
        text = rewrite(texts[i], tokenizer, model, device, system_prompt)
        print(out_name)
        with open(out_name, 'w', encoding='utf-8') as f:
            f.write(text)


if __name__ == "__main__":
    main()
