import json
import queue
import threading
import time
import warnings
from queue import Queue
from threading import Event

from pandas import DataFrame

from .database.abstract_database import AbstractDatabase
from .database.mongodb_database import MongoDbDatabase
from .features.features import FeatureExtractor
from .logging.logger import Logger

warnings.filterwarnings(
    "ignore", category=FutureWarning, module="numpy.core.fromnumeric"
)


class Extractor(threading.Thread):
    def __init__(
        self,
        message_queue,
        shutdown_event,
        database_uri,
        database_name,
        number_of_processes,
    ):
        super().__init__()
        self.message_queue: Queue = message_queue
        self.shutdown_event: Event = shutdown_event
        self.daemon: bool = True
        self.logger: Logger = Logger().get_logger()
        self.feature_extractor: FeatureExtractor = FeatureExtractor(number_of_processes)
        self.database: AbstractDatabase = MongoDbDatabase(
            self.logger, database_uri, database_name
        )

    def run(self):
        self.database.connect()
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

            # Start timing feature extraction
            start_time = time.time()

            df = self.feature_extractor.extract_features(df)

            # End timing and calculate elapsed time
            elapsed_time = time.time() - start_time
            print(f"Feature extraction took {elapsed_time:.2f} seconds.")

            print(df.head())

            self.database.insert_dataframe(df)

        except ValueError as e:
            self.logger.error(f"Error processing message: {e}")
            return
