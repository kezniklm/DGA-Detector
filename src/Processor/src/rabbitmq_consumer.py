"""
 * @file rabbitmq_consumer.py
 * @brief Handles consuming messages from a RabbitMQ queue and putting them into a message queue.
 *
 * This file contains the implementation of the RabbitMQConsumer class, which is responsible for consuming messages from a RabbitMQ queue and storing them into a message queue. It utilizes the pika library for interacting with RabbitMQ and integrates a custom logging solution for comprehensive message logging.
 *
 * The main functionalities of this file include:
 * - Connecting to RabbitMQ and consuming messages from a specified queue.
 * - Handling retries in case of connection failures or unexpected errors.
 * - Putting consumed messages into a message queue for further processing.
 * - Providing a mechanism for graceful shutdown of the consumer thread.
 * - Logging activities and errors for monitoring and debugging purposes.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import threading
import time
from queue import Queue

import pika

from .logging.logger import Logger


class RabbitMQConsumer(threading.Thread):
    """
    A class representing a RabbitMQ consumer thread.

    This class handles consuming messages from a RabbitMQ queue and putting them into a message queue. It utilizes a custom logger for logging activities and errors.

    Attributes:
        connection_string (str): The connection string for connecting to RabbitMQ.
        queue_name (str): The name of the queue to consume messages from.
        message_queue (queue.Queue): A queue to store consumed messages.
        shutdown_event (threading.Event): An event to signal shutdown.
        retry_delay (int): The delay between retry attempts in seconds.
        max_retries (int): The maximum number of retry attempts.
    """

    def __init__(
        self,
        connection_string: str,
        queue_name: str,
        message_queue: Queue,
        shutdown_event: threading.Event,
        retry_delay: int = 5,
        max_retries: int = 5,
    ):
        """
        Initialize the RabbitMQConsumer.

        Args:
            connection_string (str): The connection string for connecting to RabbitMQ.
            queue_name (str): The name of the queue to consume messages from.
            message_queue (queue.Queue): A queue to store consumed messages.
            shutdown_event (threading.Event): An event to signal shutdown.
            retry_delay (int, optional): The delay between retry attempts in seconds. Defaults to 5.
            max_retries (int, optional): The maximum number of retry attempts. Defaults to 5.
        """
        super().__init__()
        self.connection_string: str = connection_string
        self.queue_name: str = queue_name
        self.message_queue: Queue = message_queue
        self.shutdown_event: threading.Event = shutdown_event
        self.retry_delay: int = retry_delay
        self.max_retries: int = max_retries
        self.daemon: bool = True
        self.logger: Logger = Logger().get_logger()

    def run(self):
        """Start the RabbitMQ consumer thread."""
        attempt = 0
        while not self.shutdown_event.is_set() and attempt <= self.max_retries:
            try:
                self.connect_and_consume()
                break
            except (
                pika.exceptions.AMQPConnectionError,
                pika.exceptions.AMQPChannelError,
            ) as e:
                self.logger.error(f"Connection attempt failed: {e}")
                time.sleep(self.retry_delay * (2**attempt))
                attempt += 1
            except Exception as e:
                self.logger.error(f"Unexpected error: {e}")
                break
        if attempt > self.max_retries:
            self.logger.error("Maximum retry attempts reached. Exiting.")
            self.shutdown_event.set()

    def connect_and_consume(self):
        """Connect to RabbitMQ and start consuming messages."""
        params = pika.URLParameters(self.connection_string)
        connection = pika.BlockingConnection(params)
        channel = connection.channel()
        channel.basic_qos(prefetch_count=1)
        channel.queue_declare(queue=self.queue_name, durable=True)

        def callback(ch, method, properties, body):
            """Callback function for handling consumed messages."""
            if not self.shutdown_event.is_set():
                self.message_queue.put(body)
                self.logger.debug(f"Message received and put into queue: {body}")
                ch.basic_ack(delivery_tag=method.delivery_tag)
            else:
                ch.stop_consuming()

        channel.basic_consume(
            queue=self.queue_name, on_message_callback=callback, auto_ack=False
        )

        try:
            while not self.shutdown_event.is_set():
                channel.start_consuming()
        except pika.exceptions.ConnectionClosedByBroker:
            if not self.shutdown_event.is_set():
                self.logger.warning(
                    "Connection closed by broker, attempting to reconnect..."
                )
                self.connect_and_consume()
        finally:
            if connection.is_open:
                connection.close()
                self.logger.info("RabbitMQ connection closed.")


