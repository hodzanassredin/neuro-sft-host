# neuro-sft-host
Just an exam work for llm sft and hosting

# TODO
не решена проблема со стабильностью ответов. Надо возможно обучить модель большего размера. Это пока мешает деплою бота и финальной версии.


[Презентация](./docs/presentation.md)

# Сбор датасетов

[Кравлер для форума](./src/Llm/Crawler)

[Синтезатор вопросов и ответов из документации](./src/Llm/DatasetGenerator/)

[Конвертер системы документации и кода BlackBox в txt файлы для датасета](./datasets/convert_bb_dir_to_txt_dir.py)

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