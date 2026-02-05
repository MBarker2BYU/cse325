using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServePoint.Cadet.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VolunteerOpportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Hours = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerOpportunities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerSignups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VolunteerOpportunityId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    SignedUpAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerSignups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerSignups_VolunteerOpportunities_VolunteerOpportunityId",
                        column: x => x.VolunteerOpportunityId,
                        principalTable: "VolunteerOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerSignups_VolunteerOpportunityId_UserId",
                table: "VolunteerSignups",
                columns: new[] { "VolunteerOpportunityId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VolunteerSignups");

            migrationBuilder.DropTable(
                name: "VolunteerOpportunities");
        }
    }
}
