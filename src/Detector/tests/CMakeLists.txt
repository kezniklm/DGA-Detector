# Enable testing
enable_testing()

# Add an executable for your tests
file(GLOB TEST_SOURCES "*.cpp")

add_executable(DetectorTests ${TEST_SOURCES})

# Link the test executable with GoogleTest and your library
target_link_libraries(DetectorTests 
    GTest::gtest 
    GTest::gtest_main
    DetectorLib
)

# Include directories for your DetectorLib
target_include_directories(DetectorTests PRIVATE 
    ${PROJECT_SOURCE_DIR}/include
)

add_test(NAME DetectorTests COMMAND DetectorTests)
