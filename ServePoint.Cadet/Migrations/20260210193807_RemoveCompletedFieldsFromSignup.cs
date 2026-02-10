using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServePoint.Cadet.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompletedFieldsFromSignup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AttendanceApproved",
                table: "VolunteerSignups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttendanceApprovedAt",
                table: "VolunteerSignups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendanceApprovedByUserId",
                table: "VolunteerSignups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AttendanceSubmitted",
                table: "VolunteerSignups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttendanceSubmittedAt",
                table: "VolunteerSignups",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceApproved",
                table: "VolunteerSignups");

            migrationBuilder.DropColumn(
                name: "AttendanceApprovedAt",
                table: "VolunteerSignups");

            migrationBuilder.DropColumn(
                name: "AttendanceApprovedByUserId",
                table: "VolunteerSignups");

            migrationBuilder.DropColumn(
                name: "AttendanceSubmitted",
                table: "VolunteerSignups");

            migrationBuilder.DropColumn(
                name: "AttendanceSubmittedAt",
                table: "VolunteerSignups");
        }
    }
}
