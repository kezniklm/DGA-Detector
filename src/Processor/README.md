# Processor Block

The Processor block of the DGA-Detector is responsible for consuming domain information from RabbitMQ, extracting features from these domains, classifying them using a LightGBM model, and storing the classification results into MongoDB.

## Prerequisites

Before running the Processor, ensure you have the following installed:
- Python 3.11.7+
- RabbitMQ
- MongoDB
- All Python dependencies listed in `requirements.txt`

## Setup

1. **Clone the repository** (if not already done):
   ```
   git clone https://github.com/kezniklm/DGA-Detector.git
   cd DGA-Detector/src/Processor
   ```
2. **Create and activate a virtual environment**:
   ```
   python -m venv venv
   source venv/bin/activate (on Windows use venv\Scripts\activate)
   ```

3. **Install Python dependencies**:
   ```
   pip install -r requirements.txt
   ```

## Configuration

You can configure the Processor using the `appsettings.json` file or command line arguments. Ensure the settings for your RabbitMQ server and MongoDB instance are correct.

- **Using appsettings.json**: Modify the `appsettings.json` file to include the necessary RabbitMQ and MongoDB connection details and other arguments.
- **Using command line arguments**: You can override the settings in `appsettings.json` by providing command line arguments when launching the processor.

## Running the Processor

To launch the Processor, use the following command from the `src` directory:

```
python processor.py
```

This script initializes the consumption of domain data from RabbitMQ, processes the data through the feature extraction and LightGBM classification, and finally stores the results in MongoDB.

## Files and Directories

- `data/`: Contains JSON files with n-gram frequencies used for feature extraction.
- `src/`: Source code including the main processor, feature extraction, database integration, and more.
- `tests/`: Directory for test scripts (currently empty but intended for unit tests).

## Author

- **Matej Keznikl** -  [kezniklm](https://github.com/kezniklm)

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](../../LICENSE) file for details.
