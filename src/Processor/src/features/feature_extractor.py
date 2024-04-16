"""
 * @file feature_extractor.py
 * @brief Feature extraction module for domain analysis.
 *
 * This module is designed to extract and compute a variety of lexical, structural, and statistical features from domain names. These features are critical in the identification and analysis of potential DGA domain names. The extraction process leverages multiprocessing for efficient handling of large datasets, ensuring scalability and performance.
 *
 * The main functionalities of this module include:
 * - Lexical feature extraction such as domain length, digit presence, and specific keyword counting.
 * - Analysis of domain name components including subdomains, second-level domains (SLD), and top-level domains (TLD).
 * - Statistical analysis using n-grams for pattern detection in domain names, comparing against known benign and malicious profiles.
 * - Computation of various metrics like entropy, Jaccard index, and KL-divergence to quantify the similarity and randomness in domain names.
 *
 * This tool is essential for organizations aiming to enhance their cybersecurity posture through proactive domain screening and anomaly detection.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import os
import sys
import warnings
from multiprocessing import Pool

import numpy as np
import pandas as pd
import tldextract

from .constants import CONSONANTS, HEX_CHARACTERS, NGRAM_MAPPING, PHISHING_KEYWORDS
from .utils import (
    compute_kl_for_domain,
    consecutive_chars,
    count_subdomains,
    find_ngram_matches,
    generate_ngrams,
    get_lengths_of_parts,
    get_normalized_entropy,
    get_tld_abuse_score,
    load_ngram_data,
    longest_consonant_seq,
    modified_jaccard_index,
    ngram_frequency_to_probability,
    remove_tld,
    vowel_count,
)

warnings.filterwarnings(
    "ignore", category=FutureWarning, module="numpy.core.fromnumeric"
)


class FeatureExtractor:
    """Class responsible for extracting various lexical features from domain names for the purpose of domain analysis.

    The class leverages multiprocessing for efficient processing of large datasets.
    """

    def __init__(self, num_processes: int) -> None:
        """Initialize the FeatureExtractor with a specified number of processes.

        Args:
            num_processes (int): The number of processes to use for parallel computation.
        """
        self.num_processes: int = num_processes
        self.initialise_feature_extractor()

    def initialise_feature_extractor(self) -> None:
        """Load n-gram data from files and prepare frequency and probability data structures for feature extraction."""
        self.ngram_freq_dga = load_ngram_data("data/ngram_freq_dga.json")
        self.ngram_freq_benign = load_ngram_data("data/ngram_freq.json")
        self.ngram_prob_dga = ngram_frequency_to_probability(self.ngram_freq_dga)
        self.ngram_prob_benign = ngram_frequency_to_probability(self.ngram_freq_benign)
        self.ngram_set_dga = {
            n: set(self.ngram_freq_dga[f"{n}gram_freq"].keys())
            for n in ["bi", "tri", "penta"]
        }
        self.ngram_set_benign = {
            n: set(self.ngram_freq_benign[f"{n}gram_freq"].keys())
            for n in ["bi", "tri", "penta"]
        }

    def extract_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """Extract features in parallel from the input DataFrame.

        Args:
            df (pd.DataFrame): DataFrame containing domain names.

        Returns:
            pd.DataFrame: DataFrame containing extracted features.
        """
        df = df.copy(True)
        df_split = np.array_split(df, self.num_processes)
        df_features = pd.DataFrame()

        # Redirect stdout and stderr
        original_stdout = sys.stdout
        original_stderr = sys.stderr
        sys.stdout = open(os.devnull, "w")
        sys.stderr = open(os.devnull, "w")

        try:
            with Pool(processes=self.num_processes) as pool:
                results = pool.map(self._apply_features, df_split)
                df_features = pd.concat(results)
        finally:
            # Restore stdout and stderr
            sys.stdout.close()
            sys.stderr.close()
            sys.stdout = original_stdout
            sys.stderr = original_stderr
        return df_features

    def _apply_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """Apply the feature extraction methods to a split of the main DataFrame.

        Args:
            df (pd.DataFrame): A split DataFrame for which features will be extracted.

        Returns:
            pd.DataFrame: DataFrame with extracted features.
        """
        df = df.copy(True)
        df = self.extract_temp_collumns(df)
        df = self.extract_basic_features(df)
        df = self.extract_tld_features(df)
        df = self.extract_sld_features(df)
        df = self.extract_subdomain_features(df)
        df = self.extract_ngram_features(df)
        df = self.drop_temp_collumns(df)
        return df

    def extract_basic_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """Extract basic lexical features from domain names within the DataFrame.

        Args:
            df (pd.DataFrame): DataFrame with domain names to process.

        Returns:
            pd.DataFrame: DataFrame updated with basic lexical features.
        """
        df = df.copy(True)
        df["lex_name_len"] = df["domain_name"].apply(
            len
        )  # Total length of the domain name
        df["lex_has_digit"] = df["domain_name"].apply(
            lambda x: 1 if sum([1 for y in x if y.isdigit()]) > 0 else 0
        )  # Binary indicator of numeric presence in the domain
        df["lex_phishing_keyword_count"] = df["domain_name"].apply(
            lambda x: sum(1 for w in PHISHING_KEYWORDS if w in x)
        )  # Count of phishing related keywords in the domain
        df["lex_consecutive_chars"] = df["domain_name"].apply(
            lambda x: consecutive_chars(x)
        )  # Count of maximum consecutive characters
        df["lex_shortest_sub_len"] = df["tmp_concat_subdomains"].apply(
            lambda x: (
                min(get_lengths_of_parts(x)) if len(get_lengths_of_parts(x)) > 0 else 0
            )
        )  # Length of the shortest part of the domain (subdomain or second-level domain)
        return df

    def extract_tld_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """Extract features related to the top-level domain (TLD) from the domain names.

        Args:
            df (pd.DataFrame): DataFrame with domain names to process.

        Returns:
            pd.DataFrame: DataFrame updated with TLD-related features.
        """
        df = df.copy(True)
        df["lex_tld_len"] = df["tmp_tld"].apply(len)  # Length of the TLD
        df["lex_tld_abuse_score"] = df["tmp_tld"].apply(
            get_tld_abuse_score
        )  # Abuse score based on the TLD
        return df

    def extract_sld_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """Extract features from the second-level domain (SLD) part of the domain names.

        Args:
            df (pd.DataFrame): DataFrame with domain names to process.

        Returns:
            pd.DataFrame: DataFrame updated with SLD-related features.
        """

        df["lex_sld_len"] = df["tmp_sld"].apply(len)  # Length of SLD
        df["lex_sld_norm_entropy"] = df["tmp_sld"].apply(
            get_normalized_entropy
        )  # Normalized entropy of the SLD
        df["lex_sld_digit_count"] = (
            df["tmp_sld"]
            .apply(lambda x: (sum([1 for y in x if y.isdigit()])) if len(x) > 0 else 0)
            .astype("float")
        )  # Count of digits in the SLD
        df["lex_sld_digit_ratio"] = df["tmp_sld"].apply(
            # Digit ratio in the SLD
            lambda x: (sum([1 for y in x if y.isdigit()]) / len(x)) if len(x) > 0 else 0
        )
        df["lex_sld_phishing_keyword_count"] = df["tmp_sld"].apply(
            lambda x: sum(1 for w in PHISHING_KEYWORDS if w in x)
        )  # Count of phishing related keywords in the SLD
        df["lex_sld_vowel_count"] = df["tmp_sld"].apply(
            lambda x: vowel_count(x)
        )  # Count of vowels in the SLD
        df["lex_sld_vowel_ratio"] = df["tmp_sld"].apply(
            lambda x: (vowel_count(x) / len(x)) if len(x) > 0 else 0
        )  # Ratio of vowels in the SLD
        df["lex_sld_consonant_count"] = df["tmp_sld"].apply(
            lambda x: (sum(1 for c in x if c in CONSONANTS)) if len(x) > 0 else 0
        )  # Count of consonants in the SLD
        df["lex_sld_consonant_ratio"] = df["tmp_sld"].apply(
            lambda x: (
                (sum(1 for c in x if c in CONSONANTS) / len(x)) if len(x) > 0 else 0
            )
        )  # Ratio of consonants in the SLD
        df["lex_sld_non_alphanum_count"] = df["tmp_sld"].apply(
            lambda x: (sum(1 for c in x if not c.isalnum())) if len(x) > 0 else 0
        )  # Count of non-alphanumeric characters in the SLD
        df["lex_sld_non_alphanum_ratio"] = df["tmp_sld"].apply(
            lambda x: (
                (sum(1 for c in x if not c.isalnum()) / len(x)) if len(x) > 0 else 0
            )
        )  # Ratio of non-alphanumeric characters in the SLD
        df["lex_sld_hex_count"] = df["tmp_sld"].apply(
            lambda x: (sum(1 for c in x if c in HEX_CHARACTERS)) if len(x) > 0 else 0
        )  # Count of hexadecimal characters in the SLD
        df["lex_sld_hex_ratio"] = df["tmp_sld"].apply(
            lambda x: (
                (sum(1 for c in x if c in HEX_CHARACTERS) / len(x)) if len(x) > 0 else 0
            )
        )  # Ratio of hexadecimal characters in the SLD
        return df

    def extract_temp_collumns(self, df: pd.DataFrame) -> pd.DataFrame:
        """Temporarily add necessary columns for feature extraction, such as separating TLD, SLD, etc.

        Args:
            df (pd.DataFrame): DataFrame with domain names to process.

        Returns:
            pd.DataFrame: DataFrame updated with temporary columns for further processing.
        """
        df = df.copy(True)
        df["tmp_tld"] = df["domain_name"].apply(
            lambda x: tldextract.extract(x).suffix
        )  # Extract TLD
        df["tmp_sld"] = df["domain_name"].apply(
            lambda x: tldextract.extract(x).domain
        )  # Extract SLD
        df["tmp_stld"] = df["tmp_sld"] + "." + df["tmp_tld"]  # Combine SLD and TLD
        df["tmp_concat_subdomains"] = df["domain_name"].apply(
            lambda x: remove_tld(x).replace(".", "")
        )  # Concatenate subdomains without dots
        df["tmp_part_lengths"] = df["domain_name"].apply(
            lambda x: get_lengths_of_parts(x)
        )  # Length of each part of the domain
        return df

    def drop_temp_collumns(self, df: pd.DataFrame) -> pd.DataFrame:
        """Remove temporary columns from the DataFrame after feature extraction.

        Args:
            df (pd.DataFrame): DataFrame from which to remove temporary columns.

        Returns:
            pd.DataFrame: Cleaned DataFrame without temporary columns.
        """
        df = df.copy(True)
        df.drop(
            columns=[
                "tmp_tld",
                "tmp_sld",
                "tmp_stld",
                "tmp_concat_subdomains",
                "tmp_part_lengths",
            ],
            inplace=True,
        )
        return df

    def extract_subdomain_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """Extract features related to subdomains in the domain names.

        Args:
            df (pd.DataFrame): DataFrame with domain names to process.

        Returns:
            pd.DataFrame: DataFrame updated with subdomain-related features.
        """
        df = df.copy(True)
        df["lex_sub_count"] = df["domain_name"].apply(
            lambda x: count_subdomains(x)
        )  # Count of subdomains
        df["lex_stld_unique_char_count"] = df["tmp_stld"].apply(
            # Count of unique characters in combined SLD and TLD
            lambda x: len(set(x.replace(".", "")))
        )
        df["lex_begins_with_digit"] = df["domain_name"].apply(
            lambda x: 1 if x[0].isdigit() else 0
        )  # Check if the domain starts with a digit
        df["lex_www_flag"] = df["domain_name"].apply(
            lambda x: 1 if (x.split("."))[0] == "www" else 0
        )  # Check if the domain starts with www
        df["lex_sub_max_consonant_len"] = df["tmp_concat_subdomains"].apply(
            longest_consonant_seq
        )  # Maximum length of consecutive consonants in subdomains
        df["lex_sub_norm_entropy"] = df["tmp_concat_subdomains"].apply(
            # Normalized entropy of concatenated subdomains
            get_normalized_entropy
        )
        df["lex_sub_digit_count"] = (
            df["tmp_concat_subdomains"]
            .apply(lambda x: (sum([1 for y in x if y.isdigit()])) if len(x) > 0 else 0)
            .astype("float")
        )  # Count of digits in concatenated subdomains
        df["lex_sub_digit_ratio"] = df["tmp_concat_subdomains"].apply(
            # Digit ratio in concatenated subdomains
            lambda x: (sum([1 for y in x if y.isdigit()]) / len(x)) if len(x) > 0 else 0
        )
        df["lex_sub_vowel_count"] = df["tmp_concat_subdomains"].apply(
            lambda x: vowel_count(x)
        )  # Count of vowels in concatenated subdomains
        df["lex_sub_vowel_ratio"] = df["tmp_concat_subdomains"].apply(
            lambda x: (vowel_count(x) / len(x)) if len(x) > 0 else 0
        )  # Ratio of vowels in concatenated subdomains
        df["lex_sub_consonant_count"] = df["tmp_concat_subdomains"].apply(
            lambda x: (sum(1 for c in x if c in CONSONANTS)) if len(x) > 0 else 0
        )  # Count of consonants in concatenated subdomains
        df["lex_sub_consonant_ratio"] = df["tmp_concat_subdomains"].apply(
            lambda x: (
                (sum(1 for c in x if c in CONSONANTS) / len(x)) if len(x) > 0 else 0
            )
        )  # Ratio of consonants in concatenated subdomains
        df["lex_sub_non_alphanum_count"] = df["tmp_concat_subdomains"].apply(
            lambda x: (sum(1 for c in x if not c.isalnum())) if len(x) > 0 else 0
        )  # Count of non-alphanumeric characters in concatenated subdomains
        df["lex_sub_non_alphanum_ratio"] = df["tmp_concat_subdomains"].apply(
            lambda x: (
                (sum(1 for c in x if not c.isalnum()) / len(x)) if len(x) > 0 else 0
            )
        )  # Ratio of non-alphanumeric characters in concatenated subdomains
        df["lex_sub_hex_count"] = df["tmp_concat_subdomains"].apply(
            lambda x: (sum(1 for c in x if c in HEX_CHARACTERS)) if len(x) > 0 else 0
        )  # Count of hexadecimal characters in concatenated subdomains
        df["lex_sub_hex_ratio"] = df["tmp_concat_subdomains"].apply(
            lambda x: (
                (sum(1 for c in x if c in HEX_CHARACTERS) / len(x)) if len(x) > 0 else 0
            )
        )  # Ratio of hexadecimal characters in concatenated subdomains
        return df

    def extract_ngram_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """Extract n-gram based features using preloaded n-gram frequency data from benign and malicious sources.

        Args:
            df (pd.DataFrame): DataFrame with domain names to process.

        Returns:
            pd.DataFrame: DataFrame updated with n-gram features.
        """
        df = df.copy(True)
        df["lex_dga_bigram_matches"] = df["tmp_concat_subdomains"].apply(
            find_ngram_matches, args=(self.ngram_freq_dga["bigram_freq"],)
        )  # Count of bigram matches with DGA model
        df["lex_dga_trigram_matches"] = df["tmp_concat_subdomains"].apply(
            find_ngram_matches, args=(self.ngram_freq_dga["trigram_freq"],)
        )  # Count of trigram matches with DGA model
        df["lex_dga_tetragram_matches"] = df["tmp_concat_subdomains"].apply(
            find_ngram_matches, args=(self.ngram_freq_dga["tetragram_freq"],)
        )  # Count of tetragram matches with DGA model
        df["lex_dga_pentagram_matches"] = df["tmp_concat_subdomains"].apply(
            find_ngram_matches, args=(self.ngram_freq_dga["pentagram_freq"],)
        )  # Count of pentagram matches with DGA model

        for n, ngram_set in self.ngram_set_benign.items():
            n_int = NGRAM_MAPPING[n]
            df[f"mod_jaccard_{n}-grams_benign"] = df["domain_name"].apply(
                lambda domain: modified_jaccard_index(
                    generate_ngrams(domain, n_int), ngram_set
                )
            )  # Modified Jaccard index for n-grams with benign model
        for n, ngram_set in self.ngram_set_dga.items():
            n_int = NGRAM_MAPPING[n]
            df[f"mod_jaccard_{n}-grams_dga"] = df["domain_name"].apply(
                lambda domain: modified_jaccard_index(
                    generate_ngrams(domain, n_int), ngram_set
                )
            )  # Modified Jaccard index for n-grams with DGA model

        df[
            [
                "kl_benign_bigrams",
                "kl_benign_trigrams",
                "kl_dga_bigrams",
                "kl_dga_trigrams",
            ]
        ] = df["domain_name"].apply(
            lambda x: pd.Series(
                compute_kl_for_domain(x, self.ngram_prob_benign, self.ngram_prob_dga)
            )
        )  # KL divergence for bigrams and trigrams comparing benign and DGA models

        return df
