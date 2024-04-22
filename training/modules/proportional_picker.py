"""
 * @file proportional_picker.py
 * @brief Proportional data picker for domain analysis datasets.
 *
 * This module implements a proportional picker for selecting records from domain analysis datasets based on DGA (Domain Generation Algorithm) statistics. It aims to create balanced training datasets by sampling data from various DGA families proportionally according to their occurrence rates.
 *
 * The main functionalities of this module include:
 * - Extraction of data from zip files containing domain analysis datasets.
 * - Loading and processing of DGA statistics to compute sampling proportions.
 * - Sampling data from individual dataset files based on computed proportions and a threshold for minimum samples.
 * - Warning for missing DGA families in the statistics.
 *
 * This tool is crucial for generating representative training datasets for machine learning models aimed at detecting DGA-generated domain names.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import glob
import os
import random
import zipfile

import pandas as pd


def extract_and_load_data(zip_path: str) -> str:
    """
    Extracts the zip file and returns the extraction directory.

    @param zip_path Path to the zip file.
    @return The path to the directory where the data is extracted.
    """
    dataset_path = zip_path.rsplit(".", 1)[0]
    if not os.path.exists(dataset_path):
        os.makedirs(dataset_path)
    with zipfile.ZipFile(zip_path, "r") as zip_ref:
        zip_ref.extractall(dataset_path)
    return dataset_path


def load_dga_stats(dataset_stats: str, training_samples: int = 230_000) -> pd.DataFrame:
    """
    Loads the DGA statistics and computes the proportions of records to sample.

    @param dataset_stats Path to the dataset statistics file.
    @param training_samples Number of training samples to compute proportions for.
    @return DataFrame containing DGA statistics with computed proportions.
    """
    dga_stats = pd.read_parquet(dataset_stats)
    total_records = dga_stats["Number_of_records"].sum()
    dga_stats["Prop"] = (
        (training_samples / total_records * dga_stats["Number_of_records"])
        .round()
        .astype(int)
    )
    return dga_stats


def sample_data(
    file_path: str, prop: int, file_nrows: int, threshold: int
) -> pd.DataFrame:
    """
    Samples data from a given file based on the proportion and threshold criteria.

    @param file_path Path to the data file.
    @param prop Proportion of data to sample from the file.
    @param file_nrows Number of rows in the file.
    @param threshold Minimum number of samples to pick.
    @return A DataFrame containing the sampled data.
    """
    num_samples = max(prop, threshold)
    random_indices = sorted(random.sample(range(file_nrows), num_samples))
    df = pd.read_parquet(file_path)
    return df[df.index.isin(random_indices)]


def proportional_picker(zip_path: str, dataset_stats: str) -> pd.DataFrame:
    """
    Picks records from datasets proportionally based on DGA statistics.

    @param zip_path Path to the zip file containing the data.
    @param dataset_stats Path to the DGA statistics file.
    @return A DataFrame with proportionally picked data based on DGA family statistics.
    """
    dataset_path = extract_and_load_data(zip_path)
    dga_stats = load_dga_stats(dataset_stats)
    proportions = dict(zip(dga_stats["DGA_Family"], dga_stats["Prop"]))
    out_df = pd.DataFrame()
    threshold = 3

    for file in glob.glob(os.path.join(dataset_path, "*.parquet")):
        dga_family = os.path.basename(file).split(".")[0]
        if dga_family in proportions:
            prop = proportions[dga_family]
            file_nrows = dga_stats.loc[
                dga_stats["DGA_Family"] == dga_family, "Number_of_records"
            ].iloc[0]
            df_filtered = sample_data(file, prop, file_nrows, threshold)
            if not df_filtered.empty:
                tmp_df = df_filtered.iloc[:, 0]
                out_df = pd.concat([out_df, tmp_df], ignore_index=True)
        else:
            print(f"Warning: {dga_family} not found in DGA stats.")

    return out_df
