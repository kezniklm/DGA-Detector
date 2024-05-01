import random
import socket

import pandas as pd
import pyarrow.parquet as pq


def load_dataset_in_batches(dataset_path, batch_size=100000):
    dataset_file = pq.ParquetFile(dataset_path)
    batches = []
    for batch in dataset_file.iter_batches(batch_size=batch_size):
        df = batch.to_pandas()
        batches.append(df["domain_name"].tolist())
    print(len(batches))
    return batches


def validate_and_correct_domain_name(domain_name):
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


def generate_dns_response(domain_name):
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


def send_dns_responses(domain_batches, udp_socket):
    count = 0
    for batch in domain_batches:
        for domain_name in batch:
            response = generate_dns_response(domain_name)
            udp_socket.sendto(response, ("localhost", 53))
            count += 1
    print(count)


def main():
    dataset_path = "combined.parquet"
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    udp_socket.bind(("localhost", 53))
    domain_batches = load_dataset_in_batches(dataset_path)
    send_dns_responses(domain_batches, udp_socket)
    udp_socket.close()


if __name__ == "__main__":
    main()

