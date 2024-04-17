/**
 * @file IWithId.cs
 *
 * @brief Contains the definition of the IWithId interface.
 *
 * This file defines the IWithId interface, which represents entities with an identifier property.
 * Classes implementing this interface should provide an implementation for the Id property.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

namespace Common;

/// <summary>
///     Represents an interface for objects with an identifier.
/// </summary>
public interface IWithId
{
    /// <summary>
    ///     Gets or sets the identifier of the object.
    /// </summary>
    /// <value>The identifier of the object.</value>
    string Id { get; set; }
}
