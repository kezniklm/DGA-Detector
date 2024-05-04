"""
 * @file send_dataset.py
 * @brief Domain analysis module for generating DNS responses based on a dataset.
 *
 * This module provides functionalities to generate DNS responses for domain names extracted from a dataset. It loads domain names from a Parquet dataset file, validates and corrects them, and then generates DNS responses with randomly generated IP addresses. The responses are sent using UDP sockets to simulate a DNS server behavior.
 *
 * The main functionalities of this module include:
 * - Loading domain names in batches from a Parquet dataset file.
 * - Validating and correcting domain names according to DNS specifications.
 * - Generating DNS responses for domain names with randomly generated IP addresses.
 * - Sending DNS responses using UDP sockets to simulate DNS server behavior.
 *
 * This tool is useful for testing and simulating DNS server responses in various scenarios, such as load testing or network simulations.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import argparse
import random
import socket
from typing import List

import pandas as pd
import pyarrow.parquet as pq


def load_dataset_in_batches(
    dataset_path: str, batch_size: int = 100000
) -> List[List[str]]:
    """
    Load the dataset in batches.

    Args:
        dataset_path (str): Path to the Parquet dataset file.
        batch_size (int): Batch size for loading the dataset.

    Returns:
        list: List of batches, where each batch contains domain names.
    """
    dataset_file = pq.ParquetFile(dataset_path)
    batches = []
    for batch in dataset_file.iter_batches(batch_size=batch_size):
        df = batch.to_pandas()
        batches.append(df["domain_name"].tolist())
    return batches


def validate_and_correct_domain_name(domain_name: str) -> str:
    """
    Validate and correct the domain name.

    Args:
        domain_name (str): Domain name to be validated and corrected.

    Returns:
        str: Corrected domain name.
    """
    if len(domain_name) > 255:
        raise ValueError("Domain name exceeds the maximum length of 255 characters")

    labels = domain_name.split(".")
    corrected_labels = []
    for label in labels:
        while len(label) > 63:
            corrected_labels.append(label[:63])  # Split label into 63 character chunks
            label = label[63:]
        corrected_labels.append(label)
    return ".".join(corrected_labels)


def generate_dns_response(domain_name: str) -> bytes:
    """
    Generate DNS response for a domain name.

    Args:
        domain_name (str): Domain name for which DNS response is generated.

    Returns:
        bytes: DNS response.
    """
    ip_address = ".".join(str(random.randint(0, 255)) for _ in range(4))
    domain_name_parts = domain_name.split(".")
    dns_question = b""
    for part in domain_name_parts:
        dns_question += len(part).to_bytes(1, "big") + part.encode()
    dns_question += b"\x00"  # Null byte to end the domain name
    dns_question += b"\x00\x01" + b"\x00\x01"  # Type A, Class IN for the question part

    dns_header = bytes.fromhex("00 01 81 80 00 01 00 01 00 00 00 00")

    # Answer section
    dns_answer = (
        dns_question  # Repeat the domain name instead of using a pointer
        + b"\x00\x01"
        + b"\x00\x01"  # Type A, Class IN for the answer part
        + b"\x00\x00\x0e\x10"  # TTL (3600 seconds)
        + b"\x00\x04"  # Data length (4 bytes for IPv4)
        + bytes(map(int, ip_address.split(".")))
    )
    dns_response = dns_header + dns_question + dns_answer
    return dns_response


def send_dns_responses(
    domain_batches: List[List[str]], udp_socket: socket.socket
) -> None:
    """
    Send DNS responses for domain names.

    Args:
        domain_batches (list): List of batches, where each batch contains domain names.
        udp_socket: UDP socket for sending DNS responses.

    Returns:
        None
    """
    count = 0
    for batch in domain_batches:
        for domain_name in batch:
            response = generate_dns_response(domain_name)
            udp_socket.sendto(response, ("localhost", 53))
            count += 1


def parse_arguments() -> argparse.Namespace:
    """
    Parse command line arguments.

    Returns:
        argparse.Namespace: Parsed arguments.
    """
    parser = argparse.ArgumentParser(
        description="Send DNS responses for domain names from a dataset."
    )
    parser.add_argument(
        "--dataset_path",
        type=str,
        help="Path to the Parquet dataset file",
    )
    parser.add_argument(
        "--batch-size",
        type=int,
        default=100000,
        nargs="?",
        help="Batch size for loading the dataset",
    )
    return parser.parse_args()


def main(dataset_path: str, batch_size: int) -> None:
    """
    Main function to send DNS responses for domain names from a dataset.

    Args:
        dataset_path (str): Path to the Parquet dataset file.
        batch_size (int): Batch size for loading the dataset.

    Returns:
        None
    """
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    udp_socket.bind(("localhost", 53))
    domain_batches = load_dataset_in_batches(dataset_path, batch_size)
    send_dns_responses(domain_batches, udp_socket)
    udp_socket.close()


if __name__ == "__main__":
    args = parse_arguments()
    main(args.dataset_path, args.batch_size)







