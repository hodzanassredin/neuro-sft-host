docker run --runtime nvidia --gpus all \
    -v ~/.cache/huggingface:/root/.cache/huggingface \
    --env "HUGGING_FACE_HUB_TOKEN=${HF_TOKEN}" \
    -p 9999:8000 \
    --ipc=host \
    vllm/vllm-openai:latest \
    --model ministral/Ministral-3b-instruct \
    --quantization fp8 --gpu_memory_utilization 0.90
