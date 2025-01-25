# neuro-sft-host
Just an exam work for llm sft and hosting


[Презентация](./docs/presentation.md)

# Сбор датасетов

[Кравлер для форума](./src/Llm/Crawler)

[Синтезатор вопросов и ответов из документации](./src/Llm/DatasetGenerator/)

# Обучение
Используется базовая модель CotypeNano 1.5B. И обучается Qlora через SFT.

[Ноутбук](./learn/notebooks/BB/pretrain.ipynb)



# Бот для телеграм

Пока не подключен к ллм

[Код](./src/Llm/LlmTelegramBot/)

[@blackbox_cp_helper_bot](https://t.me/blackbox_cp_helper_bot)

# UI

Бекенд подключается к ллм через openai ендпоинт. Фронтенд на web assembly работает с бекенд через web sockets.

[Бекенд](./src/Llm/LllmBackend/)

[Фронтенд](./src/Llm/LlmFrontend/)

# Хостинг

Используется VLLM [docker compose](./docker-compose.yml)