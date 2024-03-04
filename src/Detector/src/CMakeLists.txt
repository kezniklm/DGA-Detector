# Collect all source files from the src directory, excluding main.cpp for the library
file(GLOB_RECURSE LIB_SOURCES "*.cpp")
list(FILTER LIB_SOURCES EXCLUDE REGEX "main\\.cpp$")

# Create a static library from the collected source files
add_library(DetectorLib STATIC ${LIB_SOURCES})

target_include_directories(DetectorLib PUBLIC
    "${PROJECT_SOURCE_DIR}/include"
    "${PROJECT_SOURCE_DIR}/include/Structures"
    "${PROJECT_SOURCE_DIR}/include/Database"
    "${PROJECT_SOURCE_DIR}/include/RabbitMQ"
    "${PROJECT_SOURCE_DIR}/include/Exceptions"
    "${PROJECT_SOURCE_DIR}/include/Queue"
)

set_target_properties(DetectorLib PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${PROJECT_SOURCE_DIR}/lib"
)

# Set optimization flags based on build type
set(OPTIMIZATION_FLAGS "-O2") # Default to a lower optimization level

# Try to enable -O3 flag if supported
include(CheckCXXCompilerFlag)
check_cxx_compiler_flag("-O3" COMPILER_SUPPORTS_O3)

if (COMPILER_SUPPORTS_O3)
    set(OPTIMIZATION_FLAGS "-O3") # Use -O3 if available
endif()

# Apply optimization flags to DetectorLib
target_compile_options(DetectorLib PUBLIC ${OPTIMIZATION_FLAGS})

# Link the Detector library with dependencies found via vcpkg
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
)

# Add executable for the main application
add_executable(Detector "main.cpp")

# Apply optimization flags to Detector
target_compile_options(Detector PRIVATE ${OPTIMIZATION_FLAGS})

target_link_libraries(Detector PRIVATE DetectorLib pthread)
