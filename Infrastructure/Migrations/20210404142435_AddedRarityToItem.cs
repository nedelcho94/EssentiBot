using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Migrations
{
    public partial class AddedRarityToItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<decimal>(
            //    name: "UserId",
            //    table: "UserProfiles",
            //    nullable: false,
            //    oldClrType: typeof(decimal),
            //    oldType: "decimal(20,0)")
            //    .OldAnnotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "ServerId",
            //    table: "Servers",
            //    nullable: false,
            //    oldClrType: typeof(decimal),
            //    oldType: "decimal(20,0)")
            //    .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Rarity",
                table: "Items",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rarity",
                table: "Items");

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "UserId",
            //    table: "UserProfiles",
            //    type: "decimal(20,0)",
            //    nullable: false,
            //    oldClrType: typeof(decimal))
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "ServerId",
            //    table: "Servers",
            //    type: "decimal(20,0)",
            //    nullable: false,
            //    oldClrType: typeof(decimal))
            //    .Annotation("SqlServer:Identity", "1, 1");
        }
    }
}
