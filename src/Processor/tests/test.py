"""
 * @file test.py
 * @brief Test suite for unit tests of Processor.
 *
 * This file contains a test suite that aggregates unit tests from multiple modules.
 * The test suite includes tests for the Arguments class, FeatureExtraction class, and Evaluator class.
 *
 * Main functionalities of this file include:
 * - Constructing a test suite containing all test cases from various modules.
 * - Executing the test suite and printing the results.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import unittest

import arguments_tests
import evaluator_tests
import feature_extraction_tests


def suite() -> unittest.TestSuite:
    """
    Construct a test suite containing all test cases from various modules.

    Returns:
        TestSuite: A test suite containing all the test cases.
    """

    loader = unittest.TestLoader()
    suite = unittest.TestSuite()

    suite.addTests(loader.loadTestsFromModule(arguments_tests))
    suite.addTests(loader.loadTestsFromModule(feature_extraction_tests))
    suite.addTests(loader.loadTestsFromModule(evaluator_tests))
    return suite


if __name__ == "__main__":
    """
    Execute the test suite and print the results.
    """
    runner = unittest.TextTestRunner(verbosity=2)
    runner.run(suite())
