import queue
import threading
import time

from src.arguments import Arguments
from src.extractor import Extractor
from src.rabbitmq_consumer import RabbitMQConsumer


class Processor:
    def __init__(self):
        args = Arguments().parse_args()
        self.shutdown_event = threading.Event()
        self.message_queue = queue.Queue(5)
        self.extractors = [
            Extractor(self.message_queue, self.shutdown_event)
            for _ in range(args.threads)
        ]
        self.consumer = RabbitMQConsumer(
            args.rabbitmq, args.queue, self.message_queue, self.shutdown_event
        )

    def run(self):
        for extractor in self.extractors:
            extractor.start()
        self.consumer.start()

        print("Application is running. Press CTRL+C to exit.\n")
        try:
            while not self.shutdown_event.is_set():
                time.sleep(10)
        except KeyboardInterrupt:
            print("Interrupted by user, initiating shutdown...\n")
            self.shutdown_event.set()

        for extractor in self.extractors:
            extractor.join()
        print("Processor has exited.")


if __name__ == "__main__":
    processor = Processor()
    processor.run()
