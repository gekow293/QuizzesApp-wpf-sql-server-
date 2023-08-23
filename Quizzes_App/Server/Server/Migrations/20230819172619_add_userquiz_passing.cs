using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testing_Server.Migrations
{
    public partial class add_userquiz_passing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Passing",
                table: "UserQuizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Passing",
                table: "UserQuizzes");
        }
    }
}
