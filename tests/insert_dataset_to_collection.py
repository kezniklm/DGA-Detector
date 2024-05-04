"""
 * @file insert_dataset_to_collection.py
 * @brief Insert Parquet data into MongoDB with dynamic datetime generation.
 *
 * This script reads data from a Parquet file, generates a timestamp for each entry, and inserts it into a MongoDB collection. The timestamp is dynamically generated at the time of insertion. The script provides flexibility in specifying MongoDB connection details, database name, collection name, and the path to the Parquet file.
 *
 * The main functionalities of this script include:
 * - Reading data from a Parquet file.
 * - Generating a timestamp for each entry.
 * - Inserting data into a MongoDB collection with timestamps.
 * - Handling command-line arguments for MongoDB connection and file details.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import argparse
from datetime import datetime

import pandas as pd
from pymongo import MongoClient


def parse_arguments():
    """
    Parse command line arguments for MongoDB connection and file details.

    Returns:
        argparse.Namespace: Parsed arguments.
    """
    parser = argparse.ArgumentParser(
        description="Insert Parquet data into MongoDB with dynamic datetime generation"
    )
    parser.add_argument(
        "--mongo_uri",
        type=str,
        default="mongodb://localhost:27017/",
        help="MongoDB URI, e.g., mongodb://localhost:27017/",
    )
    parser.add_argument(
        "--database", type=str, required=True, help="Name of the MongoDB database"
    )
    parser.add_argument(
        "--collection", type=str, required=True, help="Name of the MongoDB collection"
    )
    parser.add_argument(
        "--parquet_file", type=str, required=True, help="Path to the Parquet file"
    )
    return parser.parse_args()


def connect_to_mongodb(uri, db_name, collection_name):
    """
    Establish a connection to the MongoDB database and return the collection.

    Args:
        uri (str): MongoDB connection URI.
        db_name (str): Name of the database.
        collection_name (str): Name of the collection.

    Returns:
        pymongo.collection.Collection: MongoDB collection object.
    """
    client = MongoClient(uri)
    db = client[db_name]
    return db[collection_name]


def read_and_prepare_data(file_path):
    """
    Read a Parquet file, generate 'Added' field, and prepare data for insertion.

    Args:
        file_path (str): Path to the Parquet file.

    Returns:
        pandas.DataFrame: Prepared DataFrame with 'DomainName' and 'Added' columns.

    Raises:
        ValueError: If the required column 'domain_name' is missing in the data.
    """
    df = pd.read_parquet(file_path)

    # Generate the 'Added' datetime field for each row
    df["Added"] = datetime.now()

    # Ensure the column names are correctly handled
    if "domain_name" in df.columns:
        df.rename(columns={"domain_name": "DomainName"}, inplace=True)
        df = df[["DomainName", "Added"]]
    else:
        raise ValueError("The required column 'domain_name' is missing in the data.")

    return df


def insert_data(collection, data):
    """
    Insert data into the specified MongoDB collection.

    Args:
        collection (pymongo.collection.Collection): MongoDB collection object.
        data (list[dict]): List of dictionaries representing data to insert.
    """
    collection.insert_many(data)
    print("Data inserted successfully!")


def main():
    # Parse command line arguments
    args = parse_arguments()

    # Connect to MongoDB
    collection = connect_to_mongodb(args.mongo_uri, args.database, args.collection)

    # Load the Parquet file, prepare the data with an 'Added' timestamp
    df = read_and_prepare_data(args.parquet_file)

    # Convert DataFrame to dictionary format for insertion
    data_dict = df.to_dict("records")

    # Insert data into the collection
    insert_data(collection, data_dict)

if __name__ == "__main__":
    main()
