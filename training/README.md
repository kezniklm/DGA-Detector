# DGA-Detector Training

## Overview
The training component of the DGA-Detector system focuses on preparing and optimizing models for Domain Generation Algorithms detection. It consists of several important directories housing scripts, models, and datasets crucial for training and evaluating the DGA detection capabilities.

## Setup Instructions
- **Environment Setup**:
  - It is recommended to use an Anaconda environment to manage the dependencies.
  - Create a new environment using Anaconda: `conda create --name dga-detector python=3.11.7`
  - Activate the environment: `conda activate dga-detector`
  - Install required Python packages: `pip install -r requirements.txt`

## Directory Structure

### `/floor`
- **Purpose**: Contains datasets used for training the DGA-Detector models.
- **Contents**: This folder includes various datasets in `parquet` format ready to be ingested by training scripts to create and validate models.

### `/modules`
- **Purpose**: Includes scripts for feature extraction and dataset manipulation, such as proportional picking.
- **Contents**:
  - `lexical.py`: Handles the extraction of lexical features from domain names.
  - `ngrams.py`: Generates n-gram based features from text data.
  - `proportional_picker.py`: Script for selecting a proportional subset of data, ensuring balanced representation across classes.
  - `__init__.py`: Makes Python treat the directories as containing packages.

### `/models`
- **Purpose**: Stores the best performing models, which are utilized by the main DGA-Detector application.
- **Contents**:
  - `lightgbm_model.joblib` : Trained model using the LightGBM framework, saved in `joblib` format.

## Jupyter Notebooks
- **Usage**: These notebooks are crucial for model selection, parameter tuning, and visualization of the results. They serve as an interactive environment where different models and configurations are tested to identify the best setup.
  - **Key Notebooks**:
    - `DGA-Detector-Best-Classifiers.ipynb`: Focuses on evaluating and selecting the best classifiers.
    - `DGA-Detector-Model-Selection.ipynb`: Explores various model architectures and their performances.
    - `DGA-Detector-Parameter-Tuning.ipynb`: Dedicated to tuning model parameters to optimize performance.
    - 
## Additional Scripts
- **Details**: There are various utility scripts within the repository that assist with tasks such as data preprocessing, analysis, and other ancillary functions. These scripts are not directly involved in model training but support the process.

## Author

- **Matej Keznikl** -  [kezniklm](https://github.com/kezniklm)

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](../LICENSE) file for details.
