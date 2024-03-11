using Microsoft.Extensions.DependencyInjection;

namespace Common.Installers;
public abstract class AbstractInstaller : IInstaller
{
    public virtual void Install(IServiceCollection serviceCollection)
    {
    }
    
    public virtual void Install(IServiceCollection serviceCollection, string connectionString, string databaseName)
    {
    }
}
