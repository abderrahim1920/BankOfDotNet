using Microsoft.EntityFrameworkCore.Migrations;

namespace BankOfDotNet.API.Migrations
{
    public partial class addAdresstoCustomers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "Customers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "Customers");
        }
    }
}
