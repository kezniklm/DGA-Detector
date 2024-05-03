"""
 * @file extractor.py
 * @brief Manages the extraction of features from messages and interaction with databases in a multithreaded environment.
 *
 * This file contains the implementation of the Extractor class, which extends from threading.Thread. The Extractor is designed to process messages from a message queue, perform feature extraction, evaluate results, and save them into a database. It uses various components such as feature extractors, evaluators, and database interfaces to handle different aspects of data processing efficiently.
 *
 * The main functionalities of this file include:
 * - Processing messages continuously from a shared queue until a shutdown signal is received.
 * - Decoding messages, extracting relevant data, and converting them into a DataFrame.
 * - Utilizing a feature extractor to add computed features to the data.
 * - Evaluating the processed data using a predefined set of rules or models.
 * - Inserting results into the appropriate database collections based on processing outcomes.
 * - Handling exceptions and logging errors throughout the message processing flow.
 * - Providing mechanisms for a clean and safe shutdown of the processing thread.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import os

os.environ["TF_CPP_MIN_LOG_LEVEL"] = "2"

import json
import logging
import queue
import threading
import time
import warnings
from queue import Queue
from threading import Event

import tensorflow as tf
from pandas import DataFrame

from .database.abstract_database import AbstractDatabase
from .database.mongodb_database import MongoDbDatabase
from .evaluator.abstract_evaluator import Evaluator
from .evaluator.lightgbm_evaluator import LightGBMEvaluator
from .features.feature_extractor import FeatureExtractor
from .logging.logger import Logger

warnings.filterwarnings(
    "ignore", category=FutureWarning, module="numpy.core.fromnumeric"
)


class Extractor(threading.Thread):
    """Class for processing messages and extracting features in a multithreaded environment.

    Extends the threading.Thread class to process messages from a queue, extract features,
    evaluate them, and interact with a database for storage.

    Attributes:
        message_queue (Queue): Queue from which messages are fetched for processing.
        shutdown_event (Event): Event that signals the thread to shutdown.
        daemon (bool): Flag to set this thread as a daemon.
        logger (Logger): Logger instance for logging information.
        feature_extractor (FeatureExtractor): Instance to handle feature extraction.
        database (AbstractDatabase): Database instance for storing processed data.
        evaluator (Evaluator): Evaluator instance to perform data evaluations.
    """

    def __init__(
        self,
        message_queue,
        shutdown_event,
        database_uri,
        database_name,
        number_of_processes,
    ) -> None:
        """Initialize the Extractor thread with the necessary components.

        Args:
            message_queue (Queue): Queue from which messages are to be processed.
            shutdown_event (Event): Event to signal the thread to stop running.
            database_uri (str): URI for the database connection.
            database_name (str): Name of the database to connect to.
            number_of_processes (int): Number of processes for parallel feature extraction.
        """
        super().__init__()
        self.message_queue: Queue = message_queue
        self.shutdown_event: Event = shutdown_event
        self.daemon: bool = True
        self.logger: Logger = Logger().get_logger()
        self.feature_extractor: FeatureExtractor = FeatureExtractor(number_of_processes)
        self.database: AbstractDatabase = MongoDbDatabase(
            self.logger, database_uri, database_name
        )
        self.lightgbm_evaluator: Evaluator = LightGBMEvaluator()

    def run(self) -> None:
        """Main execution point for the thread, handling message processing in a loop."""
        self.database.connect()

        while not self.shutdown_event.is_set() or not self.message_queue.empty():
            try:
                message = self.message_queue.get()
                self.process_message(message)
            except queue.Empty:
                time.sleep(1)
                continue

    def process_message(self, message) -> None:
        """Process a single message, extract features, and store results in the database.

        Args:
            message (str or bytes): Message to be processed, may be in string or bytes format.

        Notes:
            Messages are assumed to be JSON strings that can be decoded and contain 'domains'
            data for processing. The function handles decoding, data extraction, feature
            extraction, evaluation, and database storage.
        """
        if isinstance(message, bytes):
            message = message.decode("utf-8")

        try:
            data = json.loads(message)

            domains_dict = data.get("domains", {})

            df = DataFrame(
                list(domains_dict.items()), columns=["domain_name", "return_code"]
            )

            df = self.feature_extractor.extract_features(df)

            df_return_code_3 = df[df["return_code"] == 3].drop(columns=["return_code"])

            df_other_return_codes = df[df["return_code"] != 3].drop(
                columns=["return_code"]
            )

            df_return_code_3 = self.lightgbm_evaluator.evaluate(df_return_code_3)
            df_other_return_codes = self.lightgbm_evaluator.evaluate(
                df_other_return_codes
            )

            self.database.insert_dataframe(df_return_code_3)

            self.database.insert_dataframe(df_other_return_codes)

        except ValueError as e:
            self.logger.error(f"Error processing message: {e}")
            return



















