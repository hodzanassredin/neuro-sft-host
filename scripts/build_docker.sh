docker build -t sft-bot      -f $1/LlmTelegramBot/Dockerfile $1
docker build -t sft-backend  -f $1/LlmBackend/Dockerfile $1
docker build -t sft-frontend -f $1/LlmFrontend/Dockerfile $1