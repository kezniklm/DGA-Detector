import json
import queue
import threading
import time

from pandas import DataFrame


class Extractor(threading.Thread):
    def __init__(self, message_queue, shutdown_event):
        super().__init__()
        self.message_queue = message_queue
        self.shutdown_event = shutdown_event
        self.daemon = (
            True  # Ensures that this thread won't prevent the program from exiting
        )
        self.count = 0

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
            df_return_code_3 = df[df["return_code"] == 3]
            df_other_return_codes = df[df["return_code"] != 3]

        except ValueError as e:
            print(f"Error processing message: {e}")
            return

    def lexical_features(df: DataFrame) -> DataFrame:
        pass
