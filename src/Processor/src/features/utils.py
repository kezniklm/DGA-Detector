"""
 * @file utils.py
 * @brief Utility functions for feature extraction.
 *
 * This file contains a collection of utility functions essential at identifying risks associated with DGA domain names.
 *
 * The main functionalities of this file include:
 * - Parsing and extracting components from domain names to facilitate risk assessments.
 * - Converting frequency data into probability distributions to aid in statistical analysis.
 * - Calculating various string metrics such as vowel counts, consecutive character sequences, and entropy, which are crucial in evaluating domain name anomalies.
 * - Generating and matching n-grams to detect patterns that deviate from benign profiles.
 * - Assessing the abuse risk of top-level domains and quantifying subdomain proliferation to predict domain-based threats.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import json
import math
import warnings
from collections import Counter
from itertools import groupby
from typing import Optional

import ahocorasick
import numpy as np
import tldextract

from .constants import TLD_ABUSE_SCORES, VOWELS

warnings.filterwarnings(
    "ignore", category=FutureWarning, module="numpy.core.fromnumeric"
)


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


def consecutive_chars(domain: str) -> int:
    """
    Counts the maximum number of consecutive characters in a domain name.

    @param domain The domain name to analyze.
    @return Maximum count of consecutive characters.
    """
    if not domain:
        return 0

    return max(len(list(group)) for _, group in groupby(domain))


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


def vowel_count(domain: str) -> int:
    """
    Counts the number of vowels in a domain name.

    @param domain The domain name.
    @return Number of vowels in the domain name.
    """

    return sum(1 for char in domain.lower() if char in VOWELS)


def remove_tld(domain: str) -> str:
    """
    Removes the top-level domain (TLD) from a full domain name.

    @param domain Full domain name.
    @return Domain name without the TLD.
    """
    extracted = tldextract.extract(domain)

    non_tld_parts = [part for part in [extracted.subdomain, extracted.domain] if part]

    return ".".join(non_tld_parts)


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
