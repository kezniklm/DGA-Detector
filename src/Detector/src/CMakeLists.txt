cmake_minimum_required(VERSION 3.10)
project(Detector)

# Collect all source files from the src directory, excluding main.cpp for the library
file(GLOB_RECURSE LIB_SOURCES "${CMAKE_SOURCE_DIR}/src/*.cpp")
list(FILTER LIB_SOURCES EXCLUDE REGEX "main\\.cpp$")

# Create a static library from the collected source files
add_library(DetectorLib STATIC ${LIB_SOURCES})

# Set include directories for the library
target_include_directories(DetectorLib PUBLIC
    "${CMAKE_SOURCE_DIR}/include"
    "${CMAKE_SOURCE_DIR}/include/Structures"
    "${CMAKE_SOURCE_DIR}/include/Database"
    "${CMAKE_SOURCE_DIR}/include/RabbitMQ"
    "${CMAKE_SOURCE_DIR}/include/Exceptions"
    "${CMAKE_SOURCE_DIR}/include/Queue"
)

# Set output directory for the library
set_target_properties(DetectorLib PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/lib"
)

# Set optimization flags based on build type
if(CMAKE_BUILD_TYPE STREQUAL "Release")
    if(CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang")
        target_compile_options(DetectorLib PUBLIC -O3)
    elseif(MSVC)
        target_compile_options(DetectorLib PUBLIC /O2)
    endif()
endif()

# Link the Detector library with dependencies
target_link_libraries(DetectorLib PUBLIC
    $<IF:$<TARGET_EXISTS:libuv::uv_a>,libuv::uv_a,libuv::uv>
    $<IF:$<TARGET_EXISTS:mongo::bsoncxx_static>,mongo::bsoncxx_static,mongo::bsoncxx_shared>
    $<IF:$<TARGET_EXISTS:mongo::mongocxx_static>,mongo::mongocxx_static,mongo::mongocxx_shared>
    PcapPlusPlus::Pcap++
    PcapPlusPlus::Packet++
    PcapPlusPlus::Common++
    nlohmann_json::nlohmann_json
    MPMCQueue::MPMCQueue
    $<IF:$<TARGET_EXISTS:rabbitmq::rabbitmq-static>,rabbitmq::rabbitmq-static,rabbitmq::rabbitmq>
    $<$<TARGET_EXISTS:Poco::Foundation>:Poco::Foundation>
    $<$<TARGET_EXISTS:Poco::Net>:Poco::Net>
    $<$<TARGET_EXISTS:Poco::Util>:Poco::Util>
)

# Add executable for the main application
add_executable(Detector "main.cpp")

# Set optimization flags for the executable
if(CMAKE_BUILD_TYPE STREQUAL "Release")
    if(CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang")
        target_compile_options(Detector PRIVATE -O3)
    elseif(MSVC)
        target_compile_options(Detector PRIVATE /O2)
    endif()
endif()

if(UNIX)
    find_package(Threads REQUIRED)
    target_link_libraries(Detector PRIVATE DetectorLib Threads::Threads)
elseif(WIN32)
    target_link_libraries(Detector PRIVATE DetectorLib)
endif()
