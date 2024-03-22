"""
 * @file feature_template_base.py
 * @brief Defines the FeatureTemplateBase class for adding columns to a DataFrame.
 *
 * This file contains the implementation of the FeatureTemplateBase class, which serves as a base class for creating feature templates to add columns to a DataFrame. It provides functionality to add columns based on specified column names and data types using pandas.
 *
 * The main functionalities of this file include:
 * - Defining the FeatureTemplateBase class.
 * - Initializing a FeatureTemplateBase instance with column names and data types.
 * - Adding columns to a DataFrame according to specified column names and data types.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import pandas as pd


class FeatureTemplateBase:
    """
    @brief Base class for creating feature templates to add columns to a DataFrame.

    This class provides a template for adding columns to a DataFrame based on specified column names and data types.
    """

    def __init__(self, columns_with_types):
        """
        @brief Initializes a FeatureTemplateBase instance.

        @param columns_with_types A dictionary containing column names as keys and data types as values.
        """
        self.columns_with_types = columns_with_types

    def add_columns(self, dataframe):
        """
        @brief Adds columns to the DataFrame according to the specified column names and data types.

        This method iterates over the specified column names and data types, and adds corresponding columns to the DataFrame.

        @param dataframe The DataFrame to which columns will be added.
        @return The DataFrame with added columns.
        """
        for col, dtype in self.columns_with_types.items():
            dataframe[col] = pd.Series(dtype=dtype)
        return dataframe
