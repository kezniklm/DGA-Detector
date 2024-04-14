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
    def __init__(self, num_processes: int) -> None:
        self.num_processes: int = num_processes
        self.initialise_feature_extractor()

    def initialise_feature_extractor(self) -> None:
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
        df = df.copy(True)
        df_split = np.array_split(df, self.num_processes)
        df_features = pd.DataFrame()
        with Pool(processes=self.num_processes) as pool:
            results = pool.map(self._apply_features, df_split)
            df_features = pd.concat(results)
        return df_features

    def _apply_features(self, df: pd.DataFrame) -> pd.DataFrame:
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
        df["lex_shortest_sub_len"] = df["tmp_concat_subdomains"].apply(
            lambda x: (
                min(get_lengths_of_parts(x)) if len(get_lengths_of_parts(x)) > 0 else 0
            )
        )
        return df

    def extract_tld_features(self, df: pd.DataFrame) -> pd.DataFrame:
        df = df.copy(True)
        df["lex_tld_len"] = df["tmp_tld"].apply(len)  # Length of TLD
        df["lex_tld_abuse_score"] = df["tmp_tld"].apply(
            get_tld_abuse_score
        )  # TLD abuse score
        return df

    def extract_sld_features(self, df: pd.DataFrame) -> pd.DataFrame:
        df = df.copy(True)

        # SLD-based features
        df["lex_sld_len"] = df["tmp_sld"].apply(len)  # Length of SLD
        df["lex_sld_norm_entropy"] = df["tmp_sld"].apply(
            get_normalized_entropy
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
            lambda x: (sum(1 for c in x if c in CONSONANTS)) if len(x) > 0 else 0
        )
        df["lex_sld_consonant_ratio"] = df["tmp_sld"].apply(
            lambda x: (
                (sum(1 for c in x if c in CONSONANTS) / len(x)) if len(x) > 0 else 0
            )
        )
        df["lex_sld_non_alphanum_count"] = df["tmp_sld"].apply(
            lambda x: (sum(1 for c in x if not c.isalnum())) if len(x) > 0 else 0
        )
        df["lex_sld_non_alphanum_ratio"] = df["tmp_sld"].apply(
            lambda x: (
                (sum(1 for c in x if not c.isalnum()) / len(x)) if len(x) > 0 else 0
            )
        )
        df["lex_sld_hex_count"] = df["tmp_sld"].apply(
            lambda x: (sum(1 for c in x if c in HEX_CHARACTERS)) if len(x) > 0 else 0
        )
        df["lex_sld_hex_ratio"] = df["tmp_sld"].apply(
            lambda x: (
                (sum(1 for c in x if c in HEX_CHARACTERS) / len(x)) if len(x) > 0 else 0
            )
        )
        return df

    def extract_temp_collumns(self, df: pd.DataFrame) -> pd.DataFrame:
        df = df.copy(True)
        df["tmp_tld"] = df["domain_name"].apply(lambda x: tldextract.extract(x).suffix)
        df["tmp_sld"] = df["domain_name"].apply(lambda x: tldextract.extract(x).domain)
        df["tmp_stld"] = df["tmp_sld"] + "." + df["tmp_tld"]
        df["tmp_concat_subdomains"] = df["domain_name"].apply(
            lambda x: remove_tld(x).replace(".", "")
        )
        df["tmp_part_lengths"] = df["domain_name"].apply(
            lambda x: get_lengths_of_parts(x)
        )
        return df

    def drop_temp_collumns(self, df: pd.DataFrame) -> pd.DataFrame:
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
        df = df.copy(True)
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
        df["lex_www_flag"] = df["domain_name"].apply(
            lambda x: 1 if (x.split("."))[0] == "www" else 0
        )  # Begins with www
        df["lex_sub_max_consonant_len"] = df["tmp_concat_subdomains"].apply(
            longest_consonant_seq
        )  # Max consonant sequence length
        df["lex_sub_norm_entropy"] = df["tmp_concat_subdomains"].apply(
            # Normalized entropy od the domain name (without TLD)
            get_normalized_entropy
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
            lambda x: (sum(1 for c in x if c in CONSONANTS)) if len(x) > 0 else 0
        )
        df["lex_sub_consonant_ratio"] = df["tmp_concat_subdomains"].apply(
            lambda x: (
                (sum(1 for c in x if c in CONSONANTS) / len(x)) if len(x) > 0 else 0
            )
        )
        df["lex_sub_non_alphanum_count"] = df["tmp_concat_subdomains"].apply(
            lambda x: (sum(1 for c in x if not c.isalnum())) if len(x) > 0 else 0
        )
        df["lex_sub_non_alphanum_ratio"] = df["tmp_concat_subdomains"].apply(
            lambda x: (
                (sum(1 for c in x if not c.isalnum()) / len(x)) if len(x) > 0 else 0
            )
        )
        df["lex_sub_hex_count"] = df["tmp_concat_subdomains"].apply(
            lambda x: (sum(1 for c in x if c in HEX_CHARACTERS)) if len(x) > 0 else 0
        )
        df["lex_sub_hex_ratio"] = df["tmp_concat_subdomains"].apply(
            lambda x: (
                (sum(1 for c in x if c in HEX_CHARACTERS) / len(x)) if len(x) > 0 else 0
            )
        )
        return df

    def extract_ngram_features(self, df: pd.DataFrame) -> pd.DataFrame:
        df = df.copy(True)
        df["lex_dga_bigram_matches"] = df["tmp_concat_subdomains"].apply(
            find_ngram_matches, args=(self.ngram_freq_dga["bigram_freq"],)
        )
        df["lex_dga_trigram_matches"] = df["tmp_concat_subdomains"].apply(
            find_ngram_matches, args=(self.ngram_freq_dga["trigram_freq"],)
        )
        df["lex_dga_tetragram_matches"] = df["tmp_concat_subdomains"].apply(
            find_ngram_matches, args=(self.ngram_freq_dga["tetragram_freq"],)
        )
        df["lex_dga_pentagram_matches"] = df["tmp_concat_subdomains"].apply(
            find_ngram_matches, args=(self.ngram_freq_dga["pentagram_freq"],)
        )

        for n, ngram_set in self.ngram_set_benign.items():
            n_int = NGRAM_MAPPING[n]
            df[f"mod_jaccard_{n}-grams_benign"] = df["domain_name"].apply(
                lambda domain: modified_jaccard_index(
                    generate_ngrams(domain, n_int), ngram_set
                )
            )
        for n, ngram_set in self.ngram_set_dga.items():
            n_int = NGRAM_MAPPING[n]
            df[f"mod_jaccard_{n}-grams_dga"] = df["domain_name"].apply(
                lambda domain: modified_jaccard_index(
                    generate_ngrams(domain, n_int), ngram_set
                )
            )

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
        )

        return df
