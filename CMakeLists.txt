cmake_minimum_required(VERSION 3.10)
project(NeuroSft)

set(DATASETS_DIR "${CMAKE_SOURCE_DIR}/datasets")
set(MODELS_DIR "${CMAKE_SOURCE_DIR}/models")
set(SCRIPTS_DIR "${CMAKE_SOURCE_DIR}/scripts")
set(SRC_DIR "${CMAKE_SOURCE_DIR}/src/Llm")
# Specify the MODEL_NAME variable, which can be passed when calling cmake
set(MODEL_NAME "Qwen/Qwen2.5-Coder-3B-Instruct" CACHE STRING "Name of the model to train")
set(DATASET_NAME "hodza/BlackBox.Shkola.2014" CACHE STRING "Name of the dataset to train")

find_package(Python REQUIRED)
find_program(DOTNET_EXE NAMES "dotnet" PATHS "$ENV{ProgramFiles}/dotnet" "$ENV{HOME}/.dotnet/tools")
if (NOT DOTNET_EXE)
    message(FATAL_ERROR "dotnet CLI not found! https://dotnet.microsoft.com/en-us/download/dotnet/9.0")
endif()


# Путь к вашему .sln файлу
set(SOLUTION_PATH ${SRC_DIR}/"Llm.sln")

# Кастомная цель для сборки
add_custom_target(build_dot_net_solution ALL
    COMMAND ${DOTNET_EXE} build ${SOLUTION_PATH} --configuration Release
    WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
    COMMENT "Build .NET solution..."
)

add_custom_command(
    OUTPUT ${DATASETS_DIR}/.bbdone
    COMMAND ${SCRIPTS_DIR}/datasets/create_bb_dataset.sh ${DATASETS_DIR}
    WORKING_DIRECTORY ${SCRIPTS_DIR}/datasets/
    COMMAND touch ${DATASETS_DIR}/.bbdone
)

# Add a target to build text dataset from BB
add_custom_target(build_text_dataset DEPENDS ${DATASETS_DIR}/.bbdone)

add_custom_command(
    OUTPUT ${DATASETS_DIR}/.pdfsdone
    COMMAND python3 ${SCRIPTS_DIR}/datasets/download_pdfs.py --folder ${DATASETS_DIR}/pdfs
    COMMAND touch ${DATASETS_DIR}/.pdfsdone
)

add_custom_command(
    OUTPUT ${DATASETS_DIR}/.mdsdone
    COMMAND python3 ${SCRIPTS_DIR}/datasets/convert_pdf_to_md_surja.py ${DATASETS_DIR}/pdfs ${DATASETS_DIR}/mds
    COMMAND touch ${DATASETS_DIR}/.mdsdone
    DEPENDS ${DATASETS_DIR}/.pdfsdone
)
# Add a target to convert pdfs dataset to md dataset
add_custom_target(build_pdfs_dataset DEPENDS ${DATASETS_DIR}/.mdsdone)

# Add a target to run training
add_custom_target(run_pretrain
    COMMAND python3 ${SCRIPTS_DIR}/models/pretrain.py --model_name ${MODEL_NAME} --dataset_path ${DATASET_NAME}
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)

add_custom_target(download_models
    COMMAND ${SCRIPTS_DIR}/models/download_gguf_models.sh ${MODELS_DIR}
    WORKING_DIRECTORY ${MODELS_DIR}
)

add_custom_target(crawl_forum
    COMMAND dotnet run --project ${SRC_DIR}/Crawler/Crawler.csproj --output ${DATASETS_DIR}/forum.jsonl
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)


# Add a target to run Docker Compose
add_custom_target(run_notebooks
    COMMAND docker-compose -f docker-compose.yml --env-file ${CMAKE_SOURCE_DIR}/.env up -d 
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/labs
)

add_custom_target(run_local
    COMMAND docker-compose --project-directory ${CMAKE_SOURCE_DIR} -f ${CMAKE_SOURCE_DIR}/docker/docker-compose.local.yml --env-file ${CMAKE_SOURCE_DIR}/.env up -d 
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)

add_custom_target(stop_notebooks
    COMMAND docker-compose -f docker-compose.yml --env-file ${CMAKE_SOURCE_DIR}/.env down
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/labs
)

add_custom_target(build_docker
    COMMAND ${SCRIPTS_DIR}/build_docker.sh ${SRC_DIR}
)

# # Add a target to run Docker Compose
# add_custom_target(run_inference
#     COMMAND docker compose --project-directory ${CMAKE_SOURCE_DIR} -f ${CMAKE_SOURCE_DIR}/docker/docker-compose.yml up --env-file ${CMAKE_SOURCE_DIR}/.env up -d 
#     WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
# )

add_custom_target(clearml_init
    COMMAND clearml-init
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)
