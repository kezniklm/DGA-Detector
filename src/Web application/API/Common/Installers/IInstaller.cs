using Microsoft.Extensions.DependencyInjection;

namespace Common.Installers;

public interface IInstaller
{
    void Install(IServiceCollection serviceCollection);
    void Install(IServiceCollection serviceCollection, string connectionString, string databaseName);
}
