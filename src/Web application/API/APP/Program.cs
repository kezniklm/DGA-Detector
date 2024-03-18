using APP.Config;
using APP.Constraints;
using AutoMapper;
using BL.Installers;
using Common.Extensions;
using Common.Models;
using DAL;
using DAL.Entities.Interfaces;
using DAL.Installers;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Core;
using Swashbuckle.AspNetCore.Filters;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConfigureLogging(builder.Services);

ConfigureCors(builder.Services);

ConfigureOpenApiDocuments(builder.Services);

ConfigureDependencies(builder.Services, builder.Configuration);

ConfigureCookies(builder.Services);

ConfigureAutoMapper(builder.Services);

ConfigureControllers(builder.Services);

ConfigureConstraints(builder.Services);

WebApplication app = builder.Build();

ValidateAutoMapperConfiguration(app.Services);

UseDevelopmentOrProductionSettings(app);

UseRouting(app);

UseSecurityFeatures(app);

UseEndpoints(app);

UseOpenApi(app);

app.Run();

void ConfigureLogging(IServiceCollection services)
{
    LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.Console();

    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
    {
        loggerConfiguration.WriteTo.EventLog(
            source: "DGA-Detector",
            manageEventSource: true,
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error);
    }
    else
    {
        loggerConfiguration.WriteTo.LocalSyslog(
            appName: "DGA-Detector",
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error);
    }

    Logger logger = loggerConfiguration.CreateLogger();

    builder.Host.UseSerilog(logger);
}

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

    serviceCollection.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1",
            new OpenApiInfo
            {
                Title = "DGA-Detector API", Description = "API for DGA-Detector application", Version = "v1"
            });
        options.AddSecurityDefinition("oauth2",
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header, Name = "Authorization", Type = SecuritySchemeType.ApiKey
            });

        options.OperationFilter<SecurityRequirementsOperationFilter>();
    });
}

void ConfigureDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
{
    DbConfig dbConfig = GetDatabaseConfig(configuration);

    serviceCollection.AddIdentityApiEndpoints<User>().AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();

    serviceCollection.AddInstaller<DalInstaller>(dbConfig.ConnectionString, dbConfig.DatabaseName);

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

void ConfigureCookies(IServiceCollection serviceCollection)
{
    serviceCollection.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "DGA-Detector_Cookie";

        options.LoginPath = "/Login";

        options.LogoutPath = "/Logout";

        options.ExpireTimeSpan = TimeSpan.FromDays(15);

        // options.Cookie.HttpOnly = true;

        // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

    serviceCollection.AddDistributedMemoryCache();
    serviceCollection.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });
}


void ConfigureAutoMapper(IServiceCollection serviceCollection)
{
    serviceCollection.AddAutoMapper(typeof(IEntity), typeof(BlInstaller));
}

void ConfigureControllers(IServiceCollection serviceCollection)
{
    serviceCollection.AddControllers();

    //serviceCollection.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
    //{
    //    options.LoginPath = "/login";
    //    options.LogoutPath = "/logout";
    //});

    serviceCollection.AddAuthorization();

    serviceCollection.AddEndpointsApiExplorer();
}

void ConfigureConstraints(IServiceCollection serviceCollection)
{
    serviceCollection.Configure<RouteOptions>(options =>
    {
        options.ConstraintMap.Add("ObjectId", typeof(ObjectIdConstraint));
    });
}

void ValidateAutoMapperConfiguration(IServiceProvider serviceProvider)
{
    IMapper mapper = serviceProvider.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}

void UseDevelopmentOrProductionSettings(WebApplication application)
{
    IWebHostEnvironment environment = application.Services.GetRequiredService<IWebHostEnvironment>();

    if (environment.IsDevelopment())
    {
        application.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }
}

void UseSecurityFeatures(IApplicationBuilder application)
{
    application.UseCors();
    application.UseHttpsRedirection();
    MapIdentityEndpoints(app);
    application.UseSession();
    application.UseAuthentication();
    application.UseAuthorization();
}

void MapIdentityEndpoints(WebApplication application)
{
    application.MapIdentityApi<User>().WithTags("Auth");
}

void UseRouting(IApplicationBuilder application)
{
    application.UseRouting();
}

void UseEndpoints(WebApplication application)
{
    application.MapControllers();
}

void UseOpenApi(IApplicationBuilder application)
{
    application.UseSwagger();
    application.UseSwaggerUI();
}

// Make the implicit Program class public so test projects can access it
public partial class Program
{
}
