using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServePoint.Cadet.Migrations
{
    /// <inheritdoc />
    public partial class FixAppBoolColumnsForPostgres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql("""
                                         ALTER TABLE "VolunteerOpportunities"
                                         ALTER COLUMN "IsApproved" TYPE boolean
                                         USING (COALESCE("IsApproved", 0) <> 0);

                                         ALTER TABLE "VolunteerOpportunities"
                                         ALTER COLUMN "IsDeletionRequested" TYPE boolean
                                         USING (COALESCE("IsDeletionRequested", 0) <> 0);

                                         ALTER TABLE "VolunteerSignups"
                                         ALTER COLUMN "AttendanceApproved" TYPE boolean
                                         USING (COALESCE("AttendanceApproved", 0) <> 0);
                                     """);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql("""
                                         ALTER TABLE "VolunteerOpportunities"
                                         ALTER COLUMN "IsApproved" TYPE integer
                                         USING (CASE WHEN "IsApproved" THEN 1 ELSE 0 END);

                                         ALTER TABLE "VolunteerOpportunities"
                                         ALTER COLUMN "IsDeletionRequested" TYPE integer
                                         USING (CASE WHEN "IsDeletionRequested" THEN 1 ELSE 0 END);

                                         ALTER TABLE "VolunteerSignups"
                                         ALTER COLUMN "AttendanceApproved" TYPE integer
                                         USING (CASE WHEN "AttendanceApproved" THEN 1 ELSE 0 END);
                                     """);
            }
        }
    }
}
