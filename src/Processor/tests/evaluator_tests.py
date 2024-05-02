"""
 * @file evaluator_tests.py
 * @brief Test suite for evaluator classes.
 *
 * This file contains unit tests for the Evaluator classes in the src.evaluator module.
 * It includes tests for basic evaluation, invalid input handling, empty input handling,
 * and various edge cases.
 *
 * Main functionalities of this file include:
 * - Testing the evaluate method with different inputs and scenarios.
 * - Ensuring proper error handling for invalid inputs and edge cases.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import sys
import unittest

import pandas as pd

sys.path.append("..")

from src.evaluator.abstract_evaluator import Evaluator
from src.evaluator.lightgbm_evaluator import LightGBMEvaluator


class EvaluatorTests(unittest.TestCase):
    """
    A class containing unit tests for the Evaluator classes.
    """

    def setUp(self):
        """
        Set up the test environment by initializing the evaluator.
        """
        self.evaluator: Evaluator = LightGBMEvaluator(
            model_path="../../../training/models/lightgbm_model.joblib"
        )

    def test_evaluate(self):
        """
        Test the evaluate method of the evaluator with valid input data.
        """
        input_data = pd.DataFrame(
            {
                "domain_name": ["b1bc086df017d40a4123488866a265b2.info"],
                "lex_name_len": [37],
                "lex_has_digit": [1],
                "lex_phishing_keyword_count": [1],
                "lex_consecutive_chars": [3],
                "lex_tld_len": [4],
                "lex_tld_abuse_score": [0.0043],
                "lex_sld_len": [32],
                "lex_sld_norm_entropy": [0.113502],
                "lex_sld_digit_count": [23.0],
                "lex_sld_digit_ratio": [0.71875],
                "lex_sld_phishing_keyword_count": [0],
                "lex_sld_vowel_count": [2],
                "lex_sld_vowel_ratio": [0.0625],
                "lex_sld_consonant_count": [7],
                "lex_sld_consonant_ratio": [0.21875],
                "lex_sld_non_alphanum_count": [0],
                "lex_sld_non_alphanum_ratio": [0.0],
                "lex_sld_hex_count": [32],
                "lex_sld_hex_ratio": [1.0],
                "lex_sub_count": [0],
                "lex_stld_unique_char_count": [17],
                "lex_begins_with_digit": [0],
                "lex_sub_max_consonant_len": [2],
                "lex_sub_norm_entropy": [0.113502],
                "lex_sub_digit_count": [23.0],
                "lex_sub_digit_ratio": [0.71875],
                "lex_sub_vowel_count": [2],
                "lex_sub_vowel_ratio": [0.0625],
                "lex_sub_consonant_count": [7],
                "lex_sub_consonant_ratio": [0.21875],
                "lex_sub_non_alphanum_count": [0],
                "lex_sub_non_alphanum_ratio": [0.0],
                "lex_sub_hex_count": [32],
                "lex_sub_hex_ratio": [1.0],
                "lex_dga_bigram_matches": [29],
                "lex_dga_trigram_matches": [4],
                "lex_dga_tetragram_matches": [0],
                "lex_dga_pentagram_matches": [0],
                "mod_jaccard_bi-grams_benign": [1.0],
                "mod_jaccard_tri-grams_benign": [0.171429],
                "mod_jaccard_penta-grams_benign": [0.030303],
                "mod_jaccard_bi-grams_dga": [0.941176],
                "mod_jaccard_tri-grams_dga": [0.171429],
                "mod_jaccard_penta-grams_dga": [0.0],
                "lex_avg_part_len": [18.0],
                "lex_stdev_part_lens": [0.5],
                "lex_longest_part_len": [32],
                "lex_shortest_sub_len": [32],
            }
        )

        output = self.evaluator.evaluate(input_data)

        self.assertTrue("domain_name" in output.columns)
        self.assertTrue("predict_prob" in output.columns)
        self.assertTrue("binary_pred" in output.columns)

        self.assertEqual(len(input_data), len(output))

    def test_invalid_input(self):
        """
        Test the evaluate method of the evaluator with invalid input data.
        """
        input_data = pd.DataFrame()
        with self.assertRaises(ValueError):
            self.evaluator.evaluate(input_data)

        input_data = pd.DataFrame({"feature1": [0.1, 0.2], "feature2": [0.5, 0.8]})
        with self.assertRaises(ValueError):
            self.evaluator.evaluate(input_data)

    def test_empty_input(self):
        """
        Test the evaluate method of the evaluator with empty input data.
        """
        input_data = pd.DataFrame(columns=["domain_name", "feature1", "feature2"])
        output = self.evaluator.evaluate(input_data)

        self.assertTrue(output.empty)

        