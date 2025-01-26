# neuro-sft-host
Just an exam work for llm sft and hosting

# TODO

Деплой бота и UI.

# Результаты

[Презентация](./docs/presentation.md)

[Опубликованный датасет](https://huggingface.co/datasets/hodza/BlackBox.Shkola.2014)

[Опубликованная модель](https://huggingface.co/hodza/BlackBox-Coder-3B)

[@blackbox_cp_helper_bot](https://t.me/blackbox_cp_helper_bot)

# Ollama

**ollama run hodza/BlackBox-Coder-3B**

gguf q8 модель работает хуже чем родная hf модель.

# Сбор датасетов

[Кравлер для форума](./src/Llm/Crawler)

[Синтезатор вопросов и ответов из документации](./src/Llm/DatasetGenerator/)

[Конвертер системы документации и кода BlackBox в txt файлы для датасета](./datasets/convert_bb_dir_to_txt_dir.py)



# Обучение
Используется базовая модель QwenCoder2.5 3B. И обучается Qlora через SFT.

[Ноутбук](./learn/notebooks/BB/pretrain.ipynb)

# Бот для телеграм

[Код](./src/Llm/LlmTelegramBot/)

# UI

Бекенд подключается к ллм через openai ендпоинт. Фронтенд на web assembly работает с бекенд через web sockets.

[Бекенд](./src/Llm/LllmBackend/)

[Фронтенд](./src/Llm/LlmFrontend/)

# Хостинг

Используется VLLM [docker compose](./docker-compose.yml)