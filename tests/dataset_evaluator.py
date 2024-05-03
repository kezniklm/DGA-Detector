"""
 * @file domain_evaluator.py
 * @brief Domain analysis module using MongoDB and confusion matrix calculations.
 *
 * This module is designed to process domain names by querying them against entries in a MongoDB database and evaluating their safety status based on predefined criteria. The results are then used to compute a confusion matrix, providing insights into the effectiveness of the domain classification process. The script is scalable and modular, accommodating large datasets through batch processing.
 *
 * The main functionalities of this module include:
 * - Querying domain names in a MongoDB collection.
 * - Batch processing to handle large datasets efficiently.
 * - Evaluation of domain names based on database entries to classify them as dangerous or safe.
 * - Calculation of a confusion matrix to assess the classification accuracy.
 *
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import pandas as pd
from pymongo import MongoClient
from sklearn.metrics import confusion_matrix


def run_analysis(dataset_path, db_name, collection_name, batch_size=200000):
    """
    Perform a domain analysis by comparing a list of domain names from a dataset
    against records in a MongoDB collection, and calculate the confusion matrix
    for predictions based on database entries.

    @param dataset_path: Path to the parquet file containing the dataset.
    @param db_name: Name of the MongoDB database to use.
    @param collection_name: Name of the MongoDB collection to query.
    @param batch_size: Number of records to process in each batch (default is 200,000).

    @return None: Outputs the confusion matrix directly to the console.
    """
    try:
        data = pd.read_parquet(dataset_path)
    except Exception as e:
        print(f"Failed to load dataset: {e}")
        return

    try:
        client = MongoClient("mongodb://localhost:27017/")
        db = client[db_name]
        collection = db[collection_name]
    except Exception as e:
        print(f"Failed to connect to MongoDB: {e}")
        return

    def process_batch(domains):
        """
        Query the MongoDB collection for a batch of domain names and store the results
        in a dictionary keyed by domain name.

        @param domains: List of domain names to query.
        @return: Dictionary of query results keyed by domain name.
        """
        try:
            query_results = {
                item["DomainName"]: item
                for item in collection.find({"DomainName": {"$in": domains}})
            }
            return query_results
        except Exception as e:
            print(f"Database query failed: {e}")
            return {}

    predictions = []
    true_labels = []

    for i in range(0, len(data), batch_size):
        batch = data.iloc[i : i + batch_size]
        domain_names = batch["domain_name"].tolist()

        batch_results = process_batch(domain_names)

        for _, row in batch.iterrows():
            domain_name = row["domain_name"]
            label = row["label"]
            true_labels.append(label)

            query_result = batch_results.get(domain_name)

            predicted_label = (
                1
                if query_result and query_result.get("DangerousBoolValue", False)
                else 0
            )
            predictions.append(predicted_label)

    valid_predictions = [p for p in predictions if p is not None]
    conf_matrix = confusion_matrix(
        true_labels[: len(valid_predictions)], valid_predictions, labels=[1, 0]
    )

    print("Confusion Matrix:")
    print(conf_matrix)


run_analysis(
    "00-Dataset-test-without-Blacklist-Whitelist/00-Dataset-not-in-Blacklist-Whitelist.parquet",
    "Database",
    "Result",
)
