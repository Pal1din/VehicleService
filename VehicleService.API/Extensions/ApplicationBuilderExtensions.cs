using FluentMigrator.Runner;
using Serilog;
using VehicleService.Grpc.Services;

namespace VehicleService.API.Extensions;

public static class ApplicationBuilderExtensions
{
    internal static WebApplication ConfigureApplication(this WebApplication app)
    {
        var swaggerUiSettings = app.Configuration.GetSection("SwaggerUi").Get<SwaggerUISettings>();
        app.UseCors(builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
        app.MapPrometheusScrapingEndpoint();
        app.UseSerilogRequestLogging();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint(swaggerUiSettings.Url, swaggerUiSettings.Name);
            c.OAuthClientId(swaggerUiSettings.ClientId);
            c.OAuthClientSecret(swaggerUiSettings.ClientSecret);
            c.OAuthUsePkce();
        });

        app.UseHttpsRedirection(); 
        app.MapControllers();
        app.MapGrpcReflectionService();
        app.MapGrpcService<VehicleGrpcService>().EnableGrpcWeb();
        return app;
    }

    internal static async Task MigrateAndRun(this WebApplication app)
    {
        app.Migrate();
        await app.RunAsync();
    }
    private static void Migrate(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}