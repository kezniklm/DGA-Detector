import threading
import time

import pika


class RabbitMQConsumer(threading.Thread):
    def __init__(
        self,
        connection_string,
        queue_name,
        message_queue,
        shutdown_event,
        retry_delay=5,
        max_retries=5,
    ):
        super().__init__()
        self.connection_string = connection_string
        self.queue_name = queue_name
        self.message_queue = message_queue
        self.shutdown_event = shutdown_event
        self.retry_delay = retry_delay
        self.max_retries = max_retries
        self.daemon = True

    def run(self):
        attempt = 0
        while not self.shutdown_event.is_set() and attempt <= self.max_retries:
            try:
                self.connect_and_consume()
                break  # If successful, exit the loop
            except (
                pika.exceptions.AMQPConnectionError,
                pika.exceptions.AMQPChannelError,
            ) as e:
                print(f"Connection attempt failed: {e}")
                time.sleep(self.retry_delay * (2**attempt))  # Exponential backoff
                attempt += 1
            except Exception as e:
                print(f"Unexpected error: {e}")
                break  # Exit loop on unexpected errors
        if attempt > self.max_retries:
            print("Maximum retry attempts reached. Exiting.")
            self.shutdown_event.set()  # Correctly initiate connection and start consuming messages

    def connect_and_consume(self):
        params = pika.URLParameters(self.connection_string)
        connection = pika.BlockingConnection(params)
        channel = connection.channel()
        channel.queue_declare(queue=self.queue_name, durable=True)

        def callback(ch, method, properties, body):
            if not self.shutdown_event.is_set():
                self.message_queue.put(body)
            else:
                ch.stop_consuming()

        channel.basic_consume(
            queue=self.queue_name, on_message_callback=callback, auto_ack=True
        )

        try:
            while not self.shutdown_event.is_set():
                channel.start_consuming()
        except pika.exceptions.ConnectionClosedByBroker:
            if not self.shutdown_event.is_set():
                self.connect_and_consume()  # Attempt to reconnect and resume if disconnected
        finally:
            connection.close()
