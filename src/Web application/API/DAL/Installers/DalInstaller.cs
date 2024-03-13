using Common.Installers;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Installers;

public class DalInstaller : AbstractInstaller
{
    public override void Install(IServiceCollection serviceCollection, string connectionString, string databaseName)
    {
        serviceCollection.AddDbContext<UserDbContext>(options =>
            options.UseSqlite("Data Source=userdatabase.db;", b => b.MigrationsAssembly("DAL")));

        serviceCollection.AddScoped<ApiDbContext>(sp => new ApiDbContext(connectionString, databaseName));

        serviceCollection.Scan(selector =>
            selector.FromAssemblyOf<DalInstaller>()
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime());
    }
}
