using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace task1.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class EnsureUsersPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename Email to PhoneNumber in Users if Email still exists (idempotent)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'Users' AND column_name = 'Email'
                    ) THEN
                        ALTER TABLE ""Users"" RENAME COLUMN ""Email"" TO ""PhoneNumber"";
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'Users' AND column_name = 'PhoneNumber'
                    ) THEN
                        ALTER TABLE ""Users"" RENAME COLUMN ""PhoneNumber"" TO ""Email"";
                    END IF;
                END $$;
            ");
        }
    }
}
