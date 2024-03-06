from enum import Enum, auto


class ReturnCodes(Enum):
    SUCCESS = 0
    FILE_NOT_FOUND = auto()
    JSON_DECODE_ERROR = auto()
    MISSING_ARGUMENTS = auto()
    RABBITMQ_CONNECTION_FAILED = auto()
