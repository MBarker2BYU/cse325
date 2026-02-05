using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServePoint.Cadet.Migrations
{
    /// <inheritdoc />
    public partial class AddOpportunityApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "VolunteerOpportunities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "VolunteerOpportunities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "VolunteerOpportunities",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "VolunteerOpportunities");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "VolunteerOpportunities");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "VolunteerOpportunities");
        }
    }
}
