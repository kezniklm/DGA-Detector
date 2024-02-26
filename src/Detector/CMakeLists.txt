cmake_minimum_required(VERSION 3.5)
project(Detector)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_INCLUDE_CURRENT_DIR ON)

# Collect all source files from the src directory
file(GLOB SOURCES "src/*.cpp")

# Create a static library from the collected source files
add_library(DetectorLib STATIC ${SOURCES})

target_include_directories(DetectorLib PUBLIC 
    include
    "${CMAKE_CURRENT_SOURCE_DIR}/include/Data/Structures"
    "${CMAKE_CURRENT_SOURCE_DIR}/include/Data/Database"
    "${CMAKE_CURRENT_SOURCE_DIR}/include/Data/RabbitMQ"
)

set_target_properties(DetectorLib PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/lib"
)

# Use vcpkg to find the necessary packages
find_package(libuv CONFIG REQUIRED)
find_package(bsoncxx CONFIG REQUIRED)
find_package(mongocxx CONFIG REQUIRED)
find_package(PcapPlusPlus CONFIG REQUIRED)
find_package(nlohmann_json CONFIG REQUIRED)
find_package(amqpcpp CONFIG REQUIRED)
find_package(MPMCQueue CONFIG REQUIRED)

# Link the Detector library with dependencies found via vcpkg
target_link_libraries(DetectorLib PUBLIC 
    $<IF:$<TARGET_EXISTS:libuv::uv_a>,libuv::uv_a,libuv::uv>
    $<IF:$<TARGET_EXISTS:mongo::bsoncxx_static>,mongo::bsoncxx_static,mongo::bsoncxx_shared>
    $<IF:$<TARGET_EXISTS:mongo::mongocxx_static>,mongo::mongocxx_static,mongo::mongocxx_shared>
    PcapPlusPlus::Pcap++
    PcapPlusPlus::Packet++
    PcapPlusPlus::Common++
    nlohmann_json::nlohmann_json
    amqpcpp
    MPMCQueue::MPMCQueue
)

add_executable(Detector "src/Detector.cpp")

target_link_libraries(Detector PRIVATE 
    DetectorLib
    pthread
)
