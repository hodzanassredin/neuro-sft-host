cmake_minimum_required(VERSION 3.10)
project(NeuroSft)

set(DATASETS_DIR "${CMAKE_SOURCE_DIR}/datasets")
set(MODELS_DIR "${CMAKE_SOURCE_DIR}/models")
# Указываем переменную MODEL_NAME, которую можно передать при вызове cmake
set(MODEL_NAME "default_model" CACHE STRING "Name of the model to train")
# Добавляем цель для запуска обучения
add_custom_target(run_training
    COMMAND python3 ${DATASETS_DIR}/train.py --model_name ${MODEL_NAME}
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)

# Добавляем цель для запуска Docker Compose
add_custom_target(run_inference
    COMMAND docker-compose -f ${CMAKE_SOURCE_DIR}/docker-compose.yml up
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)



