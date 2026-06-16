using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Mindflow_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitsBoundedContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "habits",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    frequency = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    streak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    paused_by_ai = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_habits", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "habit_completion_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    habit_id = table.Column<int>(type: "int", nullable: false),
                    habit_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    completed = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_habit_completion_logs", x => x.id);
                    table.ForeignKey(
                        name: "f_k_habit_completion_logs_habits_habit_id",
                        column: x => x.habit_id,
                        principalTable: "habits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "i_x_habit_completion_logs_habit_id",
                table: "habit_completion_logs",
                column: "habit_id");

            migrationBuilder.CreateIndex(
                name: "i_x_habits_user_id",
                table: "habits",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "habit_completion_logs");

            migrationBuilder.DropTable(
                name: "habits");
        }
    }
}
