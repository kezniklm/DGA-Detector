/**
 * @file IBlacklistRepository.cs
 *
 * @brief Defines the interface for Blacklist repository in the DAL.
 *
 * This file contains the definition of the IBlacklistRepository interface, which specifies the contract for repositories handling BlacklistEntity objects in the DAL (Data Access Layer).
 *
 * The main functionalities of this interface include:
 * - Inheriting from the IRepository<T> interface to provide generic repository operations.
 * - Specifying methods specific to handling BlacklistEntity objects.
 *
 * @see IRepository<T>
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
///     Interface for accessing and managing BlacklistEntity objects in the repository.
/// </summary>
public interface IBlacklistRepository : IRepository<BlacklistEntity>
{
}
