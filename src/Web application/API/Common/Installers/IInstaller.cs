/**
 * @file IInstaller.cs
 *
 * @brief Defines an interface for installing services in the dependency injection container.
 *
 * This file contains the definition of the IInstaller interface, which declares methods for installing services in the dependency injection container. Implementations of this interface are responsible for configuring and registering services within the application.
 *
 * The main functionalities of this file include:
 * - Declaring the Install method for installing services in the IServiceCollection.
 * - Overloading the Install method to include a DbConfig parameter for configuring database services.
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
///     Interface for installer classes responsible for configuring services.
/// </summary>
public interface IInstaller
{
    /// <summary>
    ///     Installs services into the specified <paramref name="serviceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The service collection to install services into.</param>
    void Install(IServiceCollection serviceCollection);

    /// <summary>
    ///     Installs services into the specified <paramref name="serviceCollection" /> using the provided
    ///     <paramref name="config" />.
    /// </summary>
    /// <param name="serviceCollection">The service collection to install services into.</param>
    /// <param name="config">The database configuration to use during installation.</param>
    void Install(IServiceCollection serviceCollection, DbConfig config);
}
