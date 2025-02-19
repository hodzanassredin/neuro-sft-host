import os
import argparse
import torch
import transformers
import peft
import datasets
import evaluate
import time
import math

def parse_arguments():
    parser = argparse.ArgumentParser(description='Train a QA model using transformers and PEFT.')
    parser.add_argument('--dataset_path', type=str, required=True, help='Path to the dataset file.')
    parser.add_argument('--model_name', type=str, required=True, help='Name of the model to use.')
    parser.add_argument('--model_path', type=str, required=True, help='Path to the model directory.')
    parser.add_argument('--epochs', type=int, default=7, help='Number of training epochs.')
    parser.add_argument('--output_dir', type=str, default='./results_stage2', help='Directory to save results.')
    parser.add_argument('--log_dir', type=str, default='./logs', help='Directory to save logs.')
    return parser.parse_args()

def setup_device():
    assert torch.cuda.is_available(), "CUDA is required for this script."
    return torch.device('cuda:0')

def load_model_and_tokenizer(model_path, device):
    bnb_config = transformers.BitsAndBytesConfig(
        load_in_4bit=True,
        bnb_4bit_quant_type="nf4",
        bnb_4bit_use_double_quant=True,
        bnb_4bit_compute_dtype=torch.bfloat16
    )
    tokenizer = transformers.AutoTokenizer.from_pretrained(model_path)
    peft_config = peft.PeftConfig.from_pretrained(model_path)
    model = transformers.AutoModelForCausalLM.from_pretrained(model_path, device_map=device, quantization_config=bnb_config)
    model = peft.PeftModel.from_pretrained(model, model_path, is_trainable=True)
    model.print_trainable_parameters()
    return model, tokenizer

def load_dataset(dataset_path):
    return datasets.load_dataset("json", data_files=dataset_path)['train']

def format_chat(example, tokenizer):
    chat = [
        {"role": "user", "content": example["Question"]},
        {"role": "assistant", "content": example["Answer"]}
    ]
    formatted_chat = tokenizer.apply_chat_template(chat, tokenize=False)
    return {"formatted_chat": formatted_chat}

def tokenize_dataset(dataset, tokenizer):
    def tokenize_function(examples):
        res = tokenizer(examples["formatted_chat"], truncation=True, padding="max_length", max_length=512)
        res["labels"] = res["input_ids"].copy()
        return res
    return dataset.map(tokenize_function, batched=True)

def split_dataset(dataset):
    return dataset.train_test_split(test_size=0.1).remove_columns(['Source', 'Question', 'Answer', 'formatted_chat'])

def setup_training_args(output_dir, log_dir, epochs):
    timestr = time.strftime("%Y%m%d-%H%M%S")
    return transformers.TrainingArguments(
        output_dir=f"{output_dir}/{model_name}/{timestr}/",
        eval_strategy="epoch",
        save_strategy="epoch",
        learning_rate=5e-5,
        per_device_train_batch_size=2,
        per_device_eval_batch_size=2,
        bf16=torch.cuda.get_device_capability(torch.cuda.current_device())[0] >= 8,
        gradient_accumulation_steps=4,
        num_train_epochs=epochs,
        weight_decay=0.01,
        logging_dir=log_dir,
        logging_steps=32,
        warmup_steps=100,
    )

def train_model(model, tokenizer, dataset, training_args):
    data_collator = transformers.DataCollatorForLanguageModeling(tokenizer=tokenizer, mlm=False)
    trainer = transformers.Trainer(
        model=model,
        args=training_args,
        train_dataset=dataset["train"],
        eval_dataset=dataset["test"],
        data_collator=data_collator,
    )
    trainer.train()
    return trainer

def evaluate_model(trainer):
    eval_results = trainer.evaluate()
    perplexity = math.exp(eval_results['eval_loss'])
    print(f"Perplexity: {perplexity:.2f}")
    return perplexity

def save_model(model, tokenizer, output_dir, timestr):
    final_path = f"{output_dir}/{model_name}_{timestr}"
    model.save_pretrained(final_path)
    tokenizer.save_pretrained(final_path)

def main():
    args = parse_arguments()
    device = setup_device()
    model, tokenizer = load_model_and_tokenizer(args.model_path, device)
    dataset = load_dataset(args.dataset_path)
    dataset = dataset.map(lambda x: format_chat(x, tokenizer))
    dataset = tokenize_dataset(dataset, tokenizer)
    dataset = split_dataset(dataset)
    training_args = setup_training_args(args.output_dir, args.log_dir, args.epochs)
    trainer = train_model(model, tokenizer, dataset, training_args)
    evaluate_model(trainer)
    save_model(model, tokenizer, args.output_dir, time.strftime("%Y%m%d-%H%M%S"))

if __name__ == "__main__":
    main()
