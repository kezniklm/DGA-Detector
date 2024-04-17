/**
 * @file BlInstaller.cs
 *
 * @brief Installs services for Business Logic layer.
 *
 * This file contains the implementation of the BlInstaller class, which is responsible for installing services for the Business Logic layer. It utilizes Microsoft.Extensions.DependencyInjection for service registration.
 *
 * The main functionalities of this file include:
 * - Scanning assemblies to discover classes implementing specific interfaces.
 * - Registering classes implementing the IFacade interface.
 * - Assigning implemented interfaces to these classes.
 * - Setting scoped lifetime for the registered services.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using BL.Facades.Interfaces;
using Common.Installers;
using Microsoft.Extensions.DependencyInjection;

namespace BL.Installers;

/// <summary>
///     Installer class for BL layer services.
/// </summary>
public class BlInstaller : AbstractInstaller
{
    /// <summary>
    ///     Method to install services.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add services to.</param>
    public override void Install(IServiceCollection serviceCollection) =>
        serviceCollection.Scan(selector =>
            selector.FromAssemblyOf<BlInstaller>()
                .AddClasses(classes => classes.AssignableTo(typeof(IFacade<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
}
