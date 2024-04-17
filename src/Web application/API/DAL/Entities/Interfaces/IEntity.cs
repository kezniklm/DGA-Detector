/**
 * @file IEntity.cs
 *
 * @brief Defines the IEntity interface for entities in the data access layer.
 *
 * This file contains the definition of the IEntity interface, which represents entities in the data access layer. Entities implementing this interface should provide properties for unique identifiers and domain names.
 *
 * The main functionalities of this interface include:
 * - Providing a contract for entities in the data access layer.
 * - Extending the IWithId interface for unique identifiers.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using Common;

namespace DAL.Entities.Interfaces;

/// <summary>
///     Interface for entities in the data access layer.
/// </summary>
public interface IEntity : IWithId
{
    /// <summary>
    ///     Gets or sets the domain name associated with the entity.
    /// </summary>
    /// <value>The domain name.</value>
    string DomainName { get; set; }
}
