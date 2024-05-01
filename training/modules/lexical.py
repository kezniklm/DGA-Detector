"""
 * @file lexical.py
 * @brief Lexical feature extraction for domain analysis datasets.
 *
 * This module provides functions for extracting lexical features from domain names in domain analysis datasets. These features are crucial for training machine learning models aimed at detecting phishing or malicious domain names.
 *
 * The main functionalities of this module include:
 * - Calculation of various lexical features such as domain name length, presence of digits or special characters, vowel and consonant counts, etc.
 * - Generation of n-grams and calculation of modified Jaccard index for comparing domain names against DGA (Domain Generation Algorithm) and benign models.
 * - Extraction of TLD-based features such as TLD length and abuse score.
 * - Subdomain-based feature extraction including counts of subdomains, digit ratios, entropy, etc.
 *
 * This tool is essential for preprocessing domain analysis datasets before training machine learning models to ensure accurate and effective detection of malicious domain names.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import json
import math
import sys
from itertools import groupby
from typing import Optional


import pandas as pd
import ahocorasick
import tldextract
from pandas import DataFrame

PHISHING_KEYWORDS = [
    "account",
    "action",
    "alert",
    "app",
    "auth",
    "bank",
    "billing",
    "center",
    "chat",
    "device",
    "fax",
    "event",
    "find",
    "free",
    "gift",
    "help",
    "info",
    "invoice",
    "live",
    "location",
    "login",
    "mail",
    "map",
    "message",
    "my",
    "new",
    "nitro",
    "now",
    "online",
    "pay",
    "promo",
    "real",
    "required",
    "safe",
    "secure",
    "security",
    "service",
    "signin",
    "support",
    "track",
    "update",
    "verification",
    "verify",
    "vm",
    "web",
]
"""List of common keywords used in phishing emails.

This list includes various words that are commonly found in phishing attempts.
These keywords are often used to trigger urgency or relate to sensitive user information.
"""

TLD_ABUSE_SCORES = {
    "com": 0.6554,
    "net": 0.1040,
    "eu": 0.0681,
    "name": 0.0651,
    "co": 0.0107,
    "life": 0.0087,
    "moe": 0.0081,
    "org": 0.0081,
    "xyz": 0.0072,
    "site": 0.0051,
    "ch": 0.0051,
    "it": 0.0048,
    "club": 0.0046,
    "info": 0.0043,
    "de": 0.0041,
    "racing": 0.0040,
    "live": 0.0035,
    "ru": 0.0034,
    "cc": 0.0034,
    "mobi": 0.0029,
    "me": 0.0023,
    "au": 0.0020,
    "cn": 0.0019,
    "pw": 0.0014,
    "in": 0.0011,
    "fr": 0.0010,
    "be": 0.0010,
    "pro": 0.0010,
    "top": 0.0009,
    "stream": 0.0007,
}
"""Dictionary mapping top-level domains to their abuse scores.

These scores represent the relative likelihood of abuse occurring from domains using these top-level domains,
based on data from ScoutDNS. A higher score indicates a higher observed rate of malicious activities.

Source: https://www.scoutdns.com/most-abused-top-level-domains-list-october-scoutdns/
"""

CONSONANTS = "bcdfghjklmnpqrstvwxyz"
"""String of all consonants in the alphabet.

Used for various operations where distinguishing between consonants and vowels is necessary.
"""

VOWELS = "aeiouy"
"""String of all vowels in the alphabet.

Used for various operations where distinguishing between vowels and consonants is necessary.
"""

HEX_CHARACTERS = "0123456789ABCDEFabcdef"
"""String of all hexadecimal characters.

Used in operations requiring hexadecimal validation or processing.
"""

NGRAM_MAPPING = {
    "bi": 2,
    "tri": 3,
    "tetra": 4,
    "penta": 5,
}
"""Dictionary mapping n-gram prefixes to their numerical values.

Used to translate n-gram text prefixes like 'bi', 'tri', etc., into their corresponding numerical values.
"""


def load_ngram_data(json_path: str) -> dict:
    """
    Loads JSON data from a file.

    @param json_path Path to the JSON file.
    @return Dictionary containing the loaded data, or an empty dictionary if an error occurs.
    """
    try:
        with open(json_path) as file:
            data = json.load(file)
        return data
    except FileNotFoundError:
        print(f"Error: The file {json_path} was not found.")
    except json.JSONDecodeError:
        print("Error: The file is not a valid JSON document.")
    except Exception as e:
        print(f"An unexpected error occurred: {e}")
    return {}


def build_automatons(
    ngram_freqs: dict[str, dict[str, int]]
) -> dict[str, ahocorasick.Automaton]:
    """
    Builds and returns a dictionary of Aho-Corasick automatons for each n-gram type provided.

    @param ngram_freqs: Dictionary where keys are n-gram types (e.g., "bigram_freq") and values are dictionaries of n-grams.
    @return: Dictionary of Aho-Corasick automatons for each n-gram type.
    """
    automatons = {}
    for ngram_type, ngrams in ngram_freqs.items():
        automaton = ahocorasick.Automaton()
        for ngram in ngrams:
            automaton.add_word(ngram, ngram)
        automaton.make_automaton()
        automatons[ngram_type] = automaton
    return automatons


def frequencies_to_probabilities(freq_dict: dict) -> dict:
    """
    Converts frequency counts to probabilities.

    @param freq_dict Dictionary of frequency counts.
    @return Dictionary with the same keys and values converted to probabilities.
    """
    total = sum(freq_dict.values())
    return {k: v / total for k, v in freq_dict.items()}


def ngram_frequency_to_probability(freq_dict: dict) -> dict:
    """
    Converts bigram and trigram frequencies to probabilities.

    @param freq_dict Dictionary with keys 'bigram_freq' and 'trigram_freq' containing frequency counts.
    @return Dictionary with the same structure and probabilities instead of frequencies.
    """
    prob_dict = dict()
    for ngram_type in ["bigram_freq", "trigram_freq"]:
        prob_dict[ngram_type] = frequencies_to_probabilities(freq_dict[ngram_type])
    return prob_dict


_ngram_freq_dga = load_ngram_data("ngram_freq_dga.json")
_ngram_freq_benign = load_ngram_data("ngram_freq.json")
_ngram_aho_corasick_automatons_dga = build_automatons(_ngram_freq_dga)
_ngram_set_dga = {
    n: set(_ngram_freq_dga[f"{n}gram_freq"].keys()) for n in ["bi", "tri", "penta"]
}
_ngram_set_benign = {
    n: set(_ngram_freq_benign[f"{n}gram_freq"].keys()) for n in ["bi", "tri", "penta"]
}


def longest_consonant_seq(domain: str) -> int:
    """
    Finds the length of the longest sequence of consonants in a domain name.

    @param domain The domain name.
    @return Length of the longest consonant sequence.
    """
    max_sequence_length = 0
    sequence_length = 0

    for char in domain.lower():
        if char.isalpha() and char not in VOWELS:
            sequence_length += 1
            if sequence_length > max_sequence_length:
                max_sequence_length = sequence_length
        else:
            sequence_length = 0

    return max_sequence_length


def count_subdomains(domain: str) -> int:
    """
    Counts the number of subdomains in a domain name, excluding 'www'.

    @param domain The full domain name.
    @return Number of subdomains.
    """
    subdomain = tldextract.extract(domain).subdomain

    if not subdomain:
        return 0

    subdomains_list = subdomain.replace("www.", "").split(".")

    filtered_subdomains = [s for s in subdomains_list if s]

    return len(filtered_subdomains)


def remove_tld(domain: str) -> str:
    """
    Removes the top-level domain (TLD) from a full domain name.

    @param domain Full domain name.
    @return Domain name without the TLD.
    """
    extracted = tldextract.extract(domain)

    non_tld_parts = [part for part in [extracted.subdomain, extracted.domain] if part]

    return ".".join(non_tld_parts)


def calculate_standard_deviation(values):
    """
    Calculate the standard deviation of a list of numerical values.

    Args:
        values (list): A list of numerical values.

    Returns:
        float: The standard deviation of the given values. Returns 0.0 if input is None or has less than 2 valid values.
    """

    if values is None:
        return 0.0

    valid_values = [float(x) for x in values if x is not None]

    if len(valid_values) < 2:
        return 0.0

    standard_deviation = np.std(valid_values)

    return float(standard_deviation)


def calculate_normalized_entropy(text: str) -> Optional[float]:
    """Calculate the normalized entropy of the input string.

    The function computes the frequency of each character in the string
    and then calculates the entropy based on these frequencies using the
    Shannon entropy formula. Normalization is achieved by dividing the entropy
    by the log base 2 of the number of unique characters in the string, which is the
    maximum possible entropy when each character appears with equal probability.

    Args:
        text (str): The input string.

    Returns:
        float: The normalized entropy of the string, or None if the string is empty.
    """
    text_len = len(text)
    if text_len == 0:
        return 0

    freqs = {}
    for char in text:
        if char in freqs:
            freqs[char] += 1
        else:
            freqs[char] = 1

    entropy = 0.0
    for f in freqs.values():
        p = float(f) / text_len
        entropy -= p * math.log(p, 2)
    return entropy / text_len


def vowel_count(domain: str) -> int:
    """
    Counts the number of vowels in a domain name.

    @param domain The domain name.
    @return Number of vowels in the domain name.
    """

    return sum(1 for char in domain.lower() if char in VOWELS)


def consecutive_chars(domain: str) -> int:
    """
    Counts the maximum number of consecutive characters in a domain name.

    @param domain The domain name to analyze.
    @return Maximum count of consecutive characters.
    """
    if not domain:
        return 0

    return max(len(list(group)) for _, group in groupby(domain))


def find_ngram_matches(text: str, automaton: ahocorasick.Automaton):
    """
    Uses a precompiled Aho-Corasick automaton to count unique n-gram matches in the text.
    The search is case-insensitive.

    @param text: Text in which to find n-grams.
    @param automaton: Precompiled Aho-Corasick automaton.
    @return: Number of unique matches found.
    """
    matched_ngrams = set()
    for _, found_ngram in automaton.iter(text):
        matched_ngrams.add(found_ngram)
    return len(matched_ngrams)


def get_lengths_of_parts(dn: str):
    """
    Splits the domain name into parts separated by dots and returns their lengths.

    @param dn Full domain name.
    @return List of integers representing the lengths of each part.
    """
    return list(map(len, dn.split(".")))


def get_tld_abuse_score(tld: str) -> int:
    """
    Retrieves the abuse score for a top-level domain (TLD).

    @param tld The top-level domain.
    @return Abuse score if available, otherwise 0.
    """
    tld = tld.lstrip(".")

    return TLD_ABUSE_SCORES.get(tld, 0)


def modified_jaccard_index(set1: set, set2: set) -> float:
    """
    Computes a modified Jaccard index between two sets.

    @param set1 First set.
    @param set2 Second set.
    @return Jaccard index as a float.
    """
    intersection = set1.intersection(set2)
    if not set1:
        return 0
    return len(intersection) / len(set1)


def generate_ngrams(text: str, n: int) -> set[str]:
    """
    Generates a set of n-grams from the given text.

    @param text Text to generate n-grams from.
    @param n Length of each n-gram.
    @return Set of n-grams.
    """
    return {text[i : i + n] for i in range(len(text) - n + 1)}


def lex(df: DataFrame) -> DataFrame:
    """
    Calculate domain lexical features.
    Input: DF with domain_name column
    Output: DF with lexical features derived from domain_name added
    """

    # The dataframe tends to get fragmented here; this should defragment it
    df = df.copy(True)

    df["lex_name_len"] = df["domain_name"].apply(len)
    df["lex_has_digit"] = df["domain_name"].apply(
        lambda x: 1 if sum([1 for y in x if y.isdigit()]) > 0 else 0
    )
    df["lex_phishing_keyword_count"] = df["domain_name"].apply(
        lambda x: sum(1 for w in PHISHING_KEYWORDS if w in x)
    )
    df["lex_consecutive_chars"] = df["domain_name"].apply(
        lambda x: consecutive_chars(x)
    )

    # Save temporary domain name parts for lex feature calculation
    df["tmp_tld"] = df["domain_name"].apply(lambda x: tldextract.extract(x).suffix)
    df["tmp_sld"] = df["domain_name"].apply(lambda x: tldextract.extract(x).domain)
    df["tmp_stld"] = df["tmp_sld"] + "." + df["tmp_tld"]
    df["tmp_concat_subdomains"] = df["domain_name"].apply(
        lambda x: remove_tld(x).replace(".", "")
    )

    # TLD-based features
    df["lex_tld_len"] = df["tmp_tld"].apply(len)  # Length of TLD
    df["lex_tld_abuse_score"] = df["tmp_tld"].apply(
        get_tld_abuse_score
    )  # TLD abuse score

    # SLD-based features
    df["lex_sld_len"] = df["tmp_sld"].apply(len)  # Length of SLD
    df["lex_sld_norm_entropy"] = df["tmp_sld"].apply(
        calculate_normalized_entropy
    )  # Normalized entropy od the SLD only
    df["lex_sld_digit_count"] = (
        df["tmp_sld"]
        .apply(lambda x: (sum([1 for y in x if y.isdigit()])) if len(x) > 0 else 0)
        .astype("float")
    )
    df["lex_sld_digit_ratio"] = df["tmp_sld"].apply(
        # Digit ratio in subdomains
        lambda x: (sum([1 for y in x if y.isdigit()]) / len(x)) if len(x) > 0 else 0
    )
    df["lex_sld_phishing_keyword_count"] = df["tmp_sld"].apply(
        lambda x: sum(1 for w in PHISHING_KEYWORDS if w in x)
    )
    df["lex_sld_vowel_count"] = df["tmp_sld"].apply(lambda x: vowel_count(x))
    df["lex_sld_vowel_ratio"] = df["tmp_sld"].apply(
        lambda x: (vowel_count(x) / len(x)) if len(x) > 0 else 0
    )
    df["lex_sld_consonant_count"] = df["tmp_sld"].apply(
        lambda x: (
            (sum(1 for c in x if c in "bcdfghjklmnpqrstvwxyz")) if len(x) > 0 else 0
        )
    )
    df["lex_sld_consonant_ratio"] = df["tmp_sld"].apply(
        lambda x: (
            (sum(1 for c in x if c in "bcdfghjklmnpqrstvwxyz") / len(x))
            if len(x) > 0
            else 0
        )
    )
    df["lex_sld_non_alphanum_count"] = df["tmp_sld"].apply(
        lambda x: (sum(1 for c in x if not c.isalnum())) if len(x) > 0 else 0
    )
    df["lex_sld_non_alphanum_ratio"] = df["tmp_sld"].apply(
        lambda x: (sum(1 for c in x if not c.isalnum()) / len(x)) if len(x) > 0 else 0
    )
    df["lex_sld_hex_count"] = df["tmp_sld"].apply(
        lambda x: (
            (sum(1 for c in x if c in "0123456789ABCDEFabcdef")) if len(x) > 0 else 0
        )
    )
    df["lex_sld_hex_ratio"] = df["tmp_sld"].apply(
        lambda x: (
            (sum(1 for c in x if c in "0123456789ABCDEFabcdef") / len(x))
            if len(x) > 0
            else 0
        )
    )
    # End of new SLD-based features

    df["lex_sub_count"] = df["domain_name"].apply(
        lambda x: count_subdomains(x)
    )  # Number of subdomains (without www)
    df["lex_stld_unique_char_count"] = df["tmp_stld"].apply(
        # Number of unique characters in TLD and SLD
        lambda x: len(set(x.replace(".", "")))
    )
    df["lex_begins_with_digit"] = df["domain_name"].apply(
        lambda x: 1 if x[0].isdigit() else 0
    )  # Is first character a digit
    df["lex_sub_max_consonant_len"] = df["tmp_concat_subdomains"].apply(
        longest_consonant_seq
    )  # Max consonant sequence length
    df["lex_sub_norm_entropy"] = df["tmp_concat_subdomains"].apply(
        # Normalized entropy od the domain name (without TLD)
        calculate_normalized_entropy
    )
    df["lex_sub_digit_count"] = (
        df["tmp_concat_subdomains"]
        .apply(lambda x: (sum([1 for y in x if y.isdigit()])) if len(x) > 0 else 0)
        .astype("float")
    )
    df["lex_sub_digit_ratio"] = df["tmp_concat_subdomains"].apply(
        # Digit ratio in subdomains
        lambda x: (sum([1 for y in x if y.isdigit()]) / len(x)) if len(x) > 0 else 0
    )
    df["lex_sub_vowel_count"] = df["tmp_concat_subdomains"].apply(
        lambda x: vowel_count(x)
    )
    df["lex_sub_vowel_ratio"] = df["tmp_concat_subdomains"].apply(
        lambda x: (vowel_count(x) / len(x)) if len(x) > 0 else 0
    )
    df["lex_sub_consonant_count"] = df["tmp_concat_subdomains"].apply(
        lambda x: (
            (sum(1 for c in x if c in "bcdfghjklmnpqrstvwxyz")) if len(x) > 0 else 0
        )
    )
    df["lex_sub_consonant_ratio"] = df["tmp_concat_subdomains"].apply(
        lambda x: (
            (sum(1 for c in x if c in "bcdfghjklmnpqrstvwxyz") / len(x))
            if len(x) > 0
            else 0
        )
    )
    df["lex_sub_non_alphanum_count"] = df["tmp_concat_subdomains"].apply(
        lambda x: (sum(1 for c in x if not c.isalnum())) if len(x) > 0 else 0
    )
    df["lex_sub_non_alphanum_ratio"] = df["tmp_concat_subdomains"].apply(
        lambda x: (sum(1 for c in x if not c.isalnum()) / len(x)) if len(x) > 0 else 0
    )
    df["lex_sub_hex_count"] = df["tmp_concat_subdomains"].apply(
        lambda x: (
            (sum(1 for c in x if c in "0123456789ABCDEFabcdef")) if len(x) > 0 else 0
        )
    )
    df["lex_sub_hex_ratio"] = df["tmp_concat_subdomains"].apply(
        lambda x: (
            (sum(1 for c in x if c in "0123456789ABCDEFabcdef") / len(x))
            if len(x) > 0
            else 0
        )
    )

    # N-Grams
    df["lex_dga_bigram_matches"] = df["tmp_concat_subdomains"].apply(
        find_ngram_matches,
        args=(_ngram_aho_corasick_automatons_dga["bigram_freq"],),
    )  # Count of bigram matches with DGA model
    df["lex_dga_trigram_matches"] = df["tmp_concat_subdomains"].apply(
        find_ngram_matches,
        args=(_ngram_aho_corasick_automatons_dga["trigram_freq"],),
    )  # Count of trigram matches with DGA model
    df["lex_dga_tetragram_matches"] = df["tmp_concat_subdomains"].apply(
        find_ngram_matches,
        args=(_ngram_aho_corasick_automatons_dga["tetragram_freq"],),
    )  # Count of tetragram matches with DGA model
    df["lex_dga_pentagram_matches"] = df["tmp_concat_subdomains"].apply(
        find_ngram_matches,
        args=(_ngram_aho_corasick_automatons_dga["pentagram_freq"],),
    )  # Count of pentagram matches with DGA model

    for n, ngram_set in _ngram_set_benign.items():
        n_int = NGRAM_MAPPING[n]
        df[f"mod_jaccard_{n}-grams_benign"] = df["domain_name"].apply(
            lambda domain: modified_jaccard_index(
                generate_ngrams(domain, n_int), ngram_set
            )
        )
    for n, ngram_set in _ngram_set_dga.items():
        n_int = NGRAM_MAPPING[n]
        df[f"mod_jaccard_{n}-grams_dga"] = df["domain_name"].apply(
            lambda domain: modified_jaccard_index(
                generate_ngrams(domain, n_int), ngram_set
            )
        )

    # Part lengths
    df["tmp_part_lengths"] = df["domain_name"].apply(lambda x: get_lengths_of_parts(x))
    df["lex_avg_part_len"] = df["tmp_part_lengths"].apply(
        lambda x: sum(x) / len(x) if len(x) > 0 else 0
    )
    df["lex_stdev_part_lens"] = df["tmp_part_lengths"].apply(
        lambda x: calculate_normalized_entropy(x) if len(x) > 0 else 0
    )
    df["lex_longest_part_len"] = df["tmp_part_lengths"].apply(
        lambda x: max(x) if len(x) > 0 else 0
    )

    # Length distribution
    df["lex_shortest_sub_len"] = df["tmp_concat_subdomains"].apply(
        lambda x: (
            min(get_lengths_of_parts(x)) if len(get_lengths_of_parts(x)) > 0 else 0
        )
    )

    # Drop temporary columns
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


def main(input_file: str, output_file: str):
    df = pd.read_parquet(input_file, columns=['domain_name'])

    df_features = lex(df)

    df_features.to_parquet(output_file, index=False)


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: ./lexical.py <input_file> <output_file>")
        sys.exit(1)
        
    input_file, output_file = sys.argv[1], sys.argv[2]
    main(input_file, output_file)

