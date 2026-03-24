using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace task1.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomerPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Customers never had PasswordHash in this schema - only drop if it exists (PostgreSQL)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'Customers' AND column_name = 'PasswordHash'
                    ) THEN
                        ALTER TABLE ""Customers"" DROP COLUMN ""PasswordHash"";
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
