"""
 * @file feature_extraction_tests.py
 * @brief Test suite for feature extraction module.
 *
 * This file contains unit tests for the FeatureExtractor class in the feature extraction module.
 * It includes tests for basic feature extraction, TLD feature extraction, n-gram feature extraction,
 * second-level domain feature extraction, subdomain feature extraction, error handling, performance,
 * and various edge cases.
 *
 * Main functionalities of this file include:
 * - Testing feature extraction methods with different inputs and scenarios.
 * - Ensuring proper error handling for invalid inputs and edge cases.
 * - Assessing the performance of feature extraction methods, especially with large datasets.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import sys
import unittest
from unittest.mock import patch

from pandas import DataFrame

sys.path.append("..")

from src.features.feature_extractor import FeatureExtractor


class FeatureExtractionTests(unittest.TestCase):
    def setUp(self):
        """
        Set up test environment.
        """
        self.extractor = FeatureExtractor(num_processes=1)

    def test_extract_features(self):
        """
        Test feature extraction method.
        """
        df = DataFrame({"domain_name": ["example.com"]})
        with patch.object(
            self.extractor, "_apply_features", return_value=df
        ) as mock_apply_features:
            result_df = self.extractor.extract_features(df)
        mock_apply_features.assert_called_once()
        self.assertEqual(result_df.iloc[0]["domain_name"], "example.com")

    def test_extract_basic_features(self):
        """
        Test basic feature extraction method.
        """
        df = DataFrame(
            {
                "domain_name": ["example.com"],
                "tmp_concat_subdomains": ["example"],
                "tmp_part_lengths": [[7]],
            }
        )
        result_df = self.extractor.extract_basic_features(df)
        self.assertEqual(result_df.iloc[0]["lex_name_len"], 11)
        self.assertEqual(result_df.iloc[0]["lex_has_digit"], 0)
        self.assertEqual(result_df.iloc[0]["lex_phishing_keyword_count"], 0)

    def test_extract_features_with_digits(self):
        """
        Test feature extraction with digits in domain.
        """
        df = DataFrame(
            {
                "domain_name": ["example123.com"],
                "tmp_concat_subdomains": ["example123"],
                "tmp_part_lengths": [[10, 3]],
            }
        )
        result_df = self.extractor.extract_basic_features(df)
        self.assertEqual(result_df.iloc[0]["lex_has_digit"], 1)

    def test_extract_features_with_missing_domain(self):
        """
        Test feature extraction with missing domain.
        """
        df = DataFrame({"domain_name": [None]})
        with self.assertRaises(TypeError):
            self.extractor.extract_basic_features(df)

    def test_extract_tld_features(self):
        """
        Test TLD feature extraction.
        """
        df = DataFrame({"domain_name": ["example.weird"]})
        df["tmp_tld"] = df["domain_name"].apply(lambda x: x.split(".")[-1])
        result_df = self.extractor.extract_tld_features(df)
        self.assertEqual(result_df.iloc[0]["lex_tld_len"], 5)
        self.assertEqual(result_df.iloc[0]["lex_tld_abuse_score"], 0)

    def test_extract_ngram_features(self):
        """
        Test n-gram feature extraction.
        """
        df = DataFrame({"domain_name": ["abcd"]})
        df["tmp_concat_subdomains"] = ["abcd"]
        self.extractor.ngram_set_benign = {"bi": {"ab", "bc"}, "tri": {"abc"}}
        self.extractor.ngram_set_dga = {"bi": {"bc", "cd"}, "tri": {"bcd"}}
        result_df = self.extractor.extract_ngram_features(df)
        self.assertEqual(
            result_df.iloc[0]["mod_jaccard_bi-grams_benign"], 0.6666666666666666
        )
        self.assertEqual(result_df.iloc[0]["mod_jaccard_tri-grams_benign"], 0.5)
        self.assertGreater(result_df.iloc[0]["lex_dga_bigram_matches"], 0)

    def test_full_data_processing(self):
        """
        Test full data processing.
        """
        df = DataFrame({"domain_name": ["example.com", "test123.net", "weird-tld.xyz"]})
        result_df = self.extractor.extract_features(df)
        self.assertEqual(len(result_df), 3)
        self.assertIn("lex_name_len", result_df.columns)
        self.assertIn("lex_has_digit", result_df.columns)
        self.assertIn("lex_tld_len", result_df.columns)

    def test_complete_feature_extraction(self):
        """
        Test complete feature extraction.
        """
        df = DataFrame({"domain_name": ["test.example.com"]})
        result_df = self.extractor.extract_features(df)
        expected_features = [
            "lex_name_len",
            "lex_has_digit",
            "lex_tld_len",
            "mod_jaccard_tri-grams_benign",
        ]
        for feature in expected_features:
            self.assertIn(feature, result_df.columns)

    def test_extract_sld_features(self):
        """
        Test second-level domain feature extraction.
        """
        df = DataFrame({"tmp_sld": ["secure-login"]})
        result_df = self.extractor.extract_sld_features(df)
        self.assertEqual(result_df.iloc[0]["lex_sld_len"], 12)
        self.assertGreater(result_df.iloc[0]["lex_sld_norm_entropy"], 0)

    def test_extract_subdomain_features(self):
        """
        Test subdomain feature extraction.
        """
        df = DataFrame(
            {
                "domain_name": ["info.secure.example.com"],
                "tmp_concat_subdomains": ["infosecure"],
                "tmp_stld": ["examplecom"],
            }
        )
        result_df = self.extractor.extract_subdomain_features(df)
        self.assertEqual(result_df.iloc[0]["lex_sub_count"], 2)
        self.assertGreater(result_df.iloc[0]["lex_sub_max_consonant_len"], 0)

    def test_error_handling_with_missing_data(self):
        """
        Test error handling with missing data.
        """
        df = DataFrame({"domain_name": [None]})
        with self.assertRaises(TypeError):
            self.extractor.extract_basic_features(df)

    def test_domain_with_special_characters(self):
        """
        Test domain with special characters.
        """
        df = DataFrame(
            {
                "domain_name": ["example-domain.com"],
                "tmp_concat_subdomains": ["example-domain"],
                "tmp_part_lengths": [[14, 3]],
            }
        )
        result_df = self.extractor.extract_basic_features(df)
        self.assertEqual(result_df.iloc[0]["lex_has_digit"], 0)
        self.assertEqual(result_df.iloc[0]["lex_name_len"], len("example-domain.com"))

    def test_domain_with_all_digits(self):
        """
        Test domain with all digits.
        """
        df = DataFrame(
            {
                "domain_name": ["1234567890.com"],
                "tmp_concat_subdomains": ["1234567890"],
                "tmp_part_lengths": [[10, 3]],
            }
        )
        result_df = self.extractor.extract_basic_features(df)
        self.assertTrue(result_df.iloc[0]["lex_has_digit"])
        self.assertEqual(result_df.iloc[0]["lex_name_len"], len("1234567890.com"))

    def test_empty_domain(self):
        """
        Test empty domain.
        """
        df = DataFrame(
            {
                "domain_name": [""],
                "tmp_concat_subdomains": [""],
                "tmp_part_lengths": [[0]],
            }
        )
        result_df = self.extractor.extract_basic_features(df)
        self.assertEqual(result_df.iloc[0]["lex_name_len"], 0)
        self.assertEqual(result_df.iloc[0]["lex_has_digit"], 0)

    def test_invalid_characters_in_domain(self):
        """
        Test invalid characters in domain.
        """
        df = DataFrame({"domain_name": ["example\x00.com"]})
        try:
            self.extractor.extract_features(df)
        except ValueError:
            assert False, "ValueError was raised unexpectedly"

    def test_performance_large_dataset(self):
        """
        Test performance with a large dataset.
        """
        df = DataFrame({"domain_name": ["test" + str(i) + ".com" for i in range(1000)]})
        import time

        start_time = time.time()
        result_df = self.extractor.extract_features(df)
        end_time = time.time()
        self.assertTrue((end_time - start_time) < 5)
        self.assertEqual(len(result_df), 1000)

    def test_exact_feature_values(self):
        """
        Test exact feature values.
        """
        df = DataFrame({"domain_name": ["test.example.com"]})
        result_df = self.extractor.extract_features(df)
        self.assertEqual(result_df.iloc[0]["lex_sld_norm_entropy"], 0.3602343766204741)
        self.assertEqual(result_df.iloc[0]["lex_tld_len"], 3)

    def test_internationalized_domain_names(self):
        """
        Test internationalized domain names.
        """
        df = DataFrame({"domain_name": ["münchen.com"]})
        result_df = self.extractor.extract_features(df)
        self.assertIn("lex_name_len", result_df.columns)
        self.assertEqual(result_df.iloc[0]["lex_name_len"], len("münchen.com"))

    def test_maximum_length_domain_name(self):
        """
        Test maximum length domain name.
        """
        long_domain = "a" * 63 + ".com"
        df = DataFrame(
            {
                "domain_name": [long_domain],
                "tmp_concat_subdomains": ["a" * 63],
                "tmp_part_lengths": [[63, 3]],
            }
        )
        result_df = self.extractor.extract_basic_features(df)
        self.assertEqual(result_df.iloc[0]["lex_name_len"], len(long_domain))

    def test_deeply_nested_subdomains(self):
        """
        Test deeply nested subdomains.
        """
        domain = "a.b.c.d.e.f.g.h.example.com"
        df = DataFrame(
            {
                "domain_name": [domain],
                "tmp_stld": ["examplecom"],
                "tmp_concat_subdomains": ["abcdefgh"],
            }
        )
        result_df = self.extractor.extract_subdomain_features(df)
        self.assertEqual(result_df.iloc[0]["lex_sub_count"], 8)

    def test_domain_without_tld(self):
        """
        Test domain without TLD.
        """
        df = DataFrame({"domain_name": ["localhost"]})
        result_df = self.extractor.extract_features(df)
        self.assertEqual(result_df.iloc[0]["lex_tld_len"], 0)

    def test_domains_with_unusual_characters(self):
        """
        Test domains with unusual characters.
        """
        df = DataFrame({"domain_name": ["exam!ple.com"]})
        result_df = self.extractor.extract_features(df)
        self.assertEqual(result_df.iloc[0]["lex_has_digit"], 0)
        self.assertTrue(result_df.iloc[0]["lex_sub_non_alphanum_count"] > 0)

