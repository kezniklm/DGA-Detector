using APP.Config;
using AutoMapper;
using AutoMapper.Internal;
using BL.Installers;
using BL.Models.User;
using Common.Extensions;
using DAL.Entities.Interfaces;
using DAL.Installers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConfigureCors(builder.Services);

ConfigureOpenApiDocuments(builder.Services);

ConfigureDependencies(builder.Services, builder.Configuration);

ConfigureAutoMapper(builder.Services);

ConfigureControllers(builder.Services);

WebApplication app = builder.Build();

ValidateAutoMapperConfiguration(app.Services);

UseDevelopmentSettings(app);

UseSecurityFeatures(app);

UseRouting(app);

UseEndpoints(app);

UseOpenApi(app);

app.Run();

void ConfigureCors(IServiceCollection serviceCollection)
{
    serviceCollection.AddCors(options =>
    {
        options.AddDefaultPolicy(o =>
            o.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
    });
}

void ConfigureOpenApiDocuments(IServiceCollection serviceCollection)
{
    serviceCollection.AddEndpointsApiExplorer();
    serviceCollection.AddOpenApiDocument();
}

void ConfigureDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
{
    DbConfig dbConfig = GetDatabaseConfig(configuration);

    serviceCollection.AddIdentityApiEndpoints<User>()
        .AddMongoDbStores<User, UserRole, Guid>
        (
            dbConfig.ConnectionString, dbConfig.DatabaseName
        )
        .AddDefaultTokenProviders();

    serviceCollection.AddInstaller<DalInstaller>(dbConfig.ConnectionString,dbConfig.DatabaseName);

    serviceCollection.AddInstaller<BlInstaller>();
}

DbConfig GetDatabaseConfig(IConfiguration configuration)
{
    IConfigurationSection dbConfigSection = configuration.GetSection("DbConfig");

    string connectionString = dbConfigSection["ConnectionString"] ??
                              throw new ArgumentException("The connection string is missing");
    string databaseName =
        dbConfigSection["DatabaseName"] ?? throw new ArgumentException("The database name is missing");

    return new DbConfig { ConnectionString = connectionString, DatabaseName = databaseName };
}


void ConfigureAutoMapper(IServiceCollection serviceCollection)
{
    serviceCollection.AddAutoMapper(typeof(IEntity), typeof(BlInstaller));
}

void ConfigureControllers(IServiceCollection serviceCollection)
{
    builder.Services.AddControllers();
}

void ValidateAutoMapperConfiguration(IServiceProvider serviceProvider)
{
    IMapper mapper = serviceProvider.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}

void UseDevelopmentSettings(WebApplication application)
{
    IWebHostEnvironment environment = application.Services.GetRequiredService<IWebHostEnvironment>();

    if (environment.IsDevelopment())
    {
        application.UseDeveloperExceptionPage();
    }
}
void UseSecurityFeatures(IApplicationBuilder application)
{
    application.UseCors();
    application.UseHttpsRedirection();
    MapIdentityEndpoints(app);
    application.UseAuthorization();
    application.UseAuthentication();
}

void MapIdentityEndpoints(WebApplication application)
{
    application.MapIdentityApi<User>().WithTags("Auth"); ;
}

void UseRouting(IApplicationBuilder application)
{
    application.UseRouting();
}

void UseEndpoints(WebApplication application)
{
    app.MapControllers();
}

void UseOpenApi(IApplicationBuilder application)
{
    application.UseOpenApi();
    application.UseSwaggerUi();
}
