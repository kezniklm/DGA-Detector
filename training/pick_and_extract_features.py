"""
 * @file pick_and_extract_features.py
 * @brief Main module for DGA domain processing workflow.
 *
 * This script orchestrates a comprehensive DGA (Domain Generation Algorithm) domain processing workflow, 
 * including proportional domain picking, n-gram analysis, and lexical feature extraction from domain names. 
 * The script operates across multiple stages, each designed to refine the data and extract meaningful 
 * insights that aid in the detection of DGA-generated domains.
 *
 * Main functionalities include:
 * - Proportional selection of domains based on predefined statistical criteria.
 * - N-gram analysis to uncover underlying patterns and frequencies in domain strings.
 * - Extraction of lexical features from domains to assist in classifying their origins.
 * - Efficient handling and storage of large datasets using pandas and PyArrow.
 *
 * This module is key for cybersecurity applications where quick identification of DGA domains is critical
 * to mitigating threats and enhancing network security.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

from typing import Tuple
import os
import shutil
import pandas as pd
from modules import lexical
from modules.ngrams import NgramsAnalyzer
from modules.proportional_picker import proportional_picker

# Configuration
DGA_DATASET_PATH: str = "floor/01-Raw-data/DGA/DGA-parquet-files.zip"
DGA_STATS_FILE: str = "floor/01-Raw-data/DGA/DGA-Families-Summary.parquet"
DGA_PICK_OUTPUT_PATH: str = "floor/01-Raw-data/DGA/"
DGA_FEATURES_OUTPUT_PATH: str = "floor/02-Preprocessed-data/DGA/"
NUMBER_OF_ITERATIONS: int = 10


def pick_domains(i: int) -> Tuple[str, str]:
    """
    Picks domain names from a dataset based on proportionality criteria.

    @param i Index of the current iteration to create unique output paths.
    @return Tuple containing the path to the output file and the output directory.
    """
    output_pick_folder: str = os.path.join(DGA_PICK_OUTPUT_PATH, f"{i:02}-Proportion-pick")
    os.makedirs(output_pick_folder, exist_ok=True)
    output_file: str = os.path.join(output_pick_folder, f"{i:02}-Proportion-pick.parquet")

    df: pd.DataFrame = proportional_picker(DGA_DATASET_PATH, DGA_STATS_FILE)
    df.columns = ["domain_name"]
    df.to_parquet(output_file, index=False)
    return output_file, output_pick_folder


def analyze_ngrams(output_file: str, output_pick_folder: str) -> str:
    """
    Analyzes and saves the n-gram frequencies of domain names from the provided file.

    @param output_file Path to the file containing domain names.
    @param output_pick_folder Directory path where the n-gram frequency file will be saved.
    @return Path to the n-gram frequency output file.
    """
    analyzer: NgramsAnalyzer = NgramsAnalyzer(output_file)
    ngram_output_file: str = os.path.join(output_pick_folder, "ngram_freq_dga.json")
    analyzer.analyze_ngrams(
        ngram_output_file,
        bigram_n=1000,
        trigram_n=5000,
        tetragram_n=10000,
        pentagram_n=50000,
    )
    return ngram_output_file


def copy_ngrams_to_current_dir(ngram_file: str) -> None:
    """
    Copies the n-gram frequency file to the current directory.

    @param ngram_file Path to the n-gram frequency file to be copied.
    """
    shutil.copy(ngram_file, ".")


def extract_features(df: pd.DataFrame, i: int) -> None:
    """
    Extracts lexical features from the domain names and saves them to a file.

    @param df DataFrame containing the domain names.
    @param i Index of the current iteration to create unique output paths.
    """
    df_features: pd.DataFrame = lexical.lex(df)
    df_features.to_parquet(
        os.path.join(DGA_FEATURES_OUTPUT_PATH, f"{i:02}-DGA-Features.parquet"),
        index=False,
    )


def main() -> None:
    """
    Main function to orchestrate the domain data processing workflow.
    """
    for i in range(NUMBER_OF_ITERATIONS):
        output_file, output_pick_folder = pick_domains(i)
        ngram_file = analyze_ngrams(output_file, output_pick_folder)
        copy_ngrams_to_current_dir(ngram_file)

        df: pd.DataFrame = pd.read_parquet(output_file)
        extract_features(df, i)


if __name__ == "__main__":
    main()