HF_HUB_ENABLE_HF_TRANSFER=1 huggingface-cli download hodza/BlackBox-Coder-3B-GGUF --include "BlackBox-Coder-3B-Q8.gguf" --local-dir $1
HF_HUB_ENABLE_HF_TRANSFER=1 huggingface-cli download hodza/BlackBox-Coder-3B-GGUF --include "BlackBox-Coder-3B-F32.gguf" --local-dir $1
