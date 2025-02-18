
python3 -m venv .hg_download
source .hg_download/bin/activate
python3 -m pip install "huggingface_hub[hf_transfer]"
HF_HUB_ENABLE_HF_TRANSFER=1 huggingface-cli download hodza/BlackBox-Coder-3B-GGUF --include "BlackBox-Coder-3B-Q8.gguf" --local-dir ../../models