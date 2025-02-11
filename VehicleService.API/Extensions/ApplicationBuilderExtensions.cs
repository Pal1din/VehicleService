using FluentMigrator.Runner;

namespace VehicleService.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void Migrate(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}