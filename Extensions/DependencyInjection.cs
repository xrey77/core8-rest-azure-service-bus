// Extensions/DependencyInjection.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
// using Azure.Messaging.ServiceBus.
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
// using Microsoft.Extensions.Azure;
// using Azure.Messaging.ServiceBus;
// using Azure.Identity;
using core8_rest_azure_service_bus.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core Web API Dependencies
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // string azure_connectionString = configuration.GetConnectionString("AzureServiceBus") 
        //             ?? throw new InvalidOperationException("Connection string 'AzureServiceBus' not found.");

    
        // 2. Register the Azure Service Bus Client
        // services.AddAzureClients(clientBuilder =>
        // {
        //     clientBuilder.AddServiceBusClient(azure_connectionString);
        // });



        // Infrastructure & Database
        string connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        services.AddScoped<IDbConnection>(sp => new SqlConnection(connectionString));
        services.AddScoped<DbInitializer>();

        // Custom Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();        
        // services.AddScoped<IJWTServiceManage, JWTServiceManage>();

        return services;
    }

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        return services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyHeader()
                      .WithMethods("GET", "POST", "DELETE", "PATCH", "OPTIONS")
                      .WithExposedHeaders("Content-Disposition")
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });
    }

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Safely fetch JWT settings from appsettings.json
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing.");
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero // Removes the default 5-minute grace period
                };
            });

        services.AddAuthorization();

        return services;
    }
}
