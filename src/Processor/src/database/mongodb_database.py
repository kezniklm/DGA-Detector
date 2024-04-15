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
from datetime import datetime
from typing import Union

import pandas as pd
from pymongo import MongoClient, errors
from pymongo.collection import Collection
from pymongo.database import Database

from ..logging.logger import Logger
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
        collection_name: str = "Result",
        max_retries: int = 3,
        delay: int = 5,
    ) -> None:
        """
        Initialize the MongoDbDatabase instance.

        Args:
            uri (str): The URI of the MongoDB server.
            db_name (str): The name of the database.
            collection_name (str, optional): The name of the collection within the database. Defaults to Result.
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

    def __del__(self) -> None:
        """
        Destructor method to ensure that the MongoDB connection is properly closed when the object is destroyed.
        """
        self.close()

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

    def _transform_data(self, data: Union[pd.DataFrame, dict]) -> list:
        """
        Transforms the input data into a list of documents matching the structure of ResultEntity.

        Args:
            data (Union[pd.DataFrame, dict]): The data to transform.

        Returns:
            list: A list of dictionaries formatted according to the ResultEntity structure.
        """

        if isinstance(data, pd.DataFrame):
            documents = data.to_dict("records")
        elif isinstance(data, dict):
            documents = [data]
        else:
            raise ValueError("Data must be a pandas DataFrame or a dictionary")

        # Transform each document to match the Result structure
        transformed_documents = []
        for doc in documents:
            transformed_doc = {
                "Detected": doc.get("Detected", datetime.now()),
                "DidBlacklistHit": doc.get("DidBlacklistHit", False),
                "DangerousProbabilityValue": doc.get("DangerousProbabilityValue", 0),
                "DangerousBoolValue": doc.get("DangerousBoolValue", False),
                "DomainName": doc["domain_name"],
            }
            transformed_documents.append(transformed_doc)

        return transformed_documents

    def handle_connection_failure(func):
        """Decorator to handle reconnection on connection failures."""

        def wrapper(*args, **kwargs):
            self = args[0]
            try:
                return func(*args, **kwargs)
            except (errors.ConnectionFailure, errors.NetworkTimeout) as e:
                self.logger.error(
                    f"Operation failed due to connection issue: {e}, attempting to reconnect..."
                )
                self.connect()
                return func(*args, **kwargs)

        return wrapper

    @handle_connection_failure
    def insert_dataframe(
        self, data: Union[pd.DataFrame, dict], batch_size: int = 1000
    ) -> None:
        """
        Inserts transformed data into the MongoDB collection from a Pandas DataFrame or dictionary, in batches.
        Wrapped with automatic reconnection.
        """
        self._ensure_connected()

        transformed_documents = self._transform_data(data)
        total_documents = len(transformed_documents)
        inserted_count = 0

        for start_idx in range(0, total_documents, batch_size):
            end_idx = start_idx + batch_size
            batch = transformed_documents[start_idx:end_idx]
            try:
                if batch:
                    result = self.collection.insert_many(batch, ordered=False)
                    inserted_count += len(result.inserted_ids)
            except Exception as e:
                self.logger.error(
                    f"An error occurred while inserting batch {start_idx // batch_size + 1}: {e}"
                )

        if inserted_count:
            self.logger.info(f"Total inserted documents: {inserted_count}")
        else:
            self.logger.info("No data was inserted.")

    def close(self) -> None:
        """
        Explicitly closes the MongoDB client connection.
        """
        if self.client:
            try:
                self.client.close()
                self.logger.info("MongoDB connection closed successfully.")
            except Exception as e:
                self.logger.error(f"Failed to close MongoDB connection: {e}")
            finally:
                self.client = None
                self.db = None
                self.collection = None
