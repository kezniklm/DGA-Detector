import json
import math
import warnings
from collections import Counter
from typing import Optional

import numpy as np
import tldextract

from .constants import CONSONANTS, TLD_ABUSE_SCORES, VOWELS

warnings.filterwarnings(
    "ignore", category=FutureWarning, module="numpy.core.fromnumeric"
)


def load_ngram_data(json_path: str) -> dict:
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
    total = sum(freq_dict.values())
    return {k: v / total for k, v in freq_dict.items()}


def ngram_frequency_to_probability(freq_dict: dict) -> dict:
    prob_dict = dict()
    for ngram_type in ["bigram_freq", "trigram_freq"]:
        prob_dict[ngram_type] = frequencies_to_probabilities(freq_dict[ngram_type])
    return prob_dict


def consecutive_chars(domain: str) -> int:
    """Function returns the number of consecutive
    characters.

    Args:
        domain (str): The domain name

    Returns:
        int: Number of consecutive characters
    """
    if len(domain) == 0:
        return 0

    max_count = 1
    count = 1
    prev_char = domain[0]
    for char in domain[1:]:
        if char == prev_char:
            count += 1
            max_count = max(max_count, count)
        else:
            count = 1
        prev_char = char
    return max_count


def get_lengths_of_parts(dn: str):
    # Split the domain string into parts divided by dots
    domain_parts = dn.split(".")

    # Get the length of each part and store in a list
    part_lens = [len(part) for part in domain_parts]
    return part_lens


def get_tld_abuse_score(tld):
    # Dictionary containing the abuse scores for the provided TLDs

    # Remove the dot from the start of the TLD if it exists
    tld = tld.lstrip(".")

    # Return the abuse score if the TLD is in the dictionary, otherwise return 0
    return TLD_ABUSE_SCORES.get(tld, 0)


def vowel_count(domain: str) -> int:
    """Function returns the number of vowels in
    the domain name
    Args:
        domain (str): The domain name
    Returns:
        int: Number of vowels
    """

    return sum(1 for char in domain.lower() if char in VOWELS)


def remove_tld(domain: str) -> str:
    """Function removes tld from
    the domain name

    Args:
        domain (str): Domain name

    Returns:
        str: Domain without TLD
    """
    ext = tldextract.extract(domain)
    subdomain = ext.subdomain
    sld = ext.domain
    result = subdomain + "." + sld if subdomain else sld
    return result


def count_subdomains(domain: str) -> int:
    """
    Function returns the number of subdomains
    in the domain name

    Args:
        domain (str): The domain name

    Returns:
        int: Number of subdomains
    """
    ext = tldextract.extract(domain)
    if not ext.subdomain:
        return 0

    else:
        subdomains = ext.subdomain.split(".")
        subdomains_count = len(subdomains)
        if "www" in subdomains:
            subdomains_count -= 1
        return subdomains_count


def longest_consonant_seq(domain: str) -> int:
    """Function returns longest consonant sequence

    Args:
        domain (str): domain name

    Returns:
        int: length of the longest consonant sequence
    """
    current_len = 0
    max_len = 0
    domain = domain.lower()
    for char in domain:
        if char in CONSONANTS:
            current_len += 1
        else:
            current_len = 0
        if current_len > max_len:
            max_len = current_len
    return max_len


def find_ngram_matches(text: str, ngrams: dict) -> int:
    """
    Find the number of ngram matches in the text.
    Input: text string, ngrams dictionary
    Output: number of matches
    """
    matches = 0
    for ngram in ngrams:
        if ngram in text:
            matches += 1
    return matches


def modified_jaccard_index(set1, set2):
    intersection = set1.intersection(set2)
    if not set1:
        return 0
    return len(intersection) / len(set1)


def generate_ngrams(text, n):
    return {text[i : i + n] for i in range(len(text) - n + 1)}


def compute_kl_for_domain(domain, benign_ngrams, dga_ngrams):
    head, _ = domain.split(".", 1)
    bigrams = generate_ngrams(head, 2)
    trigrams = generate_ngrams(head, 3)

    bigram_freqs = frequencies_to_probabilities(Counter(bigrams))
    trigram_freqs = frequencies_to_probabilities(Counter(trigrams))

    kl_benign_2 = kl_divergence(bigram_freqs, benign_ngrams["bigram_freq"])
    kl_benign_3 = kl_divergence(trigram_freqs, benign_ngrams["trigram_freq"])
    kl_dga_2 = kl_divergence(bigram_freqs, dga_ngrams["bigram_freq"])
    kl_dga_3 = kl_divergence(trigram_freqs, dga_ngrams["trigram_freq"])

    return kl_benign_2, kl_benign_3, kl_dga_2, kl_dga_3


def kl_divergence(p, q):
    p_keys = list(p.keys())
    # Avoid log(0) by adding a small epsilon
    p_values = np.array([p.get(k, 1e-10) for k in p_keys])
    q_values = np.array([q.get(k, 1e-10) for k in p_keys])
    return np.sum(p_values * np.log(p_values / q_values))


def get_normalized_entropy(text: str) -> Optional[float]:
    """Function returns the normalized entropy of the
    string. The function first computes the frequency
    of each character in the string using
    the collections.Counter function.
    It then uses the formula for entropy to compute
    the entropy of the string, and normalizes it by
    dividing it by the maximum possible entropy
    (which is the logarithm of the minimum of the length
    of the string and the number of distinct characters
    in the string).

    Args:
        text (str): the string

    Returns:
        float: normalized entropy
    """
    text_len = len(text)
    if text_len == 0:
        # return None
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
