using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HITs.Internship.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDiaryTemplatesOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Order",
                table: "DiaryTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "DiaryTemplates");
        }
    }
}
