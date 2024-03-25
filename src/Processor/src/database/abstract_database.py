"""
 * @file abstract_database.py
 * @brief Defines an abstract base class for interacting with a database.
 *
 * This file contains the definition of the AbstractDatabase class, which serves as an abstract base class for classes that interact with databases. It defines abstract methods for establishing a connection to the database and inserting data into it.
 *
 * The main functionalities of this file include:
 * - Defining an abstract base class (ABC) for interacting with a database.
 * - Declaring abstract methods for connecting to a database and inserting data.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024 
 *
 * @see ABC, abstractmethod
 * @warning This is an abstract base class and should not be instantiated directly.
"""

from abc import ABC, abstractmethod

from pandas import DataFrame


class AbstractDatabase(ABC):
    """
    @brief Abstract base class for interacting with a database.

    This class defines abstract methods for connecting to a database and inserting data into it.

    Attributes:
        None
    """

    @abstractmethod
    def connect(self) -> None:
        """
        @brief Establish a connection to the database.

        This method should be implemented by concrete subclasses to establish a connection to the database.
        """
        pass

    @abstractmethod
    def insert_dataframe(self, df: DataFrame) -> None:
        """
        @brief Insert data into the database.

        This method should be implemented by concrete subclasses to insert data into the database.

        @param df: The DataFrame containing data to be inserted into the database.
        """
        pass
