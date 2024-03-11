using Common.Installers;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Installers;

public class DalInstaller : AbstractInstaller
{
    public override void Install(IServiceCollection serviceCollection, string connectionString, string databaseName)
    {
        serviceCollection.AddDbContext<ApiDbContext>(options => options.UseMongoDB(connectionString, databaseName));

        serviceCollection.Scan(selector =>
            selector.FromAssemblyOf<DalInstaller>()
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
    }
}
