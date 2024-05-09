"""
 * @file lightgbm_evaluator.py
 * @brief Concrete implementation of Evaluator for LightGBM binary classification models.
 *
 * This file contains the LightGBMEvaluator class, a concrete implementation of the abstract base class Evaluator,
 * specifically tailored for use with LightGBM binary classification models. It provides functionalities to evaluate
 * input data and return both probability and binary predictions using a pre-trained LightGBM model.
 * This setup allows for seamless integration and flexibility in model evaluations, particularly in binary classification scenarios.
 *
 * The main functionalities included:
 * - Evaluate method to output probabilities and binary predictions.
 * - Constructor for initializing the evaluator with a pre-trained model.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import numpy as np
import pandas as pd
from joblib import load

from ..features.constants import DESIRED_FEATURE_ORDER
from .abstract_evaluator import Evaluator


class LightGBMEvaluator(Evaluator):
    """
    Concrete implementation of the Evaluator class for a LightGBM binary classification model.

    This evaluator uses the LightGBM library to predict outcomes based on a pre-trained model.
    Outputs include the domain name, the probability of the positive class, and the binary prediction.
    """

    def __init__(
        self, model_path: str = "../../training/models/lightgbm_model.joblib"
    ) -> None:
        """
        Initializes the LightGBMEvaluator with a model loaded from the specified path.

        Args:
            model_path (str): Path to the LightGBM model file.
        """
        super().__init__(model_path)
        self.model = load(model_path)

    def evaluate(self, data: pd.DataFrame) -> pd.DataFrame:
        """
        Evaluates the provided data using the pre-loaded LightGBM model.

        This method uses the LightGBM model to predict the probability of the positive class and makes a binary decision.
        It includes the domain name from the input data in the output DataFrame.
        The predictions are returned as a DataFrame containing columns for the domain name, probabilities, and binary predictions.

        Args:
            data (pd.DataFrame): A DataFrame containing the input data for evaluation.

        Returns:
            pd.DataFrame: A DataFrame containing the domain names, probabilities of the positive class, and the binary predictions.
        """
        if "domain_name" not in data.columns:
            raise ValueError("Input DataFrame must include a 'domain_name' column.")

        if (
            data.empty
            or len(data.shape) != 2
            or data.shape[0] == 0
            or data.shape[1] == 0
        ):
            return pd.DataFrame(columns=["domain_name", "predict_prob", "binary_pred"])

        data = data[DESIRED_FEATURE_ORDER]

        data.fillna(-1, inplace=True)

        probabilities = self.model.predict_proba(data.drop(columns=["domain_name"]))

        positive_class_probabilities = probabilities[:, 1] * 100

        positive_class_probabilities = np.round(positive_class_probabilities, 4)

        binary_predictions = positive_class_probabilities >= 50

        return pd.DataFrame(
            {
                "domain_name": data["domain_name"],
                "predict_prob": positive_class_probabilities,
                "binary_pred": binary_predictions,
            }
        )
