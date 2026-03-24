using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace task1.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Roles", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Claims", x => x.Id));

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => new { x.RoleId, x.ClaimId });
                    table.ForeignKey(
                        name: "FK_RoleClaims_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                INSERT INTO ""Roles"" (""Id"", ""Name"") VALUES (1, 'Admin'), (2, 'General Manager'), (3, 'Social Media Manager');
                INSERT INTO ""Claims"" (""Id"", ""Name"") VALUES (1, 'ManageUsers'), (2, 'ManageCustomers'), (3, 'ManageCars'), (4, 'ManagePosts'), (5, 'ViewDashboard');
                INSERT INTO ""RoleClaims"" (""RoleId"", ""ClaimId"") VALUES (1,1),(1,2),(1,3),(1,4),(1,5),(2,2),(2,3),(2,4),(2,5),(3,4),(3,5);
            ");

            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Roles\"', 'Id'), 3);");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Claims\"', 'Id'), 5);");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='Users' AND column_name='Role') THEN
                        UPDATE ""Users"" SET ""RoleId"" = 1 WHERE ""Role"" = 'Admin';
                        UPDATE ""Users"" SET ""RoleId"" = 2 WHERE ""Role"" = 'General Manager';
                        UPDATE ""Users"" SET ""RoleId"" = 3 WHERE ""Role"" = 'Social Media Manager';
                        UPDATE ""Users"" SET ""RoleId"" = 1 WHERE ""RoleId"" IS NULL;
                        ALTER TABLE ""Users"" DROP COLUMN ""Role"";
                    END IF;
                    UPDATE ""Users"" SET ""RoleId"" = 1 WHERE ""RoleId"" IS NULL;
                END $$;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_ClaimId",
                table: "RoleClaims",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Users_Roles_RoleId", table: "Users");
            migrationBuilder.DropIndex(name: "IX_Users_RoleId", table: "Users");
            migrationBuilder.DropTable(name: "RoleClaims");
            migrationBuilder.DropTable(name: "Claims");
            migrationBuilder.DropTable(name: "Roles");
            migrationBuilder.AddColumn<string>(name: "Role", table: "Users", type: "text", nullable: false, defaultValue: "Admin");
        }
    }
}
