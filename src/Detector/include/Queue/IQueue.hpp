/**
 * @file IQueue.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of the IQueue interface for generic queue operations.
 *
 * This file introduces the IQueue interface, designed to abstract queue operations
 * for various data types. It provides a blueprint for queue implementations, ensuring
 * they offer essential functionalities such as try_pop and emplace. This interface allows
 * for type-safe queue operations, making it a versatile component in applications requiring
 * queue data structures.
 *
 * Key components include:
 * - The try_pop method, for attempting to pop an element from the queue.
 * - The emplace method, for inserting an element into the queue.
 *
 * @version 1.0
 * @date 2024-03-04
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

template <typename T>
class IQueue
{
public:
    /**
     * @brief Virtual destructor for the IQueue interface, ensuring derived classes can clean up resources properly.
     */
    virtual ~IQueue() = default;

    /**
     * @brief Attempts to pop an element from the queue. If the queue is empty, returns false without modifying the value parameter.
     *
     * @param value Reference to a variable of type T, where the popped value will be stored if available.
     * @return true If an element was successfully popped and value was updated.
     * @return false If the queue was empty and no element could be popped.
     */
    virtual bool try_pop(T &value) = 0;

    /**
     * @brief Inserts an element into the queue by constructing it in place, avoiding unnecessary copy or move operations.
     *
     * @param value An rvalue reference to an instance of type T, which will be moved into the queue.
     */
    virtual void emplace(T &&value) = 0;
};
