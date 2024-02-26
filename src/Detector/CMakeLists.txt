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
    "${CMAKE_CURRENT_SOURCE_DIR}/lib"
    "/usr/local/include/mongocxx/v_noabi/"
    "/usr/local/include/bsoncxx/v_noabi/"
    "/usr/local/include"
)

set_target_properties(DetectorLib PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/lib"
)

# Searching for required dependencies
# Find PCAP library
find_path(PCAP_INCLUDE_DIR NAMES pcap.h)
find_library(PCAP_LIBRARY NAMES wpcap pcap)

# Find libuv library
find_library(LIBUV_LIBRARY NAMES uv)

find_package(PkgConfig REQUIRED)

pkg_check_modules(SimpleAmqpClient REQUIRED IMPORTED_TARGET libSimpleAmqpClient)

# Error message if dependencies are not found
if(NOT PCAP_INCLUDE_DIR OR NOT PCAP_LIBRARY)
  message(FATAL_ERROR "Npcap not found. Please ensure it is installed.")
endif()

if(NOT LIBUV_LIBRARY)
  message(FATAL_ERROR "libuv not found. Please ensure it is installed.")
endif()

# Include the pcap directory
target_include_directories(DetectorLib PUBLIC ${PCAP_INCLUDE_DIR})

# Manually specify include directory for PcapPlusPlus
include_directories(/usr/local/include/pcapplusplus)

# Find PcapPlusPlus libraries
find_library(PCAPPLUSPLUS_PCAP_LIB NAME Pcap++ PATHS /usr/local/lib)
find_library(PCAPPLUSPLUS_PACKET_LIB NAME Packet++ PATHS /usr/local/lib)
find_library(PCAPPLUSPLUS_COMMON_LIB NAME Common++ PATHS /usr/local/lib)

# Find libpcap library
find_library(LIBPCAP_LIBRARY NAMES pcap)

# MongoDB C++ Driver
find_package(mongocxx REQUIRED)
find_package(bsoncxx REQUIRED)

target_include_directories(DetectorLib PUBLIC ${LIBMONGOCXX_INCLUDE_DIRS} ${LIBBSONCXX_INCLUDE_DIRS})
target_link_libraries(DetectorLib PUBLIC mongo::mongocxx_shared mongo::bsoncxx_shared)

add_executable(Detector "src/Detector.cpp")

target_link_libraries(Detector PRIVATE 
    DetectorLib 
    ${PCAP_LIBRARY} 
    ${PCAPPLUSPLUS_PCAP_LIB} 
    ${PCAPPLUSPLUS_PACKET_LIB} 
    ${PCAPPLUSPLUS_COMMON_LIB} 
    ${LIBPCAP_LIBRARY} 
    pthread
    ${LIBUV_LIBRARY}
    PkgConfig::SimpleAmqpClient
)