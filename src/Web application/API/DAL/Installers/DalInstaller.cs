/**
 * @file DalInstaller.cs
 *
 * @brief Installs services related to Data Access Layer (DAL).
 *
 * This file contains the implementation of the DalInstaller class, which is responsible for installing services related to the Data Access Layer (DAL) using Microsoft.Extensions.DependencyInjection.
 *
 * The main functionalities of this file include:
 * - Installing Entity Framework Core database context for user data.
 * - Configuring the database context based on the provided configuration (MySQL or SQLite).
 * - Registering the API database context with scoped lifetime.
 * - Scanning DAL assembly for repository implementations and registering them with their interfaces.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using Common.Config;
using Common.Installers;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Installers;

/// <summary>
///     Installer class for DAL services.
/// </summary>
public class DalInstaller : AbstractInstaller
{
    /// <summary>
    ///     Installs DAL services into the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to install services into.</param>
    /// <param name="config">The database configuration.</param>
    public override void Install(IServiceCollection serviceCollection, DbConfig config)
    {
        if (config.UserDatabaseConfig.UseMySql)
        {
            serviceCollection.AddDbContext<UserDbContext>(options =>
                options.UseMySQL(config.UserDatabaseConfig.ConnectionString,
                    b => b.MigrationsAssembly("DAL")));
        }
        else
        {
            string sqliteConnectionString = $"Data Source={config.UserDatabaseConfig.DatabaseName}.db;";
            serviceCollection.AddDbContext<UserDbContext>(options =>
                options.UseSqlite(sqliteConnectionString, b => b.MigrationsAssembly("DAL")));
        }

        serviceCollection.AddScoped<ApiDbContext>(sp => new ApiDbContext(config.ConnectionString, config.DatabaseName));

        serviceCollection.Scan(selector =>
            selector.FromAssemblyOf<DalInstaller>()
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime());
    }
}
