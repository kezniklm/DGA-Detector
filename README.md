# DGA-Detector

## Overview

DGA-Detector is a comprehensive solution designed to detect and classify Domain Generation Algorithm (DGA) generated domains. It comprises three key components:

1. **Detector**: A C++ network analyzer and filter engineered to identify patterns indicative of DGA-generated domains.
2. **Processor**: A Python-based module responsible for feature extraction, classification, and storing results into a MongoDB database.
3. **Web Application**: An ASP.NET API combined with a React JS frontend for convenient interaction and visualization of the detection results.

Each component plays a vital role in the detection and analysis process, contributing to the overall effectiveness of the DGA-Detector.

## Components

### 1. Detector

The Detector is implemented in C++ and serves as the core mechanism for analyzing network traffic. It is responsible for identifying suspicious patterns that may indicate the presence of DGA-generated domains. Detailed instructions and source code for the Detector can be found in the `src/Detector` directory.

### 2. Processor

The Processor module, developed in Python, handles the subsequent processing steps after detection. It extracts features from the identified domains, classifies them using machine learning techniques, and stores the results into a MongoDB database for further analysis. Explore the `src/Processor` directory for detailed implementation and usage instructions.

### 3. Web Application

The Web Application provides a user-friendly interface for interacting with the detection results. Built with ASP.NET API for the backend and React JS for the frontend, it offers convenient visualization and management of detected DGA-generated domains. Navigate to the `src/Web application` directory for comprehensive guidelines on setting up and utilizing the Web Application.

## Additional Components
### Training
Within the `src/training` directory, you will find resources dedicated to machine learning model selection, tuning, and evaluation. These files are essential for choosing the best classifiers for accurately detecting DGA-generated domains.

### Tests
The `src/tests` directory contains tests that cover the entire DGA-Detector system. These tests ensure the reliability and effectiveness of all components, from the Detector to the Web Application.

## Usage

1. **Clone Repository**: Begin by cloning the DGA-Detector repository to your local machine.
2. **Setup Detector**: Follow the instructions in `src/Detector/README.md` to set up and configure the Detector component.
3. **Configure Processor**: Refer to `src/Processor/README.md` for guidance on configuring and utilizing the Processor module.
4. **Deploy Web Application**: Explore `src/Web application/README.md` for detailed steps on deploying the Web Application, including setting up the ASP.NET API and integrating the React JS frontend.

## Author

- **Matej Keznikl** -  [kezniklm](https://github.com/kezniklm)

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.
