# neuro-sft-host
Just an exam work for llm sft and hosting

# Результаты

[Презентация](./docs/presentation.md)

[Опубликованный датасет](https://huggingface.co/datasets/hodza/BlackBox.Shkola.2014)

[Опубликованная модель](https://huggingface.co/hodza/BlackBox-Coder-3B)

[@blackbox_cp_helper_bot](https://t.me/blackbox_cp_helper_bot)

# Ollama

**ollama run hodza/BlackBox-Coder-3B:Q8**

**ollama run hodza/BlackBox-Coder-3B:F32**

# Scripts

Setup

```
./1_setup.sh
./1.1_build_docker.sh
./1.2_build_dotnet.sh
```

Create datasets

```./2_create_forum_dataset.sh
./2_create_pdf_dataset.sh
./2_create_text_dataset.sh
```

Train
```
./2_pretrain.sh
```

Eval
```
./3_run_notebooks.sh
```
# Обучение
Используется базовая модель QwenCoder2.5-Instruct 3B. И обучается Qlora через SFT.

[Ноутбук](./labs/notebooks/pretrain.ipynb)

# Бот для телеграм

[Код](./src/Llm/LlmTelegramBot/)

# UI

Бекенд подключается к ллм через openai ендпоинт. Фронтенд на web assembly работает с бекенд через web sockets.

[Бекенд](./src/Llm/LllmBackend/)

[Фронтенд](./src/Llm/LlmFrontend/)

# Хостинг

Используется VLLM [docker compose](./docker-compose.yml)
