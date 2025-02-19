cmake_minimum_required(VERSION 3.10)
project(NeuroSft)

set(DATASETS_DIR "${CMAKE_SOURCE_DIR}/datasets")
set(MODELS_DIR "${CMAKE_SOURCE_DIR}/models")
set(SCRIPTS_DIR "${CMAKE_SOURCE_DIR}/scripts")
# Specify the MODEL_NAME variable, which can be passed when calling cmake
set(MODEL_NAME "default_model" CACHE STRING "Name of the model to train")

add_custom_command(
    OUTPUT ${DATASETS_DIR}/.bbdone
    COMMAND ${SCRIPTS_DIR}/datasets/create_bb_dataset.sh ${DATASETS_DIR}
    WORKING_DIRECTORY ${SCRIPTS_DIR}/datasets/
    COMMAND touch ${DATASETS_DIR}/.bbdone
)

# Add a target to build text dataset from BB
add_custom_target(build_text_dataset ALL DEPENDS ${DATASETS_DIR}/.bbdone)

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
add_custom_target(build_pdfs_dataset ALL DEPENDS ${DATASETS_DIR}/.mdsdone)


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



