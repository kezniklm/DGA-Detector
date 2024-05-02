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

3. Install the required C++ libraries using vcpkg and compile the project:
```
$ cmake -B build -S . -DCMAKE_TOOLCHAIN_FILE=<path to vcpkg.cmake>
```

4. Edit the appsettings.json file or use arguments with the Detector binary to configure MongoDB, RabbitMQ, and other necessary details. Ensure your MongoDB instance includes the following collections: Blacklist, Whitelist, and Results for proper operation.

5. Run the Detector block:
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

## Logging

The Detector block utilizes a custom logging mechanism encapsulated in the `Logger` class. This class provides a simple interface for logging messages at various levels of severity and automatically selects the appropriate logging channel based on the operating system. The `Logger` class supports logging to both EventLogChannel on Windows and SyslogChannel on other platforms.

For more details on the `Logger` class, refer to the [Logger.hpp](include/Logger.hpp) file.

## Built With

- [Boost](https://www.boost.org/) - Used for network and system abstraction
- [MongoDB C++ Driver](http://mongocxx.org/) - For interacting with MongoDB
- [rabbitmq-c](https://github.com/alanxz/rabbitmq-c) - RabbitMQ client library
- [CMake](https://cmake.org/) - Build system
- [PcapPlusPlus](https://pcapplusplus.github.io/) - Used for packet capturing and analysis
- [libuv](https://github.com/libuv/libuv) - Cross-platform asynchronous I/O
- [nlohmann/json](https://github.com/nlohmann/json) - JSON library for modern C++
- [cxxopts](https://github.com/jarro2783/cxxopts) - Lightweight C++ command line option parser
- [MPMCQueue](https://github.com/rigtorp/MPMCQueue) - Multi-producer, multi-consumer lock-free queue
- [librabbitmq](https://github.com/alanxz/rabbitmq-c) - RabbitMQ C client library
- [Google Test](https://github.com/google/googletest) - Testing framework for C++
- [POCO](https://pocoproject.org/) - C++ class libraries for network-centric, portable applications

## Author

- **Matej Keznikl** -  [kezniklm](https://github.com/kezniklm)

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](../../LICENSE) file for details.
