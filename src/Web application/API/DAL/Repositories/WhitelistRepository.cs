/**
 * @file WhitelistRepository.cs
 *
 * @brief Defines a repository for managing whitelisted entities in the database.
 *
 * This file contains the implementation of the WhitelistRepository class, which is responsible for managing whitelisted entities in the database. It extends the RepositoryBase class and implements the IWhitelistRepository interface.
 *
 * The main functionalities of this file include:
 * - Managing whitelisted entities in the database.
 * - Inheriting from RepositoryBase class for basic repository operations.
 * - Implementing the IWhitelistRepository interface for specific whitelist-related methods.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories;

/// <summary>
///     Represents a repository for managing whitelisted entities in the database.
/// </summary>
internal class WhitelistRepository(ApiDbContext dbContext)
    : RepositoryBase<WhitelistEntity>(dbContext), IWhitelistRepository
{
}
