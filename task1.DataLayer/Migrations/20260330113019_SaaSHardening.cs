using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace task1.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class SaaSHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Tenants""
                ALTER COLUMN ""Status"" TYPE boolean
                USING CASE
                    WHEN lower(coalesce(""Status"", '')) = 'active' THEN true
                    WHEN lower(coalesce(""Status"", '')) IN ('true','1','yes') THEN true
                    ELSE false
                END;
            ");

            migrationBuilder.Sql(@"
                DELETE FROM ""RoleClaims""
                WHERE ""RoleId"" IN (
                    SELECT r.""Id""
                    FROM ""Roles"" r
                    LEFT JOIN ""Users"" u ON u.""RoleId"" = r.""Id""
                    WHERE r.""Name"" IN ('General Manager', 'Social Media Manager')
                    GROUP BY r.""Id""
                    HAVING COUNT(u.""Id"") = 0
                );

                DELETE FROM ""Roles""
                WHERE ""Name"" IN ('General Manager', 'Social Media Manager')
                  AND ""Id"" NOT IN (SELECT DISTINCT ""RoleId"" FROM ""Users"");

                DELETE FROM ""Claims""
                WHERE ""Name"" IN ('ManageUsers', 'ManageCustomers', 'ManageCars', 'ManagePosts')
                  AND ""Id"" NOT IN (SELECT DISTINCT ""ClaimId"" FROM ""RoleClaims"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Tenants""
                ALTER COLUMN ""Status"" TYPE text
                USING CASE
                    WHEN ""Status"" = true THEN 'Active'
                    ELSE 'Inactive'
                END;
            ");
        }
    }
}
