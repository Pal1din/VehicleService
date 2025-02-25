using FluentMigrator;

namespace VehicleService.Data.Migrations;
[Migration(20250226)]
public class AddOrganizationsMigration: Migration
{
    public override void Up()
    {
        Alter.Table("vehicles").AddColumn("organization_id").AsInt64().Nullable();
    }

    public override void Down()
    {
        Execute.Sql("ALTER TABLE vehicle drop column organization_id");
    }
}