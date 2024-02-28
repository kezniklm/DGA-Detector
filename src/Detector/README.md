# Detector Block

The Detector block is a sophisticated network analysis tool designed for monitoring DNS packets across the network. It specializes in filtering these packets based on their response codes. Utilizing both a blacklist and whitelist stored in MongoDB, it efficiently identifies and manages domain names according to predefined criteria. Upon detection, relevant data is forwarded to a RabbitMQ queue for further processing or alerting.

## Getting Started

This guide will help you set up the Detector block on your local machine.

### Prerequisites
- A C++ compiler and build tools (e.g., CMake, Make)
- vcpkg package manager installed

### Installing

Follow these steps to get your development environment running:

1. Clone the Detector block repository:

```
$ git clone https://github.com/kezniklm/DGA-Detector
```

2. Navigate to the project directory:

```
$ cd DGA-Detector/src/Detector
```

3. Install the required C++ libraries using vcpkg:

```
$ ./vcpkg install --overlay-ports=vcpkg_ports
```

4. Compile the project:
```
$ mkdir build && cd build
$ cmake ..
$ make
```

5. Configure MongoDB, RabbitMQ and other needed details in the configuration file appsettings.json or by using arguments of Detector binary.

6. Run the Detector block:
```
$ sudo ./Detector <arguments>
```

## Usage

After installation, the Detector block will start analyzing network traffic for DNS packets. Detected packets are filtered and processed according to the rules defined in the MongoDB databases for blacklisting and whitelisting domain names. Matched entries are then sent to a RabbitMQ queue for further action.

## Running the Tests

Ensure your setup is correctly configured by running included tests:

```
$ cd build
$ make test
```

## Deployment

To deploy the Detector block on a live system, ensure all dependencies are met, and the system has access to the network interface for capturing DNS packets. Use the provided systemd service file or similar for daemonizing the application.

## Built With

- [Boost](https://www.boost.org/) - Used for network and system abstraction
- [MongoDB C++ Driver](http://mongocxx.org/) - For interacting with MongoDB
- [rabbitmq-c](https://github.com/alanxz/rabbitmq-c) - RabbitMQ client library
- [CMake](https://cmake.org/) - Build system

## Author

- **Matej Keznikl** -  [kezniklm](https://github.com/kezniklm)

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](../../LICENSE) file for details.