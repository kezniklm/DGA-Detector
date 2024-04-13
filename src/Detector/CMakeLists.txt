cmake_minimum_required(VERSION 3.10)
project(Detector)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_INCLUDE_CURRENT_DIR ON)

# Use vcpkg to find the necessary packages
find_package(libuv CONFIG REQUIRED)
find_package(bsoncxx CONFIG REQUIRED)
find_package(mongocxx CONFIG REQUIRED)
find_package(PcapPlusPlus CONFIG REQUIRED)
find_package(nlohmann_json CONFIG REQUIRED)
find_package(MPMCQueue CONFIG REQUIRED)
find_package(rabbitmq-c CONFIG REQUIRED)
find_package(GTest CONFIG REQUIRED)
find_package(Poco CONFIG REQUIRED COMPONENTS Foundation Net Util)

add_definitions(-DNOMINMAX)

enable_testing()

# Include the source and tests directories
add_subdirectory(src)
add_subdirectory(tests)
