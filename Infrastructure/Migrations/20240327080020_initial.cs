using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectricityPriceDatas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "CHAR(36)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    StartDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Price = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectricityPriceDatas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectricityPriceDatas");
        }
    }
}
