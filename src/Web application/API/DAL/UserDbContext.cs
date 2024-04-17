/**
 * @file UserDbContext.cs
 *
 * @brief Represents the database context for user-related operations.
 *
 * This file contains the implementation of the UserDbContext class, which serves as the database context for user-related operations. It extends the IdentityDbContext class from Microsoft.AspNetCore.Identity.EntityFrameworkCore, allowing integration with ASP.NET Core Identity.
 *
 * The main functionalities of this file include:
 * - Defining the database context for user-related operations.
 * - Configuring the entity models, particularly the User entity, using the OnModelCreating method to customize the database schema.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using Common.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class UserDbContext(DbContextOptions<UserDbContext> options) : IdentityDbContext<User>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(b => b.Property(u => u.PhotoUrl).HasMaxLength(200));
    }
}
