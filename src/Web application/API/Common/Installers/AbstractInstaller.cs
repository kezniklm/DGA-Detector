/**
 * @file AbstractInstaller.cs
 *
 * @brief Provides an abstract base class for service installers in the Common namespace.
 *
 * This file contains the implementation of the AbstractInstaller class, which serves as an abstract base class for service installers within the Common namespace. It defines methods for installing services into the provided IServiceCollection.
 *
 * The main functionalities of this file include:
 * - Defining an abstract base class for service installers.
 * - Providing methods for installing services into the IServiceCollection.
 * - Supporting installation with or without a DbConfig parameter.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using Common.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Installers;

/// <summary>
///     Abstract class defining the structure for installers.
/// </summary>
public abstract class AbstractInstaller : IInstaller
{
    /// <summary>
    ///     Installs services into the provided service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to install services into.</param>
    public virtual void Install(IServiceCollection serviceCollection)
    {
    }

    /// <summary>
    ///     Installs services into the provided service collection using a database configuration.
    /// </summary>
    /// <param name="serviceCollection">The service collection to install services into.</param>
    /// <param name="config">The database configuration.</param>
    public virtual void Install(IServiceCollection serviceCollection, DbConfig config)
    {
    }
}
