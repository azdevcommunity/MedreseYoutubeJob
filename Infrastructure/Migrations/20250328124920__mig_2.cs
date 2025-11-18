using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YoutubeApiSynchronize.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _mig_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "esm",
                table: "videos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                schema: "esm",
                table: "videos");
        }
    }
}
