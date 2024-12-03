using Microsoft.EntityFrameworkCore.Migrations;

namespace myapp.Data.Migrations;

public class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Queries",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                SqlQuery = table.Column<string>(type: "TEXT", nullable: false),
                Result = table.Column<string>(type: "TEXT", nullable: false),
                ExecutedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Queries");
    }
} 