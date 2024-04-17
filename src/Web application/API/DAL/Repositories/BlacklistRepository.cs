/**
 * @file BlacklistRepository.cs
 *
 * @brief Implements the repository for managing blacklisted entities.
 *
 * This file contains the implementation of the BlacklistRepository class, which serves as the repository for managing blacklisted entities in the database. It extends the RepositoryBase class and implements the IBlacklistRepository interface.
 *
 * The main functionalities of this class include:
 * - Providing CRUD operations for managing blacklisted entities.
 * - Inheriting common repository functionality from the RepositoryBase class.
 * - Implementing specific repository methods defined in the IBlacklistRepository interface.
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
///     Repository for managing blacklisted entities in the database.
/// </summary>
internal class BlacklistRepository(ApiDbContext dbContext)
    : RepositoryBase<BlacklistEntity>(dbContext), IBlacklistRepository
{
}
