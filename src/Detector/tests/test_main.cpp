#include <gtest/gtest.h>
// Include the header file where isEven is declared

// Example function in DetectorLib
int add(int a, int b)
{
    return a + b;
}

// Test case for the add function
TEST(AddFunction, HandlesPositiveInput)
{
    EXPECT_EQ(3, add(1, 2));
    EXPECT_EQ(5, add(2, 3));
}

TEST(AddFunction, HandlesNegativeInput)
{
    EXPECT_EQ(-1, add(-1, 0));
    EXPECT_EQ(-3, add(-1, -2));
}

// The main function running the tests
int main(int argc, char **argv)
{
    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}