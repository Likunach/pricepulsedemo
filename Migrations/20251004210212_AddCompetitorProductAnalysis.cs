using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PricePulse.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitorProductAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompetitorProductId",
                table: "competitor_products");

            migrationBuilder.AlterColumn<int>(
                name: "own_product_id",
                table: "competitor_products",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "competitor_id",
                table: "competitor_products",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "c_product_website_url",
                table: "competitor_products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "c_product_name",
                table: "competitor_products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "competitor_product_analyses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    competitor_domain = table.Column<string>(type: "text", nullable: false),
                    product_name = table.Column<string>(type: "text", nullable: false),
                    product_description = table.Column<string>(type: "text", nullable: true),
                    competitor_price = table.Column<string>(type: "text", nullable: true),
                    competitor_currency = table.Column<string>(type: "text", nullable: true),
                    product_category = table.Column<string>(type: "text", nullable: true),
                    product_image_url = table.Column<string>(type: "text", nullable: true),
                    competitor_product_url = table.Column<string>(type: "text", nullable: true),
                    discovered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competitor_product_analyses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "competitor_product_retailers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    competitor_product_analysis_id = table.Column<int>(type: "integer", nullable: false),
                    retailer_name = table.Column<string>(type: "text", nullable: false),
                    retailer_url = table.Column<string>(type: "text", nullable: true),
                    product_url = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    availability = table.Column<string>(type: "text", nullable: true),
                    shipping_info = table.Column<string>(type: "text", nullable: true),
                    rating = table.Column<string>(type: "text", nullable: true),
                    reviews_count = table.Column<string>(type: "text", nullable: true),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competitor_product_retailers", x => x.id);
                    table.ForeignKey(
                        name: "FK_competitor_product_retailers_competitor_product_analyses_co~",
                        column: x => x.competitor_product_analysis_id,
                        principalTable: "competitor_product_analyses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_competitor_product_retailers_competitor_product_analysis_id",
                table: "competitor_product_retailers",
                column: "competitor_product_analysis_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "competitor_product_retailers");

            migrationBuilder.DropTable(
                name: "competitor_product_analyses");

            migrationBuilder.AlterColumn<int>(
                name: "own_product_id",
                table: "competitor_products",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "competitor_id",
                table: "competitor_products",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "c_product_website_url",
                table: "competitor_products",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "c_product_name",
                table: "competitor_products",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompetitorProductId",
                table: "competitor_products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
