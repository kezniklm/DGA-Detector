import signal
from multiprocessing import Event, Process

from src.arguments import Arguments
from src.rabbitmq_consumer import consumer_process as run_consumer_process
from src.return_codes import ReturnCodes
from src.string_queue import StringQueue

# Initialize the termination event for signaling between processes
terminate_event = Event()


def setup_signal_handlers():
    """
    Configures signal handlers for gracefully shutting down the process.
    """

    def signal_handler(sig, frame):
        """
        Signal handler that sets the termination event.
        """
        terminate_event.set()

    # Register the signal handler for SIGINT and SIGTERM
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)


def main():
    """
    Main function to start the consumer process.
    """
    setup_signal_handlers()

    # Parse command line arguments
    arg_parser = Arguments()
    args = arg_parser.parse_args()

    # Extract RabbitMQ connection details from arguments
    rabbitmq_connection_string = args.rabbitmq
    rabbitmq_queue_name = args.queue

    # Initialize the queue with a max size
    queue = StringQueue(10)

    # Create and start the consumer process
    consumer_process = Process(
        target=run_consumer_process,
        args=(rabbitmq_connection_string, rabbitmq_queue_name, queue, terminate_event),
    )
    consumer_process.start()

    # Wait for the consumer process to complete
    consumer_process.join()

    # Exit the main process successfully
    exit(ReturnCodes.SUCCESS.value)


if __name__ == "__main__":
    main()
