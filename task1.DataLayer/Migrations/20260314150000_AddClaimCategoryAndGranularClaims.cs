using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace task1.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimCategoryAndGranularClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Claims",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
                DELETE FROM ""RoleClaims"";
                DELETE FROM ""Claims"";

                INSERT INTO ""Claims"" (""Id"", ""Name"", ""Category"") VALUES
                (1, 'ViewUsers', 'Users'),
                (2, 'SearchUsers', 'Users'),
                (3, 'ViewUser', 'Users'),
                (4, 'CreateUser', 'Users'),
                (5, 'EditUser', 'Users'),
                (6, 'DeleteUser', 'Users'),
                (7, 'ChangeUserRole', 'Users'),
                (8, 'ViewCustomers', 'Customers'),
                (9, 'SearchCustomers', 'Customers'),
                (10, 'ViewCustomer', 'Customers'),
                (11, 'CreateCustomer', 'Customers'),
                (12, 'EditCustomer', 'Customers'),
                (13, 'DeleteCustomer', 'Customers'),
                (14, 'ViewCars', 'Cars'),
                (15, 'ViewCar', 'Cars'),
                (16, 'CreateCar', 'Cars'),
                (17, 'EditCar', 'Cars'),
                (18, 'DeleteCar', 'Cars'),
                (19, 'ViewPosts', 'Posts'),
                (20, 'ViewPost', 'Posts'),
                (21, 'CreatePost', 'Posts'),
                (22, 'EditPost', 'Posts'),
                (23, 'DeletePost', 'Posts'),
                (24, 'ViewDashboard', 'Dashboard'),
                (25, 'ViewNotifications', 'Notifications'),
                (26, 'DeleteNotifications', 'Notifications');

                INSERT INTO ""RoleClaims"" (""RoleId"", ""ClaimId"") VALUES
                (1,1),(1,2),(1,3),(1,4),(1,5),(1,6),(1,7),(1,8),(1,9),(1,10),(1,11),(1,12),(1,13),(1,14),(1,15),(1,16),(1,17),(1,18),(1,19),(1,20),(1,21),(1,22),(1,23),(1,24),(1,25),(1,26),
                (2,8),(2,9),(2,10),(2,11),(2,12),(2,13),(2,14),(2,15),(2,16),(2,17),(2,18),(2,19),(2,20),(2,21),(2,22),(2,23),(2,24),(2,25),(2,26),
                (3,19),(3,20),(3,21),(3,22),(3,23),(3,24),(3,25),(3,26);

                SELECT setval(pg_get_serial_sequence('""Claims""', 'Id'), 26);
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ""RoleClaims"";
                DELETE FROM ""Claims"";

                INSERT INTO ""Claims"" (""Id"", ""Name"", ""Category"") VALUES
                (1, 'ManageUsers', ''), (2, 'ManageCustomers', ''), (3, 'ManageCars', ''), (4, 'ManagePosts', ''), (5, 'ViewDashboard', '');

                INSERT INTO ""RoleClaims"" (""RoleId"", ""ClaimId"") VALUES
                (1,1),(1,2),(1,3),(1,4),(1,5),(2,2),(2,3),(2,4),(2,5),(3,4),(3,5);

                SELECT setval(pg_get_serial_sequence('""Claims""', 'Id'), 5);
            ");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Claims");
        }
    }
}
