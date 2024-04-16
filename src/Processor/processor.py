"""
 * @file processor.py
 * @brief Handles message processing for the application.
 *
 * This file contains the implementation of the Processor class, which is responsible for managing message processing within the application. It initializes Extractors and RabbitMQConsumer, starts them, and provides a mechanism for graceful shutdown.
 *
 * The main functionalities of this file include:
 * - Initializing Extractors and RabbitMQConsumer for message processing.
 * - Starting Extractors and RabbitMQConsumer threads.
 * - Entering a main processing loop until a shutdown event occurs.
 * - Handling graceful shutdown upon user interruption (CTRL+C).
 * - Joining Extractors threads upon shutdown.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import queue
import threading
import time

from src.extractor import Extractor
from src.logging.logger import Logger
from src.rabbitmq_consumer import RabbitMQConsumer
from src.utils.arguments import Arguments


class Processor:
    """
    A class representing the main processing unit of the application.

    This class initializes Extractors and RabbitMQConsumer to process messages.

    Attributes:
        shutdown_event (threading.Event): A threading event to signal shutdown.
        message_queue (queue.Queue): A queue to store incoming messages.
        extractor (Extractor): A Extractor instance for processing messages.
        consumer (RabbitMQConsumer): A RabbitMQConsumer instance for consuming messages.
        logger (Logger): A logger instance for logging events.
    """

    def __init__(self) -> None:
        """
        Initialize the Processor class.

        Initializes Extractors, RabbitMQConsumer, Logger, and other attributes.
        """
        self.logger: Logger = Logger().get_logger()
        args: Arguments = Arguments(self.logger).parse_args()
        self.shutdown_event: threading.Event = threading.Event()
        self.message_queue: queue.Queue = queue.Queue(args.queue_size)
        self.extractor: Extractor = Extractor(
            self.message_queue,
            self.shutdown_event,
            args.database,
            args.dbname,
            args.processes,
        )
        self.consumer: RabbitMQConsumer = RabbitMQConsumer(
            args.rabbitmq, args.queue, self.message_queue, self.shutdown_event
        )

    def run(self) -> None:
        """
        Start the processing loop.

        Starts Extractor and RabbitMQConsumer, and enters the main processing loop.
        """
        self.extractor.start()
        self.consumer.start()

        self.logger.info("Application is running. Press CTRL+C to exit.")
        try:
            while not self.shutdown_event.is_set():
                time.sleep(10)
        except KeyboardInterrupt:
            self.logger.info("Interrupted by user, initiating shutdown...")
            self.shutdown_event.set()

        self.logger.info("Processor has exited.")


if __name__ == "__main__":
    processor = Processor()
    processor.run()
