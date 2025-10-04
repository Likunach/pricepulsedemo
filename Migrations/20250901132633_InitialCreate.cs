using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PricePulse.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    registration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_login_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    account_status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "authentications",
                columns: table => new
                {
                    auth_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    recovery_email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    security_questions = table.Column<string>(type: "json", nullable: true),
                    last_password_change = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authentications", x => x.auth_id);
                    table.ForeignKey(
                        name: "FK_authentications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_profiles",
                columns: table => new
                {
                    company_profile_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    company_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    company_profile = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    summary = table.Column<string>(type: "text", nullable: true),
                    company_website = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_profiles", x => x.company_profile_id);
                    table.ForeignKey(
                        name: "FK_company_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contacts",
                columns: table => new
                {
                    contact_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    secondary_email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    preferred_contact_method = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contacts", x => x.contact_id);
                    table.ForeignKey(
                        name: "FK_contacts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    profile_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    last_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    bio = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.profile_id);
                    table.ForeignKey(
                        name: "FK_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "registration_verifications",
                columns: table => new
                {
                    verification_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    verification_token = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    token_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verification_status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registration_verifications", x => x.verification_id);
                    table.ForeignKey(
                        name: "FK_registration_verifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    session_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    login_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ip_address = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true),
                    device_info = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_role_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    assigned_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.user_role_id);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    address_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    company_profile_id = table.Column<int>(type: "integer", nullable: false),
                    address_line1 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    address_line2 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    city = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    state = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    postal_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.address_id);
                    table.ForeignKey(
                        name: "FK_addresses_company_profiles_company_profile_id",
                        column: x => x.company_profile_id,
                        principalTable: "company_profiles",
                        principalColumn: "company_profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "competitors",
                columns: table => new
                {
                    competitor_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    company_profile_id = table.Column<int>(type: "integer", nullable: false),
                    competitor_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    company_profile = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    competitor_website = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competitors", x => x.competitor_id);
                    table.ForeignKey(
                        name: "FK_competitors_company_profiles_company_profile_id",
                        column: x => x.company_profile_id,
                        principalTable: "company_profiles",
                        principalColumn: "company_profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "own_products",
                columns: table => new
                {
                    own_product_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    company_profile_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    product_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    product_website_url = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_own_products", x => x.own_product_id);
                    table.ForeignKey(
                        name: "FK_own_products_company_profiles_company_profile_id",
                        column: x => x.company_profile_id,
                        principalTable: "company_profiles",
                        principalColumn: "company_profile_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_own_products_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "competitor_products",
                columns: table => new
                {
                    comp_product_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    own_product_id = table.Column<int>(type: "integer", nullable: false),
                    competitor_id = table.Column<int>(type: "integer", nullable: false),
                    c_product_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    c_product_website_url = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competitor_products", x => x.comp_product_id);
                    table.ForeignKey(
                        name: "FK_competitor_products_competitors_competitor_id",
                        column: x => x.competitor_id,
                        principalTable: "competitors",
                        principalColumn: "competitor_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_competitor_products_own_products_own_product_id",
                        column: x => x.own_product_id,
                        principalTable: "own_products",
                        principalColumn: "own_product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prices",
                columns: table => new
                {
                    price_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    own_product_id = table.Column<int>(type: "integer", nullable: true),
                    comp_product_id = table.Column<int>(type: "integer", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    price_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prices", x => x.price_id);
                    table.ForeignKey(
                        name: "FK_prices_competitor_products_comp_product_id",
                        column: x => x.comp_product_id,
                        principalTable: "competitor_products",
                        principalColumn: "comp_product_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prices_own_products_own_product_id",
                        column: x => x.own_product_id,
                        principalTable: "own_products",
                        principalColumn: "own_product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_addresses_company_profile_id",
                table: "addresses",
                column: "company_profile_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_authentications_user_id",
                table: "authentications",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_profiles_user_id",
                table: "company_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_competitor_products_competitor_id",
                table: "competitor_products",
                column: "competitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_competitor_products_own_product_id",
                table: "competitor_products",
                column: "own_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_competitors_company_profile_id",
                table: "competitors",
                column: "company_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_user_id",
                table: "contacts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_own_products_company_profile_id",
                table: "own_products",
                column: "company_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_own_products_user_id",
                table: "own_products",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_prices_comp_product_id",
                table: "prices",
                column: "comp_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_prices_own_product_id",
                table: "prices",
                column: "own_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_user_id",
                table: "profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registration_verifications_user_id",
                table: "registration_verifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_user_id",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "authentications");

            migrationBuilder.DropTable(
                name: "contacts");

            migrationBuilder.DropTable(
                name: "prices");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "registration_verifications");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "competitor_products");

            migrationBuilder.DropTable(
                name: "competitors");

            migrationBuilder.DropTable(
                name: "own_products");

            migrationBuilder.DropTable(
                name: "company_profiles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
