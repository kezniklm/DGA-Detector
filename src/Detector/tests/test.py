import multiprocessing
import random
import socket
import time
import uuid


def generate_dns_response():
    ip_address = '.'.join(str(random.randint(0, 255)) for _ in range(4))

    # Generate a UUID-based domain name
    domain_name = str(uuid.uuid4()).replace('-', '')[:16] + ".example.com"
    domain_name_parts = [part.encode() for part in domain_name.split('.')]
    dns_question = b''.join(bytes([len(part)]) + part for part in domain_name_parts)
    dns_question += b'\x00'  # End of domain name
    dns_question += b'\x00\x01'  # QTYPE: A (IPv4 address)
    dns_question += b'\x00\x01'  # QCLASS: IN (Internet)

    dns_header = bytes.fromhex(
        '00 01' +  # Transaction ID
        '81 80' +  # Flags
        '00 01' +  # Questions
        '00 01' +  # Answer RRs
        '00 00' +  # Authority RRs
        '00 00'  # Additional RRs
    )

    # Increase the size of the answer section to make the packet larger
    dns_answer = b'\xc0\x0c' + b'\x00\x01' + b'\x00\x01' + b'\x00\x00\x0e\x10' + b'\x00\x04' + bytes(
        map(int, ip_address.split('.')))

    # Add additional data to make the packet larger
    additional_data = b'A' * 500  # Adjust the size as needed

    dns_response = dns_header + dns_question + dns_answer + additional_data

    return dns_response


def generate_dns_responses(num_responses, udp_socket):
    start_time = time.time()
    for _ in range(num_responses):
        response = generate_dns_response()
        udp_socket.sendto(response, ('localhost', 53))
    end_time = time.time()
    return end_time - start_time


def main():
    print("Generating DNS responses...")

    num_processes = 16
    num_responses_per_process = 1000000000000000000000  # Adjust this as per your requirement
    total_responses = num_processes * num_responses_per_process

    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    udp_socket.bind(('localhost', 53))

    processes = []
    for _ in range(num_processes):
        process = multiprocessing.Process(target=generate_dns_responses, args=(num_responses_per_process, udp_socket))
        processes.append(process)
        process.start()

    total_time = 0
    for process in processes:
        process.join()
        total_time += process.exitcode

    udp_socket.close()

    generation_speed = total_responses / total_time

    print(f"Total responses generated: {total_responses}")
    print(f"Total time taken: {total_time} seconds")
    print(f"Generation speed: {generation_speed:.2f} responses per second")


if __name__ == "__main__":
    main()
