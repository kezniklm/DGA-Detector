"""
 * @file lexical_feature_template.py
 * @brief Defines a lexical feature template for data processing.
 *
 * This file contains the implementation of the LexicalFeatureTemplate class, which is responsible for defining specific lexical features for data processing. It inherits from FeatureTemplateBase and provides specific lexical columns with their data types.
 *
 * The main functionalities of this file include:
 * - Defining lexical columns with their corresponding data types.
 * - Inheriting from FeatureTemplateBase for common feature template functionality.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
 """

from typing import Dict

from .feature_template_base import FeatureTemplateBase


class LexicalFeatureTemplate(FeatureTemplateBase):
    """
    A class representing a lexical feature template.

    This class inherits from FeatureTemplateBase and defines specific lexical features.

    Attributes:
        lexical_columns (dict): A dictionary mapping feature names to their data types.
    """

    def __init__(self) -> None:
        """
        Initialize the LexicalFeatureTemplate.

        Initializes specific lexical columns with their data types.
        """
        lexical_columns: Dict[str, str] = {
            "string_length": "Int16",
            "subdomain_count": "Int16",
            "longest_digit_sequence": "Int16",
            "longest_consonant_sequence": "Int16",
            "longest_vowel_sequence": "Int16",
            "hexadecimal_subdomain_count": "Int16",
            "average_domain_length": "Int16",
            "numeric_character_ratio": "float32",
            "consonant_ratio": "float32",
            "vowel_ratio": "float32",
            "shannon_entropy": "float32",
            "ip_address_flag": "bool",
        }
        super().__init__(lexical_columns)
