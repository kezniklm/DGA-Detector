/**
 * @file Program.cs
 *
 * @brief Configures and runs the web application.
 *
 * This file contains the entry point of the web application. It configures various services, such as logging, CORS, OpenAPI documentation, dependencies, cookies, AutoMapper, controllers, and security features. It also defines methods for configuring and using these services.
 *
 * The main functionalities of this file include:
 * - Configuring logging using Serilog, with options for different platforms.
 * - Configuring CORS (Cross-Origin Resource Sharing) to allow requests from any origin.
 * - Configuring OpenAPI documentation using Swagger.
 * - Configuring dependencies for data access and business logic layers.
 * - Configuring cookies and sessions for user authentication.
 * - Configuring AutoMapper for object mapping.
 * - Configuring controllers and authorization.
 * - Validating AutoMapper configuration.
 * - Selecting development or production settings based on the environment.
 * - Configuring and using security features such as HTTPS redirection, session management, authentication, and authorization.
 * - Defining routing and endpoint mappings.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using System.Runtime.InteropServices;
using AutoMapper;
using BL.Installers;
using Common.Config;
using Common.Extensions;
using Common.Models;
using DAL;
using DAL.Entities.Interfaces;
using DAL.Installers;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Swashbuckle.AspNetCore.Filters;

// Create a new web application builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure logging services
ConfigureLogging(builder.Services);

// Configure Cross-Origin Resource Sharing (CORS) policies
ConfigureCors(builder.Services);

// Configure OpenAPI document generation
ConfigureOpenApiDocuments(builder.Services);

// Configure application dependencies
ConfigureDependencies(builder.Services, builder.Configuration);

// Configure cookie settings
ConfigureCookies(builder.Services);

// Configure AutoMapper for object-object mapping
ConfigureAutoMapper(builder.Services);

// Configure controllers and authorization
ConfigureControllers(builder.Services);

// Build the web application
WebApplication app = builder.Build();

// Validate AutoMapper configuration
ValidateAutoMapperConfiguration(app.Services);

// Use development or production settings based on environment
UseDevelopmentOrProductionSettings(app);

// Enable routing
UseRouting(app);

// Configure and use security features
UseSecurityFeatures(app);

// Configure and use endpoints
UseEndpoints(app);

// Configure and use OpenAPI (Swagger) UI
UseOpenApi(app);

// Start the application
app.Run();

/// <summary>
/// Configures logging services for the application.
/// </summary>
/// <param name="services">Collection of services in the application.</param>
/// <returns>Returns void.</returns>
void ConfigureLogging(IServiceCollection services)
{
    LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.Console();

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        loggerConfiguration.WriteTo.EventLog(
            "DGA-Detector",
            manageEventSource: true,
            restrictedToMinimumLevel: LogEventLevel.Error);
    }
    else
    {
        loggerConfiguration.WriteTo.LocalSyslog(
            "DGA-Detector",
            restrictedToMinimumLevel: LogEventLevel.Error);
    }

    Logger logger = loggerConfiguration.CreateLogger();

    builder.Host.UseSerilog(logger);
}

/// <summary>
/// Configures Cross-Origin Resource Sharing (CORS) policies for the application.
/// </summary>
/// <param name="serviceCollection">Collection of services in the application.</param>
/// <returns>Returns void.</returns>
void ConfigureCors(IServiceCollection serviceCollection)
{
    serviceCollection.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
            builder.SetIsOriginAllowed(origin => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    });
}

/// <summary>
/// Configures OpenAPI document generation for the application.
/// </summary>
/// <param name="serviceCollection">Collection of services in the application.</param>
/// <returns>Returns void.</returns>
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

/// <summary>
/// Configures application dependencies.
/// </summary>
/// <param name="serviceCollection">Collection of services in the application.</param>
/// <param name="configuration">Configuration for the application.</param>
/// <returns>Returns void.</returns>
void ConfigureDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
{
    DbConfig dbConfig = GetDatabaseConfig(configuration);

    serviceCollection.AddIdentityApiEndpoints<User>().AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();

    serviceCollection.AddInstaller<DalInstaller>(dbConfig);

    serviceCollection.AddInstaller<BlInstaller>();
}

/// <summary>
/// Retrieves database configuration from the application configuration.
/// </summary>
/// <param name="configuration">Configuration for the application.</param>
/// <returns>Returns database configuration.</returns>
DbConfig GetDatabaseConfig(IConfiguration configuration)
{
    DbConfig dbConfig = configuration.GetSection("DbConfig").Get<DbConfig>() ??
                        throw new ArgumentException("The DbConfig section is missing or improperly configured.");

    return dbConfig;
}

/// <summary>
/// Configures cookie settings for the application.
/// </summary>
/// <param name="serviceCollection">Collection of services in the application.</param>
/// <returns>Returns void.</returns>
void ConfigureCookies(IServiceCollection serviceCollection)
{
    serviceCollection.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "DGA-Detector_Cookie";

        options.LoginPath = "/Login";

        options.LogoutPath = "/Logout";

        options.ExpireTimeSpan = TimeSpan.FromDays(15);

        options.Cookie.HttpOnly = true;

        options.Cookie.IsEssential = true;

        options.Cookie.SameSite = SameSiteMode.None;

        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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

/// <summary>
/// Configures AutoMapper for object-object mapping.
/// </summary>
/// <param name="serviceCollection">Collection of services in the application.</param>
/// <returns>Returns void.</returns>
void ConfigureAutoMapper(IServiceCollection serviceCollection)
{
    serviceCollection.AddAutoMapper(typeof(IEntity), typeof(BlInstaller));
}

/// <summary>
/// Configures controllers and authorization.
/// </summary>
/// <param name="serviceCollection">Collection of services in the application.</param>
/// <returns>Returns void.</returns>
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

/// <summary>
/// Validates AutoMapper configuration.
/// </summary>
/// <param name="serviceProvider">Service provider for the application.</param>
/// <returns>Returns void.</returns>
void ValidateAutoMapperConfiguration(IServiceProvider serviceProvider)
{
    IMapper mapper = serviceProvider.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}

/// <summary>
/// Configures development or production settings based on environment.
/// </summary>
/// <param name="application">Web application instance.</param>
/// <returns>Returns void.</returns>
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

/// <summary>
/// Configures security features for the application.
/// </summary>
/// <param name="application">Web application instance.</param>
/// <returns>Returns void.</returns>
void UseSecurityFeatures(IApplicationBuilder application)
{
    application.UseCors();
    application.UseHttpsRedirection();
    MapIdentityEndpoints(app);
    application.UseSession();
    application.UseAuthentication();
    application.UseAuthorization();
}

/// <summary>
/// Maps Identity API endpoints.
/// </summary>
/// <param name="application">Web application instance.</param>
/// <returns>Returns void.</returns>
void MapIdentityEndpoints(WebApplication application)
{
    application.MapIdentityApi<User>().WithTags("Auth");
}

/// <summary>
/// Enables routing for the application.
/// </summary>
/// <param name="application">Web application instance.</param>
/// <returns>Returns void.</returns>
void UseRouting(IApplicationBuilder application)
{
    application.UseRouting();
}

/// <summary>
/// Configures and uses endpoints for the application.
/// </summary>
/// <param name="application">Web application instance.</param>
/// <returns>Returns void.</returns>
void UseEndpoints(WebApplication application)
{
    application.MapControllers();
}

/// <summary>
/// Configures and uses OpenAPI (Swagger) for the application.
/// </summary>
/// <param name="application">Web application instance.</param>
/// <returns>Returns void.</returns>
void UseOpenApi(IApplicationBuilder application)
{
    application.UseSwagger();
    application.UseSwaggerUI();
}

// Make the implicit Program class public so test projects can access it
public partial class Program
{
}
