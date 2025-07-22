using EventManagementSystem.API.Extensions;
using EventManagementSystem.Application;
using EventManagementSystem.Identity;
using EventManagementSystem.Persistence;
using Serilog;

internal class Program
{
    private static async Task Main(string[] args)




    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Seq(context.Configuration.GetConnectionString("Seq") ?? "http://localhost:5341");
        });

        // Add services to the container
        builder.Services.AddApplication();
        builder.Services.AddPersistence(builder.Configuration);
        builder.Services.AddIdentityServices(builder.Configuration);

        // Add API services
        builder.Services.AddApiServices(builder.Configuration);
        builder.Services.AddAuthenticationServices(builder.Configuration);
        builder.Services.AddSwaggerServices();
        builder.Services.AddSignalRServices();
        //builder.Services.AddHealthCheckServices(builder.Configuration);
        builder.Services.AddRateLimitingServices();

        var app = builder.Build();

        // Configure the HTTP request pipeline
        await app.ConfigureApplicationAsync();

        // Initialize database and identity
        using (var scope = app.Services.CreateScope())
        {
            await scope.ServiceProvider.InitializeDatabaseAsync();
            await scope.ServiceProvider.InitializeIdentityAsync();
        }

        app.Run();
    }
}