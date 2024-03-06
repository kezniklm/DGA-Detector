from multiprocessing import Queue
from typing import TypeVar, Generic

T = TypeVar('T')

class StringQueue(Generic[T]):
    def __init__(self, maxsize):
        self.maxsize = maxsize
        self.queue = Queue(maxsize=maxsize)

    def put(self, item: T):
        self.queue.put(item)

    def get(self) -> T:
        return self.queue.get()
