using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServePoint.Cadet.Migrations
{
    /// <inheritdoc />
    public partial class FixIdentityBoolsForPostgres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql("""
                                         ALTER TABLE "AspNetUsers"
                                         ALTER COLUMN "EmailConfirmed" TYPE boolean
                                         USING ("EmailConfirmed" <> 0);

                                         ALTER TABLE "AspNetUsers"
                                         ALTER COLUMN "PhoneNumberConfirmed" TYPE boolean
                                         USING ("PhoneNumberConfirmed" <> 0);

                                         ALTER TABLE "AspNetUsers"
                                         ALTER COLUMN "TwoFactorEnabled" TYPE boolean
                                         USING ("TwoFactorEnabled" <> 0);

                                         ALTER TABLE "AspNetUsers"
                                         ALTER COLUMN "LockoutEnabled" TYPE boolean
                                         USING ("LockoutEnabled" <> 0);
                                     """);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql("""
                                         ALTER TABLE "AspNetUsers"
                                         ALTER COLUMN "EmailConfirmed" TYPE integer
                                         USING (CASE WHEN "EmailConfirmed" THEN 1 ELSE 0 END);

                                         ALTER TABLE "AspNetUsers"
                                         ALTER COLUMN "PhoneNumberConfirmed" TYPE integer
                                         USING (CASE WHEN "PhoneNumberConfirmed" THEN 1 ELSE 0 END);

                                         ALTER TABLE "AspNetUsers"
                                         ALTER COLUMN "TwoFactorEnabled" TYPE integer
                                         USING (CASE WHEN "TwoFactorEnabled" THEN 1 ELSE 0 END);

                                         ALTER TABLE "AspNetUsers"
                                         ALTER COLUMN "LockoutEnabled" TYPE integer
                                         USING (CASE WHEN "LockoutEnabled" THEN 1 ELSE 0 END);
                                     """);
            }
        }

    }
}
