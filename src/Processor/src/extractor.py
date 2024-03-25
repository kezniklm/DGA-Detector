import json
import queue
import threading
import time
from queue import Queue
from threading import Event

from pandas import DataFrame

from .database.abstract_database import AbstractDatabase
from .database.mongodb_database import MongoDbDatabase
from .features import Features
from .logger import Logger


class Extractor(threading.Thread):
    def __init__(self, message_queue, shutdown_event, database_uri, database_name):
        super().__init__()
        self.message_queue: Queue = message_queue
        self.shutdown_event: Event = shutdown_event
        self.daemon: bool = True
        self.logger: Logger = Logger().get_logger()
        self.features: Features = Features()
        self.database: AbstractDatabase = MongoDbDatabase(
            self.logger, database_uri, database_name
        )

    def run(self):
        while not self.shutdown_event.is_set() or not self.message_queue.empty():
            try:
                message = self.message_queue.get_nowait()
                self.process_message(message)
                self.message_queue.task_done()
            except queue.Empty:
                time.sleep(1)
                continue

    def process_message(self, message):
        # Decode message if it's in bytes
        if isinstance(message, bytes):
            message = message.decode("utf-8")

        try:
            # Load the message as a Python object
            data = json.loads(message)

            # Extract the 'domains' dictionary
            domains_dict = data.get("domains", {})

            # Convert the 'domains' dictionary directly into a DataFrame
            # The keys become the 'Domain_Name' and the values become 'Return_Code'
            df = DataFrame(
                list(domains_dict.items()), columns=["domain_name", "return_code"]
            )

            # Split into two DataFrames based on 'Return_Code'
            df_return_code_3 = df[df["return_code"] == 3].drop(columns=["return_code"])

            df_other_return_codes = df[df["return_code"] != 3].drop(
                columns=["return_code"]
            )

            self.features.get_lexical(df_return_code_3)

            self.features.get_lexical_and_external(df_other_return_codes)

            print(df_return_code_3.head())

            print(df_other_return_codes.head())

            self.database.insert_dataframe(df_return_code_3)

            self.database.insert_dataframe(df_other_return_codes)
        except ValueError as e:
            self.logger.error(f"Error processing message: {e}")
            return

    def lexical_features(df: DataFrame) -> DataFrame:
        pass
