# Collect all source files from the src directory, excluding main.cpp for the library
file(GLOB_RECURSE LIB_SOURCES "*.cpp")
list(FILTER LIB_SOURCES EXCLUDE REGEX "main\\.cpp$")

# Create a static library from the collected source files
add_library(DetectorLib STATIC ${LIB_SOURCES})

target_include_directories(DetectorLib PUBLIC
    ${PROJECT_SOURCE_DIR}/include
    "${PROJECT_SOURCE_DIR}/include/Data/Structures"
    "${PROJECT_SOURCE_DIR}/include/Data/Database"
    "${PROJECT_SOURCE_DIR}/include/Data/RabbitMQ"
    "${PROJECT_SOURCE_DIR}/include/Exceptions"
)

set_target_properties(DetectorLib PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${PROJECT_SOURCE_DIR}/lib"
)

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

target_link_libraries(Detector PRIVATE DetectorLib pthread)
