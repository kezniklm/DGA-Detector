cmake_minimum_required(VERSION 3.5)

project(Detector)

# Set C++ standard/version
set(CMAKE_CXX_STANDARD 20) 
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

add_compile_options(-pedantic -Wextra -Werror -g -fcommon)


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

# Add an executable
add_executable(Detector "src/Detector.cpp")

# Link the executable with the library
target_link_libraries(Detector PRIVATE DetectorLib pcap)
