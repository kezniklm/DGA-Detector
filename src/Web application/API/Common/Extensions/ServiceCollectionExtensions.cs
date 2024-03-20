using Common.Config;
using Common.Installers;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInstaller<TInstaller>(this IServiceCollection serviceCollection)
        where TInstaller : IInstaller, new()
    {
        TInstaller installer = new();
        installer.Install(serviceCollection);
    }


    public static void AddInstaller<TInstaller>(this IServiceCollection serviceCollection, DbConfig dbConfig)
        where TInstaller : IInstaller, new()
    {
        TInstaller installer = new();
        installer.Install(serviceCollection, dbConfig);
    }
}
