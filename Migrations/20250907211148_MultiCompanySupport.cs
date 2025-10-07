using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PricePulse.Migrations
{
    /// <inheritdoc />
    public partial class MultiCompanySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_company_profiles_user_id",
                table: "company_profiles");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionToken",
                table: "sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "registration_verifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "registration_verifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "registration_verifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VerificationCode",
                table: "registration_verifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedDate",
                table: "registration_verifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompetitorProductId",
                table: "prices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "prices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductPrice",
                table: "prices",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompetitorWebsiteUrl",
                table: "competitors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompetitorProductId",
                table: "competitor_products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CompanyDescription",
                table: "company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "industry",
                table: "company_profiles",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AuthenticationId",
                table: "authentications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SecurityAnswer",
                table: "authentications",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityQuestion",
                table: "authentications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateProvince",
                table: "addresses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "addresses",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_profiles_user_id",
                table: "company_profiles",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_company_profiles_user_id",
                table: "company_profiles");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "SessionToken",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "registration_verifications");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "registration_verifications");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "registration_verifications");

            migrationBuilder.DropColumn(
                name: "VerificationCode",
                table: "registration_verifications");

            migrationBuilder.DropColumn(
                name: "VerifiedDate",
                table: "registration_verifications");

            migrationBuilder.DropColumn(
                name: "CompetitorProductId",
                table: "prices");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "prices");

            migrationBuilder.DropColumn(
                name: "ProductPrice",
                table: "prices");

            migrationBuilder.DropColumn(
                name: "CompetitorWebsiteUrl",
                table: "competitors");

            migrationBuilder.DropColumn(
                name: "CompetitorProductId",
                table: "competitor_products");

            migrationBuilder.DropColumn(
                name: "CompanyDescription",
                table: "company_profiles");

            migrationBuilder.DropColumn(
                name: "industry",
                table: "company_profiles");

            migrationBuilder.DropColumn(
                name: "AuthenticationId",
                table: "authentications");

            migrationBuilder.DropColumn(
                name: "SecurityAnswer",
                table: "authentications");

            migrationBuilder.DropColumn(
                name: "SecurityQuestion",
                table: "authentications");

            migrationBuilder.DropColumn(
                name: "StateProvince",
                table: "addresses");

            migrationBuilder.DropColumn(
                name: "country",
                table: "addresses");

            migrationBuilder.CreateIndex(
                name: "IX_company_profiles_user_id",
                table: "company_profiles",
                column: "user_id",
                unique: true);
        }
    }
}
