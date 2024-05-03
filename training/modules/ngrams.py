"""
 * @file ngrams.py
 * @brief This module provides tools for n-gram analysis of domain names.
 * @details This module is part of the 'dr-playground' repository available under the MIT License. It includes functionalities for extracting domain names, generating n-grams, and analyzing their frequency.
 *
 * Source: GitHub Repository 'dr-playground' by OviOvocny
 * URL: https://github.com/OviOvocny/dr-playground
 * License: MIT
 * Original Author: Petr Pouč (https://github.com/surViVeCZ)
 * Date: 2024/05/03
 *
"""

import json

import pandas as pd
import tldextract
from nltk import FreqDist, ngrams


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


class NgramsAnalyzer:
    """
    A class to analyze domain names and generate frequency distributions of bigrams, trigrams, and tetragrams.
    """

    def __init__(self, file_path):
        """
        Initialize the NgramsAnalyzer object.
        Args:
            file_path (str): The path to the Parquet file.
        """
        self.file_path = file_path
        self.df = None
        self.domain_names = []
        self.all_bigrams = []
        self.all_trigrams = []
        self.all_tetragrams = []  # Added for tetragrams
        self.all_pentagrams = []  # Added for pentagrams

    def load_data(self):
        """
        Load the Parquet file into a DataFrame.
        """
        self.df = pd.read_parquet(self.file_path)

    def extract_domain_name(self):
        """
        Extract the domain names from the DataFrame.
        """
        self.domain_names = self.df["domain_name"].tolist()

    def generate_ngrams(self):
        """
        Generate all bigrams, trigrams, and tetragrams from the domain name.
        """
        for domain in self.domain_names:
            dom = remove_tld(domain)
            domain_bigrams = list(ngrams(dom, 2))
            domain_trigrams = list(ngrams(dom, 3))
            domain_tetragrams = list(ngrams(dom, 4))  # Added for tetragrams
            domain_pentagrams = list(ngrams(dom, 5))  # Added for pentagrams
            self.all_bigrams.extend(domain_bigrams)
            self.all_trigrams.extend(domain_trigrams)
            self.all_tetragrams.extend(domain_tetragrams)  # Added for tetragrams
            self.all_pentagrams.extend(domain_pentagrams)  # Added for pentagrams

    def get_most_common_bigrams(self, n):
        """
        Get the n most common bigrams.

        Args:
            n (int): The number of most frequent bigrams we want to retrieve.

        Returns:
            list: Most common bigrams and their frequencies.
        """
        bigram_freq_dist = FreqDist(self.all_bigrams)
        return bigram_freq_dist.most_common(n)

    def get_most_common_trigrams(self, n):
        """
        Get the n most common trigrams.

        Args:
            n (int): The number of most frequent trigrams we want to retrieve.

        Returns:
            list: Most common trigrams and their frequencies.
        """
        trigram_freq_dist = FreqDist(self.all_trigrams)
        return trigram_freq_dist.most_common(n)

    def get_most_common_tetragrams(self, n):
        """
        Get the n most common tetragrams.
        Args:
            n (int): The number of most frequent tetragrams we want to retrieve.
        Returns:
            list: Most common tetragrams and their frequencies.
        """
        tetragram_freq_dist = FreqDist(self.all_pentagrams)
        return tetragram_freq_dist.most_common(n)

    def get_most_common_pentagrams(self, n):
        """
        Get the n most common all_pentagrams.
        Args:
            n (int): The number of most frequent pentagrams we want to retrieve.
        Returns:
            list: Most common pentagrams and their frequencies.
        """
        tetragram_freq_dist = FreqDist(self.all_pentagrams)
        return tetragram_freq_dist.most_common(n)

    def save_to_json(
        self, bigram_data, trigram_data, tetragram_data, pentagram_data, filename
    ):
        """
        Save the bigram, trigram, and tetragram data to a JSON file.
        Args:
            bigram_data (dict): Dictionary of bigrams and their frequencies.
            trigram_data (dict): Dictionary of trigrams and their frequencies.
            tetragram_data (dict): Dictionary of tetragrams and their frequencies.  # Added for tetragrams
            pentagram_data (dict): Dictionary of pentagrams and their frequencies.  # Added for pentagrams
            filename (str): Name of the JSON file to save.
        """
        data = {
            "bigram_freq": bigram_data,
            "trigram_freq": trigram_data,
            "tetragram_freq": tetragram_data,  # Added for tetragrams
            "pentagram_freq": pentagram_data,  # Added for pentagrams
        }
        with open(filename, "w") as file:
            json.dump(data, file)

    def analyze_ngrams(
        self,
        outfile,
        bigram_n=10000,
        trigram_n=10000,
        tetragram_n=10000,
        pentagram_n=10000,
    ):
        """
        Analyze the ngrams and save the results to a JSON file.
        Args:
            outfile (str): The name of the output JSON file.
            bigram_n (int): The number of most frequent bigrams to retrieve.
            trigram_n (int): The number of most frequent trigrams to retrieve.
            tetragram_n (int): The number of most frequent tetragrams to retrieve.
            pentagram_n (int): The number of most frequent pentagrams to retrieve.
        """
        self.load_data()
        self.extract_domain_name()
        self.generate_ngrams()

        # Get the specified number of common ngrams for each category
        most_common_bigrams = self.get_most_common_bigrams(bigram_n)
        most_common_trigrams = self.get_most_common_trigrams(trigram_n)
        most_common_tetragrams = self.get_most_common_tetragrams(tetragram_n)
        most_common_pentagrams = self.get_most_common_pentagrams(pentagram_n)

        # Convert to dictionaries for better manipulation
        bigram_dict = {
            ("".join(bigram)): count for bigram, count in most_common_bigrams
        }
        trigram_dict = {
            ("".join(trigram)): count for trigram, count in most_common_trigrams
        }
        tetragram_dict = {
            ("".join(tetragram)): count for tetragram, count in most_common_tetragrams
        }
        pentagram_dict = {
            ("".join(pentagram)): count for pentagram, count in most_common_pentagrams
        }

        # Save frequency distributions to a single JSON file
        self.save_to_json(
            bigram_dict, trigram_dict, tetragram_dict, pentagram_dict, outfile
        )


if __name__ == "__main__":

    # Number of avaliable symbols:
    # 2*26 letters + 10 digits + 1 dash + 1 dot = 64
    #
    # Number of possible bigrams:
    # 64 * 64 = 4096
    #
    # Number of possible trigrams:
    # 64 * 64 * 64 = 262,144
    #
    # Number of possible tetragrams:
    # 64 * 64 * 64 * 64 = 16,777,216
    #
    # Number of possible pentagrams:
    # 64 * 64 * 64 * 64 * 64 = 1,073,741,824
    #
    # NOTE: Choose the n-gram count as 1-50% of possible ngrams.
    #       Higher numbers would make the feature useless.
    analyzer = NgramsAnalyzer("floor/01-Raw-data/Benign/Benign.parquet")

    analyzer.analyze_ngrams(
        "ngram_freq.json",
        bigram_n=1000,
        trigram_n=5000,
        tetragram_n=10000,
        pentagram_n=50000,
    )

    analyzer = NgramsAnalyzer(
        "floor/01-Raw-data/DGA/01-Proportion-pick/01-Proportion-pick.parquet"
    )
    analyzer.analyze_ngrams(
        "ngram_freq_dga.json",
        bigram_n=1000,
        trigram_n=5000,
        tetragram_n=10000,
        pentagram_n=50000,
    )
