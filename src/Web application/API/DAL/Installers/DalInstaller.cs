using Common.Config;
using Common.Installers;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Installers;

public class DalInstaller : AbstractInstaller
{
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
