FROM huggingface/peft-gpu-bnb-latest:latest

COPY ./requirements.txt /app/requirements.txt
WORKDIR /app
RUN --mount=type=cache,target=/var/cache/apt \
        apt-get update && \
        apt-get install -y wget libsndfile1 cargo rustc
RUN --mount=type=cache,target=/root/.cache/pip \
        pip install -U pip && \
        pip install -r requirements.txt
CMD ["jupyter", "lab", "--allow-root", "--ip=0.0.0.0", "--NotebookApp.token=''", "--NotebookApp.password=''"]
