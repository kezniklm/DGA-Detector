#pragma once

#include <cstring>
#include <algorithm>
#include <pcap.h>

/**
 * @struct DetectorPacket
 * @brief Represents a network packet with a hybrid storage strategy.
 * This structure is optimized for detecting specific data patterns, providing efficient handling for both small and large packets.
 */
struct DetectorPacket
{
    /** Maximum size for static allocation. */
    static constexpr size_t MAX_STATIC_SIZE = 750;

    /** Packet header, initialized to zero. */
    pcap_pkthdr header;

    /** Actual size of the packet data, initialized to zero. */
    size_t data_size = 0;

    /** Static buffer for small packets. */
    u_char static_data[MAX_STATIC_SIZE];

    /** Pointer for dynamically allocated data for large packets. */
    u_char *dynamic_data = nullptr;

    /**
     * @brief Default constructor.
     * Initializes a new instance with default values.
     */
    DetectorPacket()
    {
        memset(&header, 0, sizeof(header));
        data_size = 0;
        memset(static_data, 0, MAX_STATIC_SIZE);
        dynamic_data = nullptr;
    }

    /**
     * @brief Constructs a DetectorPacket object with packet header and data, using hybrid storage.
     *
     * @param hdr The packet header.
     * @param packet_data Pointer to the packet data.
     */
    DetectorPacket(const pcap_pkthdr &hdr, const u_char *packet_data) : header(hdr), data_size(hdr.len)
    {
        if (data_size <= MAX_STATIC_SIZE)
        {
            memcpy(static_data, packet_data, data_size);
        }
        else
        {
            dynamic_data = new u_char[data_size];
            memcpy(dynamic_data, packet_data, data_size);
        }
    }

    /**
     * @brief Copy constructor.
     * Deep copies another DetectorPacket object.
     *
     * @param other The DetectorPacket object to copy from.
     */
    DetectorPacket(const DetectorPacket &other) : header(other.header), data_size(other.data_size), dynamic_data(nullptr)
    {
        if (data_size <= MAX_STATIC_SIZE)
        {
            memcpy(static_data, other.static_data, data_size);
        }
        else
        {
            dynamic_data = new u_char[data_size];
            memcpy(dynamic_data, other.dynamic_data, data_size);
        }
    }

    /**
     * @brief Move constructor.
     * Efficiently transfers resources from another DetectorPacket object.
     *
     * @param other The DetectorPacket object to move from.
     */
    DetectorPacket(DetectorPacket &&other) noexcept
        : header(other.header), data_size(other.data_size), dynamic_data(other.dynamic_data)
    {
        if (data_size > MAX_STATIC_SIZE)
        {
            other.dynamic_data = nullptr;
        }
        else
        {
            memcpy(static_data, other.static_data, data_size);
        }
    }

    /**
     * @brief Destructor.
     */
    ~DetectorPacket()
    {
        delete[] dynamic_data;
    }

    /**
     * @brief Move assignment operator.
     *
     * @param other The DetectorPacket object to move from.
     * @return DetectorPacket& Reference to this object.
     */
    DetectorPacket &operator=(DetectorPacket &&other) noexcept
    {
        if (this == &other)
            return *this;

        header = other.header;
        data_size = other.data_size;

        if (data_size <= MAX_STATIC_SIZE)
        {
            memcpy(static_data, other.static_data, data_size);
        }
        else
        {
            delete[] dynamic_data;
            dynamic_data = other.dynamic_data;
            other.dynamic_data = nullptr;
        }

        return *this;
    }

    /**
     * @brief Copy assignment operator.
     * Deep copies another DetectorPacket object.
     *
     * @param other The DetectorPacket object to copy from.
     * @return DetectorPacket& Reference to this object.
     */
    DetectorPacket &operator=(const DetectorPacket &other) noexcept
    {
        if (this != &other)
        {
            header = other.header;
            data_size = other.data_size;

            if (data_size <= MAX_STATIC_SIZE)
            {
                memcpy(static_data, other.static_data, data_size);
            }
            else
            {
                u_char *newData = new u_char[data_size];
                memcpy(newData, other.dynamic_data, data_size);
                delete[] dynamic_data;
                dynamic_data = newData;
            }
        }

        return *this;
    }

    /**
     * @brief Get a pointer to the packet data, whether it's in static or dynamic storage.
     *
     * @return const u_char* Pointer to the packet data.
     */
    const u_char *getData() const
    {
        return data_size <= MAX_STATIC_SIZE ? static_data : dynamic_data;
    }
};