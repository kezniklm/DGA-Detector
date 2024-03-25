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
from typing import Union

import pandas as pd
from pymongo import MongoClient, errors
from pymongo.collection import Collection
from pymongo.database import Database

from ..logger import Logger
from .abstract_database import AbstractDatabase


class MongoDbDatabase(AbstractDatabase):
    """
    A class representing a MongoDB database handler.

    This class provides methods to connect to a MongoDB database, insert data from a Pandas DataFrame or dictionary, and handle connection retries.
    A logger is used to log information, warnings, and errors during the database connection and data insertion processes. This facilitates debugging and monitoring the operation of the database handler.

    Attributes:
        uri (str): The URI of the MongoDB server.
        db_name (str): The name of the database.
        collection_name (str): The name of the collection within the database.
        max_retries (int): The maximum number of connection retry attempts.
        delay (int): The delay (in seconds) between connection retry attempts.
        client (MongoClient): The MongoDB client instance.
        db (Database): The MongoDB database instance.
        collection (Collection): The MongoDB collection instance.
        logger: The logger instance for logging messages.
    """

    def __init__(
        self,
        logger: Logger,
        uri: str,
        db_name: str,
        collection_name: str = "Results",
        max_retries: int = 3,
        delay: int = 5,
    ) -> None:
        """
        Initialize the MongoDbDatabase instance.

        Args:
            uri (str): The URI of the MongoDB server.
            db_name (str): The name of the database.
            collection_name (str, optional): The name of the collection within the database. Defaults to Results.
            max_retries (int, optional): The maximum number of connection retry attempts. Defaults to 3.
            delay (int, optional): The delay (in seconds) between connection retry attempts. Defaults to 5.
            logger: The logger instance for logging operation messages.
        """
        self.logger: Logger = logger
        self.uri: str = uri
        self.db_name: str = db_name
        self.collection_name: str = collection_name
        self.max_retries: int = max_retries
        self.delay: int = delay
        self.client: MongoClient = None
        self.db: Database = None
        self.collection: Collection = None

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
                self.logger.info("MongoDB connected successfully")
                break
            except errors.ServerSelectionTimeoutError as err:
                self.logger.error(f"Connection attempt {retries + 1} failed: {err}")
                time.sleep(self.delay)
                retries += 1
        if retries == self.max_retries:
            raise Exception("Max retries reached, could not connect to MongoDB")

    def _ensure_connected(self) -> None:
        """Checks if the database connection is established and raises an error if not."""
        if self.client is None or self.db is None or self.collection is None:
            raise RuntimeError("Database connection is not established.")

    def insert_dataframe(self, data: Union[pd.DataFrame, dict]) -> None:
        """
        Insert data into the MongoDB collection from a Pandas DataFrame or dictionary.

        Args:
            data (Union[pd.DataFrame, dict]): The data to insert.

        Raises:
            ValueError: If the provided data is not a Pandas DataFrame or a dictionary.
        """

        self._ensure_connected()

        if isinstance(data, pd.DataFrame):
            documents = data.to_dict("records")
        elif isinstance(data, dict):
            documents = [data]
        else:
            raise ValueError("Data must be a pandas DataFrame or a dictionary")

        try:
            if documents:
                result = self.collection.insert_many(documents)
                self.logger.info(f"Inserted {len(result.inserted_ids)} documents.")
            else:
                self.logger.info("No data to insert.")
        except Exception as e:
            self.logger.error(f"An error occurred while inserting documents: {e}")
