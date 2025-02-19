import os
import argparse
import torch
import transformers
import peft
import datasets
import evaluate
import time

def parse_arguments():
    parser = argparse.ArgumentParser(description='Merge and save a pre-trained model.')
    parser.add_argument('--model_name', type=str, default='hodza/BlackBox-Coder-3B', help='Name of the model to load.')
    parser.add_argument('--base_model_name', type=str, default='Qwen/Qwen2.5-Coder-3B-Instruct', help='Name of the base model to load.')
    parser.add_argument('--merged_model_path', type=str, default='/app/models/BlackBox-Coder-3B-merged', help='Path to save the merged model.')
    return parser.parse_args()

def load_tokenizer(base_model_name):
    return transformers.AutoTokenizer.from_pretrained(base_model_name)

def load_base_model(base_model_name):
    return transformers.AutoModelForCausalLM.from_pretrained(
        base_model_name,
        low_cpu_mem_usage=True,
        return_dict=True,
        torch_dtype=torch.bfloat16,
        device_map="auto"
    )

def load_peft_model(base_model, model_name):
    return peft.PeftModel.from_pretrained(base_model, model_name)

def merge_and_save_model(model, merged_model_path, tokenizer):
    merged_model = model.merge_and_unload()
    merged_model.save_pretrained(merged_model_path)
    tokenizer.save_pretrained(merged_model_path)

def main():
    args = parse_arguments()

    tokenizer = load_tokenizer(args.base_model_name)
    base_model = load_base_model(args.base_model_name)
    model = load_peft_model(base_model, args.model_name)

    merge_and_save_model(model, args.merged_model_path, tokenizer)

if __name__ == "__main__":
    main()
