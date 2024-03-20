/**
 * @file ArgumentsTests.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Implements unit tests for the Arguments class.
 *
 * This file contains the implementation of unit tests for the Arguments class. The Arguments class is responsible for parsing command line arguments and reading settings from a JSON file. These unit tests cover various scenarios to ensure the correct behavior of the Arguments class under different conditions.
 *
 * The main functionalities of this file include:
 * - Testing valid argument parsing from JSON settings and command line arguments.
 * - Testing argument parsing with missing, invalid, or conflicting inputs.
 * - Verifying proper handling of special cases, such as quoted strings and large numeric values.
 * - Ensuring correct behavior when appsettings.json is missing or contains invalid JSON.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#include <gtest/gtest.h>
#include <fstream>

#include "nlohmann/json.hpp"

#include "Arguments.hpp"

using json = nlohmann::json;

class ArgumentsTest : public ::testing::Test
{
protected:
    void SetUp() override
    {
        std::remove("appsettings.json");
    }

    void TearDown() override
    {
        std::remove("appsettings.json");
    }

    void CreateTestAppSettings(const json &settings)
    {
        std::ofstream ofs("appsettings.json");
        if (ofs.is_open())
        {
            ofs << settings.dump(4);
            ofs.close();
        }
    }

    std::vector<char *> PrepareArguments(const std::vector<std::string> &args_vec)
    {
        std::vector<char *> argv;
        for (auto &arg : args_vec)
        {
            argv.push_back(const_cast<char *>(arg.data()));
        }
        argv.push_back(nullptr);
        return argv;
    }
};

/**
 * @brief Tests that valid arguments from a JSON file do not cause exceptions.
 */
TEST_F(ArgumentsTest, ValidArgumentsFromJsonShouldNotThrow)
{
	const json settings = {
        {"Interface", "test-interface"},
        {"Size", 1024},
        {"Database", "database-string"},
        {"RabbitMQ", "rabbitmq-string"},
        {"Queue", "queue"}};
    CreateTestAppSettings(settings);

	const std::vector<std::string> args_vec = {"./Detector", "--interface", "test-interface", "--size", "1024"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_NO_THROW(args.Parse(argc, const_cast<const char **>(argv.data())));
}

/**
 * @brief Tests that valid arguments provided via the command line do not cause exceptions.
 */
TEST_F(ArgumentsTest, ValidArgumentsFromCommandLineShouldNotThrow)
{
	const json settings = {};
    CreateTestAppSettings(settings);

	const std::vector<std::string> args_vec = {
        "./Detector",
        "--interface", "test-interface",
        "--size", "1024",
        "--database", "test-database",
        "--rabbitmq", "test-rabbitmq",
        "--queue", "test-queue"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_NO_THROW(args.Parse(argc, const_cast<const char **>(argv.data())));
}

/**
 * @brief Tests that command line arguments override those from appsettings.json without causing exceptions.
 */
TEST_F(ArgumentsTest, CommandLineOverridesAppsettingsShouldNotThrow)
{
	const json settings = {
        {"interface", "default-interface"},
        {"size", 2048},
        {"database", "default-database"},
        {"rabbitmq", "default-rabbitmq"},
        {"queue", "default-queue"}};
    CreateTestAppSettings(settings);

	const std::vector<std::string> args_vec = {"./Detector", "--interface", "override-interface", "--size", "4096"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_NO_THROW(args.Parse(argc, const_cast<const char **>(argv.data())));
}

/**
 * @brief Tests that a combination of command line and appsettings.json arguments do not cause exceptions.
 */
TEST_F(ArgumentsTest, PartialCommandLineAndAppsettingsShouldNotThrow)
{
	const json settings = {
        {"interface", "partial-interface"},
        {"size", 2048}};
    CreateTestAppSettings(settings);

	const std::vector<std::string> args_vec = {"./Detector", "--database", "cmd-database", "--rabbitmq", "cmd-rabbitmq", "--queue", "cmd-queue"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_NO_THROW(args.Parse(argc, const_cast<const char **>(argv.data())));
}

/**
 * @brief Tests that missing arguments result in the expected exceptions.
 */
TEST_F(ArgumentsTest, MissingArgumentsShouldThrow)
{
	const std::vector<std::string> args_vec = {"./Detector"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_THROW(args.Parse(argc, const_cast<const char **>(argv.data())), ArgumentException);
}

/**
 * @brief Tests that duplicate command line arguments do not cause exceptions and the last one is used.
 */
TEST_F(ArgumentsTest, DuplicateCommandLineArgumentsShouldNotThrow)
{
	const std::vector<std::string> args_vec = {"./Detector", "--interface", "first-interface", "--interface", "second-interface", "--database", "cmd-database", "--rabbitmq", "cmd-rabbitmq", "--queue", "cmd-queue", "-s", "2000"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_NO_THROW(args.Parse(argc, const_cast<const char **>(argv.data())));
}

/**
 * @brief Tests that quoted string arguments are parsed correctly without causing exceptions.
 */
TEST_F(ArgumentsTest, QuotedStringArgumentsShouldNotThrow)
{
	const std::vector<std::string> args_vec = {"./Detector", "--interface", "interface", "--database", "\"Database=Test DB; Server=localhost;\"", "--rabbitmq", "cmd-rabbitmq", "--queue", "cmd-queue", "-s", "2000"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_NO_THROW(args.Parse(argc, const_cast<const char **>(argv.data())));
}

/**
 * @brief Tests that large numeric values for arguments are handled correctly without causing exceptions.
 */
TEST_F(ArgumentsTest, LargeSizeValueInsertedShouldNotThrow)
{
	const json settings = {
        {"interface", "large-value-interface"},
        {"size", std::numeric_limits<unsigned long long>::max()},
        {"database", "large-value-database"},
        {"rabbitmq", "large-value-rabbitmq"},
        {"queue", "large-value-queue"}};
    CreateTestAppSettings(settings);

	const std::vector<std::string> args_vec = {"./Detector"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_NO_THROW(args.Parse(argc, const_cast<const char **>(argv.data())));
}

/**
 * @brief Tests that invalid argument types result in the expected exceptions.
 */
TEST_F(ArgumentsTest, InvalidArgumentValueTypeShouldThrow)
{
	const json settings = {
        {"interface", "invalid-type-interface"},
        {"size", "should-be-a-number"},
        {"database", "invalid-type-database"}};
    CreateTestAppSettings(settings);

    std::vector<std::string> args_vec = {"./Detector"};
    auto argv = PrepareArguments(args_vec);
    int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_THROW(args.Parse(argc, const_cast<const char **>(argv.data())), ArgumentException);
}

/**
 * @brief Tests that special characters in arguments are handled correctly without causing exceptions.
 */
TEST_F(ArgumentsTest, SpecialCharactersInArgumentsShouldNotThrow)
{
	const std::vector<std::string> args_vec = {"./Detector", "--interface", "interface", "--database", "\"Database=Test;Password=p@$$w0rd;\"", "--rabbitmq", "cmd-rabbitmq", "--queue", "cmd-queue", "-s", "2000"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_NO_THROW(args.Parse(argc, const_cast<const char **>(argv.data())));
}

/**
 * @brief Tests that requesting help through the command line argument results in the expected exception.
 */
TEST_F(ArgumentsTest, HelpOptionHandlingShouldThrow)
{
	const std::vector<std::string> args_vec = {"./Detector", "--help"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_THROW(args.Parse(argc, const_cast<const char **>(argv.data())), ArgumentException);
}

/**
 * @brief Tests that errors in JSON parsing result in the expected exceptions.
 */
TEST_F(ArgumentsTest, JsonParsingErrorShouldThrow)
{
    std::ofstream ofs("appsettings.json");
    if (ofs.is_open())
    {
        ofs << "{invalid JSON}";
        ofs.close();
    }

    std::vector<std::string> args_vec = {"./Detector"};
    auto argv = PrepareArguments(args_vec);
    int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_THROW(args.Parse(argc, const_cast<const char **>(argv.data())), ArgumentException);
}

/**
 * @brief Tests that the absence of the appsettings.json file does not cause exceptions when all arguments are provided via command line.
 */
TEST_F(ArgumentsTest, NonexistentAppsettingsFileShouldNotThrow)
{
    std::remove("appsettings.json");

    const std::vector<std::string> args_vec = {
        "./Detector",
        "--interface", "no-settings-interface",
        "--size", "2048",
        "--database", "no-settings-database",
        "--rabbitmq", "no-settings-rabbitmq",
        "--queue", "no-settings-queue"};
    auto argv = PrepareArguments(args_vec);
    const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_NO_THROW(args.Parse(argc, const_cast<const char **>(argv.data())));
}

/**
 * @brief Tests that appsettings.json keys are case-insensitive.
 */
TEST_F(ArgumentsTest, AppSettingsKeyCaseSensitivityShouldNotThrow)
{
	const json settings = {
        {"Interface", "case-interface"},
        {"Size", 2048},
        {"DataBase", "case-database"},
        {"RabbitMQ", "case-rabbitmq"},
        {"Queue", "case-queue"}};
    CreateTestAppSettings(settings);

	const std::vector<std::string> args_vec = {"./Detector"};
    auto argv = PrepareArguments(args_vec);
	const int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_NO_THROW(args.Parse(argc, const_cast<const char **>(argv.data())));
}

/**
 * @brief Tests that missing required arguments result in the expected exceptions.
 */
TEST_F(ArgumentsTest, MissingRequiredArgumentsShouldThrow)
{
    // Incomplete appsettings.json without required "interface" and "size" settings
    json settings = {
        {"database", "some-database"},
        {"rabbitmq", "some-rabbitmq"},
        {"queue", "some-queue"}};

    CreateTestAppSettings(settings);

    // No command line arguments for "interface" and "size" which are required
    std::vector<std::string> args_vec = {"./Detector"};
    auto argv = PrepareArguments(args_vec);
    int argc = static_cast<int>(argv.size()) - 1;

    Arguments args;
    EXPECT_THROW(args.Parse(argc, const_cast<const char **>(argv.data())), ArgumentException);
}
