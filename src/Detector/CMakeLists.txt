cmake_minimum_required(VERSION 3.5)

project(Detector)

# Set C++ standard/version
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Add the source files
file(GLOB SOURCES "src/*.cpp")

# Create a library from the sources
add_library(DetectorLib STATIC ${SOURCES})

# Include directories for DetectorLib
target_include_directories(DetectorLib PUBLIC include)

# Since cxxopts.hpp and MPMCQueue.hpp are directly under lib/, adjust the include path
target_include_directories(DetectorLib PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/lib")

# Specify the output directory for the library
set_target_properties(DetectorLib PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/lib"
)

# Attempt to find pcap using find_package or find_path and find_library
find_path(PCAP_INCLUDE_DIR NAMES pcap.h 
         PATHS 
         "C:/Program Files/Npcap/include" # Adjusted to the base directory
)

find_library(PCAP_LIBRARY NAMES wpcap pcap 
             PATHS 
             "C:/Program Files/Npcap/lib/x64" # Adjusted for 64-bit library
             "C:/Program Files/Npcap/lib" # Fallback
             "C:/Program Files (x86)/Npcap/lib"
)

if(NOT PCAP_INCLUDE_DIR OR NOT PCAP_LIBRARY)
  message(FATAL_ERROR "Npcap not found. Please ensure it is installed.")
endif()

# Include the pcap directory correctly
target_include_directories(DetectorLib PUBLIC ${PCAP_INCLUDE_DIR})

# Add an executable
add_executable(Detector "src/Detector.cpp")

# Link the executable with the library and pcap
target_link_libraries(Detector PRIVATE DetectorLib ${PCAP_LIBRARY})
