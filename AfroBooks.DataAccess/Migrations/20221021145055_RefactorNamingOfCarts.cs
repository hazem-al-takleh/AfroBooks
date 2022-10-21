using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfroBooks.Migrations
{
    public partial class RefactorNamingOfCarts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingCartProducts");

            migrationBuilder.CreateTable(
                name: "CartProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartProducts_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartProducts_ApplicationUserId",
                table: "CartProducts",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartProducts_ProductId",
                table: "CartProducts",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartProducts");

            migrationBuilder.CreateTable(
                name: "ShoppingCartProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCartProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingCartProducts_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingCartProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartProducts_ApplicationUserId",
                table: "ShoppingCartProducts",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartProducts_ProductId",
                table: "ShoppingCartProducts",
                column: "ProductId");
        }
    }
}
