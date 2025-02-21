import os
import torch
import transformers
import peft
import datasets
import time
import logging
import math
import argparse
from clearml import Task

# Set up logging
logging.basicConfig(level=logging.DEBUG, format='%(asctime)s %(message)s')

# Ensure CUDA is available
assert torch.cuda.is_available(), "CUDA is required for this script"

# Set device
device = torch.device('cuda:0')

# setup clearml
task = Task.init(project_name="BlackBoxCoder", task_name="Train")

# Constants
BLOCK_SIZE = 512
STRIDE = 32


def parse_arguments():
    parser = argparse.ArgumentParser(
        description="Train a language model with PEFT and BitsAndBytesConfig.")
    parser.add_argument("--model_name", type=str,
                        required=True, 
                        help="Name of the model to use")
    parser.add_argument("--models_path", type=str,
                        default="./models",
                        help="Path to models dir")
    parser.add_argument("--dataset_path", type=str,
                        required=True, 
                        help="Path to the dataset")
    parser.add_argument("--epochs", type=int,
                        default=7, 
                        help="Number of training epochs")
    parser.add_argument("--output_dir", type=str,
                        default="./models/checkpoints",
                        help="Directory to save results")
    return parser.parse_args()


def tokenize_with_stride(texts, tokenizer, block_size=BLOCK_SIZE,
                         stride=STRIDE):
    tokenized_data = tokenizer(
        texts,
        max_length=block_size,
        truncation=True,
        stride=stride,
        return_overflowing_tokens=True,
        padding="max_length",
        return_tensors="pt",
        add_special_tokens=False,
    )
    tokenized_data["labels"] = tokenized_data["input_ids"]
    return datasets.Dataset.from_dict(tokenized_data).train_test_split(
        test_size=0.1, shuffle=False)


def load_model_and_tokenizer(model_name, device, bnb_config, peft_config):
    tokenizer = transformers.AutoTokenizer.from_pretrained(model_name)
    model = transformers.AutoModelForCausalLM\
        .from_pretrained(model_name, device_map=device, 
                         quantization_config=bnb_config)
    model._hf_peft_config_loaded = True  # silence a warning from HF trainer
    model = peft.get_peft_model(model, peft_config)
    model.print_trainable_parameters()
    return model, tokenizer

# def compute_metrics(eval_pred, metric):
#     logits, labels = eval_pred
#     predictions = np.argmax(logits, axis=-1)
#     return metric.compute(predictions=predictions, references=labels)


def main():
    args = parse_arguments()

    # Configuration for BitsAndBytes and PEFT
    bnb_config = transformers.BitsAndBytesConfig(
        load_in_4bit=True,
        bnb_4bit_quant_type="nf4",
        bnb_4bit_use_double_quant=True,
        bnb_4bit_compute_dtype=torch.bfloat16
    )
    peft_config = peft.LoraConfig(
        r=16,
        lora_alpha=32,
        target_modules=[
            "mlp.down_proj",
            "self_attn.k_proj",
            "self_attn.o_proj",
            "mlp.up_proj",
            "self_attn.v_proj",
            "mlp.gate_proj",
            "self_attn.q_proj"
        ],
        lora_dropout=0.1,
        bias="none",
        task_type=peft.TaskType.CAUSAL_LM
    )
    dataset_name = os.path.basename(args.dataset_path)
    only_model_name = args.model_name.split("/")[-1].replace(':', "_")
    # Load model and tokenizer
    model, tokenizer = load_model_and_tokenizer(args.model_name, device, 
                                                bnb_config, peft_config)

    # Prepare dataset
    dataset = datasets.load_dataset(args.dataset_path)
    texts = dataset["train"]["texts"]
    lm_datasets = tokenize_with_stride(texts, tokenizer)
    # metric = evaluate.load("bleu")

    # Training arguments
    timestr = time.strftime("%Y%m%d-%H%M%S")
    data_collator = transformers.DataCollatorForLanguageModeling(
        tokenizer=tokenizer, mlm=False)
    training_args = transformers.TrainingArguments(
        output_dir=f"{args.output_dir}/{dataset_name}_{only_model_name}/\
            {timestr}/",
        eval_strategy="epoch",
        save_strategy="epoch",
        learning_rate=2e-5,
        per_device_train_batch_size=2,
        per_device_eval_batch_size=2,
        bf16=torch.cuda.get_device_capability(
            torch.cuda.current_device())[0] >= 8,
        gradient_accumulation_steps=4,
        num_train_epochs=args.epochs,
        weight_decay=0.01,
        logging_dir='./logs',
        logging_steps=32,
        warmup_steps=100,
    )

    # Trainer
    trainer = transformers.Trainer(
        model=model,
        args=training_args,
        train_dataset=lm_datasets["train"],
        eval_dataset=lm_datasets["test"],
        data_collator=data_collator,
        # compute_metrics=lambda eval_pred: compute_metrics(eval_pred, metric),
    )

    # Train the model
    trainer.train()

    # Evaluate the model
    eval_results = trainer.evaluate()
    print(f"Perplexity: {math.exp(eval_results['eval_loss']):.2f}")

    final_path = os.path.join(args.models_path,
                              f"{dataset_name}_{only_model_name}_{timestr}")
    # Save the model
    model.save_pretrained(final_path)
    tokenizer.save_pretrained(final_path)


if __name__ == "__main__":
    main()
