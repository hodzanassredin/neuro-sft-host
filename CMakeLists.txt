cmake_minimum_required(VERSION 3.10)
project(NeuroSft)

set(DATASETS_DIR "${CMAKE_SOURCE_DIR}/datasets")
set(MODELS_DIR "${CMAKE_SOURCE_DIR}/models")
set(SCRIPTS_DIR "${CMAKE_SOURCE_DIR}/scripts")
set(SRC_DIR "${CMAKE_SOURCE_DIR}/src/Llm")
# Specify the MODEL_NAME variable, which can be passed when calling cmake
set(MODEL_NAME "default_model" CACHE STRING "Name of the model to train")


find_package (Python3 COMPONENTS Interpreter)
execute_process (COMMAND "${Python3_EXECUTABLE}" -m venv "/path/to/venv")

# # Here is the trick
# ## update the environment with VIRTUAL_ENV variable (mimic the activate script)
# set (ENV{VIRTUAL_ENV} ${CMAKE_SOURCE_DIR}/venv)
# ## change the context of the search
# set (Python3_FIND_VIRTUALENV FIRST)
# ## unset Python3_EXECUTABLE because it is also an input variable (see documentation, Artifacts Specification section)
# unset (Python3_EXECUTABLE)
# ## Launch a new search
# find_package (Python3 COMPONENTS Interpreter Development)

# find_package(Python REQUIRED)
find_program(DOTNET_EXE NAMES "dotnet" PATHS "$ENV{ProgramFiles}/dotnet" "$ENV{HOME}/.dotnet/tools")
if (NOT DOTNET_EXE)
    message(FATAL_ERROR "dotnet CLI not found! https://dotnet.microsoft.com/en-us/download/dotnet/9.0")
endif()

# if(EXISTS ${CMAKE_SOURCE_DIR}/venv)
#     execute_process(COMMAND source ${CMAKE_SOURCE_DIR}/venv/bin/activate)
# else()
#     execute_process(COMMAND python -m venv ${CMAKE_SOURCE_DIR}/venv)
#     execute_process(COMMAND source ${CMAKE_SOURCE_DIR}/venv/bin/activate)
# endif()

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
add_custom_target(run_training
    COMMAND python3 ${DATASETS_DIR}/train.py --model_name ${MODEL_NAME}
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)

# Add a target to run Docker Compose
add_custom_target(run_notebooks
    COMMAND docker-compose -f docker-compose.yml up -d
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/labs
)

add_custom_target(stop_notebooks
    COMMAND docker-compose -f docker-compose.yml down
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/labs
)

add_custom_target(build_docker
    COMMAND ${SCRIPTS_DIR}/build_docker.sh ${SRC_DIR}
)

# Add a target to run Docker Compose
add_custom_target(run_inference
    COMMAND docker-compose -f ${CMAKE_SOURCE_DIR}/docker/docker-compose.yml up
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)

add_custom_target(clearml_init
    COMMAND clearml-init
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)
