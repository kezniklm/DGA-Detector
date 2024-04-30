"""
 * @file evaluator_abstract.py
 * @brief Abstract base class for evaluating data using machine learning models.
 *
 * This file contains the abstract base class Evaluator, which defines an interface for evaluator classes.
 * Subclasses of Evaluator are expected to implement specific machine learning model evaluations.
 * This setup allows for flexibility and extensibility in applying different models to evaluate data.
 *
 * The main functionalities outlined in this abstract class include:
 * - An abstract method to evaluate input data and return predictions.
 * - A constructor that can be used by subclasses to initialize model-specific parameters.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

from abc import ABC, abstractmethod

import pandas as pd


class Evaluator(ABC):
    """
    Abstract class for evaluating data using a pre-trained model.

    This class serves as an interface for different types of evaluators that can be implemented
    to use various machine learning models for prediction.
    """

    def __init__(self, model_path: str = "model") -> None:
        """
        Initializes the Evaluator with a model loaded from the specified path.
        Note: Loading of the model is typically done in the implementation classes.

        Args:
            model_path (str): Path to the machine learning model file.
        """
        self.model = None

    @abstractmethod
    def evaluate(self, data: pd.DataFrame) -> pd.DataFrame:
        """
        Evaluates the provided data using the pre-loaded machine learning model.

        This method should be implemented by all subclasses to take a pandas DataFrame as input
        and use the pre-loaded model to predict the outcomes. The method must return the predictions
        as a pandas DataFrame.

        Args:
            data (pd.DataFrame): A DataFrame containing the input data for evaluation.

        Returns:
            pd.DataFrame: A DataFrame containing the predicted values.
        """
        pass


