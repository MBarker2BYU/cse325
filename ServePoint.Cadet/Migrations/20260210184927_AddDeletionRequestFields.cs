using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServePoint.Cadet.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletionRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionRequestedAt",
                table: "VolunteerOpportunities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletionRequestedByUserId",
                table: "VolunteerOpportunities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletionRequested",
                table: "VolunteerOpportunities",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletionRequestedAt",
                table: "VolunteerOpportunities");

            migrationBuilder.DropColumn(
                name: "DeletionRequestedByUserId",
                table: "VolunteerOpportunities");

            migrationBuilder.DropColumn(
                name: "IsDeletionRequested",
                table: "VolunteerOpportunities");
        }
    }
}
