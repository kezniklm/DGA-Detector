# Enable testing
enable_testing()

# Include Google Test
find_package(GTest REQUIRED)

# Add an executable for your tests
add_executable(DetectorTests)

# Add test source files
target_sources(DetectorTests PRIVATE
    ArgumentsTests.cpp
    DomainValidatorTests.cpp
    FilterTests.cpp
    PublisherTests.cpp
)

# Link the test executable with GoogleTest and your library
target_link_libraries(DetectorTests
    GTest::gtest
    GTest::gtest_main
    GTest::gmock
    GTest::gmock_main
    DetectorLib
)

# Include directories for your DetectorLib
target_include_directories(DetectorTests PRIVATE
    ${PROJECT_SOURCE_DIR}/include
)

# Register the tests
add_test(NAME ArgumentsTests COMMAND DetectorTests --gtest_filter=ArgumentsTests*)
add_test(NAME DomainValidatorTests COMMAND DetectorTests --gtest_filter=DomainValidatorTests*)
add_test(NAME FilterTests COMMAND DetectorTests --gtest_filter=FilterTests*)
add_test(NAME PublisherTests COMMAND DetectorTests --gtest_filter=PublisherTests*)
