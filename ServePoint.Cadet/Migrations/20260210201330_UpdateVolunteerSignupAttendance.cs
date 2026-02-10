using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServePoint.Cadet.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVolunteerSignupAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceApprovedByUserId",
                table: "VolunteerSignups");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "VolunteerSignups");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "VolunteerSignups",
                newName: "ApprovedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApprovedByUserId",
                table: "VolunteerSignups",
                newName: "CompletedAt");

            migrationBuilder.AddColumn<string>(
                name: "AttendanceApprovedByUserId",
                table: "VolunteerSignups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "VolunteerSignups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
