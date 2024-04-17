/**
 * @file IWhitelistRepository.cs
 *
 * @brief Defines the interface for a whitelist repository.
 *
 * This file contains the definition of the IWhitelistRepository interface, which specifies methods for interacting with a whitelist of entities. It extends the IRepository interface, providing CRUD operations for WhitelistEntity objects.
 *
 * The main functionalities of this interface include:
 * - Inheriting CRUD operations from the IRepository interface.
 * - Defining additional methods specific to whitelist management.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using DAL.Entities;

namespace DAL.Repositories.Interfaces;

/// <summary>
///     Represents the interface for accessing and managing whitelist entities in the repository.
/// </summary>
public interface IWhitelistRepository : IRepository<WhitelistEntity>
{
}
