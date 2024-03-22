"""
 * @file mongodb_database.py
 * @brief Handles interactions with a MongoDB database.
 *
 * This file contains the implementation of the MongoDbDatabase class, which provides methods for connecting to a MongoDB database, inserting data from Pandas DataFrame or dictionary, and handling connection retries.
 *
 * The main functionalities of this file include:
 * - Connecting to a MongoDB server with configurable retry mechanism.
 * - Inserting data into a MongoDB collection from Pandas DataFrame or dictionary.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024 
 *
 """

import time

import pandas as pd
from pymongo import MongoClient, errors

from .abstract_database import AbstractDatabase


class MongoDbDatabase(AbstractDatabase):
    """
    A class representing a MongoDB database handler.

    This class provides methods to connect to a MongoDB database, insert data from a Pandas DataFrame or dictionary, and handle connection retries.

    Attributes:
        uri (str): The URI of the MongoDB server.
        db_name (str): The name of the database.
        collection_name (str): The name of the collection within the database.
        max_retries (int): The maximum number of connection retry attempts.
        delay (int): The delay (in seconds) between connection retry attempts.
        client (MongoClient): The MongoDB client instance.
        db (Database): The MongoDB database instance.
        collection (Collection): The MongoDB collection instance.
    """

    def __init__(self, uri, db_name, collection_name, max_retries=3, delay=5) -> None:
        """
        Initialize the MongoDbDatabase instance.

        Args:
            uri (str): The URI of the MongoDB server.
            db_name (str): The name of the database.
            collection_name (str): The name of the collection within the database.
            max_retries (int, optional): The maximum number of connection retry attempts. Defaults to 3.
            delay (int, optional): The delay (in seconds) between connection retry attempts. Defaults to 5.
        """
        self.uri = uri
        self.db_name = db_name
        self.collection_name = collection_name
        self.max_retries = max_retries
        self.delay = delay
        self.client = None
        self.db = None
        self.collection = None

    def connect(self) -> None:
        """
        Connect to the MongoDB server.

        Raises:
            Exception: If the maximum number of connection retry attempts is reached.
        """
        retries = 0
        while retries < self.max_retries:
            try:
                self.client = MongoClient(self.uri, serverSelectionTimeoutMS=5000)
                self.db = self.client[self.db_name]
                self.collection = self.db[self.collection_name]
                self.client.admin.command("ismaster")
                print("MongoDB connected successfully")
                break
            except errors.ServerSelectionTimeoutError as err:
                print(f"Connection attempt {retries + 1} failed: {err}")
                time.sleep(self.delay)
                retries += 1
        if retries == self.max_retries:
            raise Exception("Max retries reached, could not connect to MongoDB")

    def insert_dataframe(self, data) -> None:
        """
        Insert data into the MongoDB collection from a Pandas DataFrame or dictionary.

        Args:
            data (Union[pd.DataFrame, dict]): The data to insert.

        Raises:
            ValueError: If the provided data is not a Pandas DataFrame or a dictionary.
        """
        if isinstance(data, pd.DataFrame):
            documents = data.to_dict("records")
        elif isinstance(data, dict):
            documents = [data]
        else:
            raise ValueError("Data must be a pandas DataFrame or a dictionary")

        try:
            if documents:
                result = self.collection.insert_many(documents)
                print(f"Inserted {len(result.inserted_ids)} documents.")
            else:
                print("No data to insert.")
        except Exception as e:
            print(f"An error occurred while inserting documents: {e}")
