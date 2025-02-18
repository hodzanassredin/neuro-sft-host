cmake_minimum_required(VERSION 3.10)
project(NeuroSft)

set(DATASETS_DIR "${CMAKE_SOURCE_DIR}/datasets")
set(MODELS_DIR "${CMAKE_SOURCE_DIR}/models")
set(SCRIPTS_DIR "${CMAKE_SOURCE_DIR}/scripts")
# Specify the MODEL_NAME variable, which can be passed when calling cmake
set(MODEL_NAME "default_model" CACHE STRING "Name of the model to train")

# Add a target to build text dataset from BB
add_custom_target(build_text_dataset
    COMMAND ${SCRIPTS_DIR}/datasets/create_bb_dataset.sh ${DATASETS_DIR}
    WORKING_DIRECTORY ${SCRIPTS_DIR}/datasets/
)
# Add a target to run training
add_custom_target(run_training
    COMMAND python3 ${DATASETS_DIR}/train.py --model_name ${MODEL_NAME}
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)

# Add a target to run Docker Compose
add_custom_target(run_inference
    COMMAND docker-compose -f ${CMAKE_SOURCE_DIR}/docker-compose.yml up
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)



