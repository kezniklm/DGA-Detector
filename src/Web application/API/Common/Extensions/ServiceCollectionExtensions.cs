/**
 * @file ServiceCollectionExtensions.cs
 *
 * @brief Extends IServiceCollection to include methods for adding service installers.
 *
 * This file contains the implementation of extensions to IServiceCollection for dynamically adding services through installers. This approach facilitates the modular addition of services to the dependency injection container based on loosely coupled installers that adhere to the IInstaller interface.
 *
 * The main functionalities of this file include:
 * - Providing a method to add an installer to IServiceCollection without additional parameters.
 * - Providing a method to add an installer to IServiceCollection that requires database configuration parameters.
 *
 * These extension methods increase the flexibility and maintainability of application setup by abstracting the specifics of service registration.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using Common.Config;
using Common.Installers;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IServiceCollection" /> to add services using installers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds services to the specified <see cref="IServiceCollection" /> using an installer.
    /// </summary>
    /// <typeparam name="TInstaller">
    ///     The type of the installer to create and use. This class must implement
    ///     <see cref="IInstaller" /> and have a parameterless constructor.
    /// </typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <remarks>
    ///     This method creates an instance of <typeparamref name="TInstaller" /> and calls its <c>Install</c> method to add
    ///     services.
    /// </remarks>
    public static void AddInstaller<TInstaller>(this IServiceCollection serviceCollection)
        where TInstaller : IInstaller, new()
    {
        TInstaller installer = new();
        installer.Install(serviceCollection);
    }

    /// <summary>
    ///     Adds services to the specified <see cref="IServiceCollection" /> using an installer and additional configuration
    ///     data.
    /// </summary>
    /// <typeparam name="TInstaller">
    ///     The type of the installer to create and use. This class must implement
    ///     <see cref="IInstaller" /> and have a parameterless constructor.
    /// </typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="dbConfig">The database configuration to pass to the installer.</param>
    /// <remarks>
    ///     This method creates an instance of <typeparamref name="TInstaller" /> and calls its <c>Install</c> method with
    ///     <paramref name="dbConfig" /> to add services.
    /// </remarks>
    public static void AddInstaller<TInstaller>(this IServiceCollection serviceCollection, DbConfig dbConfig)
        where TInstaller : IInstaller, new()
    {
        TInstaller installer = new();
        installer.Install(serviceCollection, dbConfig);
    }
}
