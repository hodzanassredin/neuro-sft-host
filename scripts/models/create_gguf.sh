docker run --rm -v $1/models:/models ghcr.io/ggml-org/llama.cpp:full --convert BlackBox-Coder-3B-merged --outfile BlackBox-Coder-3B-Q8.gguf --outtype q8_0
docker run --rm -v $1/models:/models ghcr.io/ggml-org/llama.cpp:full --convert BlackBox-Coder-3B-merged  --outfile BlackBox-Coder-3B-F32.gguf --outtype f32
docker run --rm -v $1/models:/models ghcr.io/ggml-org/llama.cpp:full --convert BlackBox-Coder-3B-merged  --outfile BlackBox-Coder-3B-F16.gguf --outtype f16
