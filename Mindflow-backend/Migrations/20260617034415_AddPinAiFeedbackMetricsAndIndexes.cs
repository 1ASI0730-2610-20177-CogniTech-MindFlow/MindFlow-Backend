using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Mindflow_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPinAiFeedbackMetricsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "pin_hash",
                table: "users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "content",
                table: "journal_entries",
                type: "LONGTEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.CreateTable(
                name: "ai_feedback_ratings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    content_id = table.Column<int>(type: "int", nullable: false),
                    content_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ai_feedback_ratings", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ai_metric_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    operation = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    latency_ms = table.Column<int>(type: "int", nullable: false),
                    success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    prompt_length = table.Column<int>(type: "int", nullable: false),
                    response_length = table.Column<int>(type: "int", nullable: false),
                    error_message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ai_metric_logs", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "i_x_habits_user_id_status",
                table: "habits",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "i_x_habit_completion_logs_date",
                table: "habit_completion_logs",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "i_x_habit_completion_logs_habit_id_date",
                table: "habit_completion_logs",
                columns: new[] { "habit_id", "date" });

            migrationBuilder.CreateIndex(
                name: "i_x_ai_feedback_ratings_user_id",
                table: "ai_feedback_ratings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ai_feedback_ratings_user_id_content_id_content_type",
                table: "ai_feedback_ratings",
                columns: new[] { "user_id", "content_id", "content_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_ai_metric_logs_created_at",
                table: "ai_metric_logs",
                column: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_feedback_ratings");

            migrationBuilder.DropTable(
                name: "ai_metric_logs");

            migrationBuilder.DropIndex(
                name: "i_x_habits_user_id_status",
                table: "habits");

            migrationBuilder.DropIndex(
                name: "i_x_habit_completion_logs_date",
                table: "habit_completion_logs");

            migrationBuilder.DropIndex(
                name: "i_x_habit_completion_logs_habit_id_date",
                table: "habit_completion_logs");

            migrationBuilder.DropColumn(
                name: "pin_hash",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "content",
                table: "journal_entries",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "LONGTEXT");
        }
    }
}
