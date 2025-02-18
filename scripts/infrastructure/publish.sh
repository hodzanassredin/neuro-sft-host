echo $DR_PAT | docker login -u hodzanassredin --password-stdin
echo $CR_PAT | docker login ghcr.io -u hodzanassredin --password-stdin

docker build -f ./src/Llm/LlmTelegramBot/Dockerfile -t ghcr.io/hodzanassredin/code_bot:latest ./src/Llm

docker push ghcr.io/hodzanassredin/code_bot:latest