/**
 * @file MPMCQueueWrapper.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration and implementation of the MPMCQueueWrapper class.
 *
 * The MPMCQueueWrapper class serves as an adapter for the rigtorp::MPMCQueue, providing
 * a concrete implementation of the IQueue interface. This wrapper allows the MPMCQueue
 * to be used in contexts where an IQueue is required, enabling seamless integration with
 * systems designed to operate on the IQueue interface. The class encapsulates the functionality
 * of the MPMCQueue, ensuring thread-safe operations and efficient queue management for concurrent
 * environments.
 *
 * Key features include:
 * - Thread-safe push and pop operations.
 * - Efficient handling of multiple producers and consumers.
 *
 * @version 1.0
 * @date 2024-03-04
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <rigtorp/MPMCQueue.h>

#include "IQueue.hpp"

template <typename T>
class MPMCQueueWrapper : public IQueue<T>
{
    rigtorp::MPMCQueue<T> queue;

public:
    /**
     * @brief Constructs a new MPMCQueueWrapper object.
     *
     * Initializes the underlying MPMCQueue with the specified size.
     *
     * @param size The capacity of the queue.
     */
    explicit MPMCQueueWrapper(size_t size) : queue(size) {}

    /**
     * @brief Attempts to pop an element from the queue.
     *
     * Overrides the try_pop method from IQueue, utilizing the MPMCQueue's try_pop
     * to attempt to pop an element. If successful, the element is removed from the queue
     * and copied into the provided reference.
     *
     * @param value Reference to a variable of type T where the popped value will be stored if available.
     * @return true If an element was successfully popped and value was updated.
     * @return false If the queue was empty and no element could be popped.
     */
    bool try_pop(T &value) override
    {
        return queue.try_pop(value);
    }

    /**
     * @brief Inserts an element into the queue by moving it.
     *
     * Overrides the emplace method from IQueue, forwarding the provided rvalue reference
     * to the MPMCQueue's emplace function to efficiently insert the element into the queue.
     *
     * @param value An rvalue reference to an instance of type T, which will be moved into the queue.
     */
    void emplace(T &&value) override
    {
        queue.emplace(std::forward<T>(value));
    }
};
