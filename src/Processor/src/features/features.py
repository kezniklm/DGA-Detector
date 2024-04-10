from typing import List

import pandas as pd

from .feature_templates.lexical_feature_template import LexicalFeatureTemplate


class Features:
    def __init__(self):
        self.lexical_template = LexicalFeatureTemplate()

    def get_lexical(self, df: pd.DataFrame) -> pd.DataFrame:
        """
        Extract lexical features for each domain name in the given DataFrame.

        Parameters:
            df (pd.DataFrame): A DataFrame containing domain names.

        Returns:
            pd.DataFrame: A DataFrame with lexical features added.
        """
        # Ensure that the DataFrame has a column named 'domain_name'
        if "domain_name" not in df.columns:
            raise ValueError("DataFrame must contain a 'domain_name' column.")

        # Iterate over the lexical columns defined in the template and apply the necessary transformations
        for feature, dtype in self.lexical_template.lexical_columns.items():
            # This is a placeholder for actual feature extraction logic.
            # You might need to replace 'self._calculate_feature' with actual
            # function calls relevant to your feature calculations.
            pass
            
            # df[feature] = df["domain_name"].apply(
            #     lambda x: self._calculate_feature(x, feature)
            # )

        return df
