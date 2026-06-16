using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Mindflow_backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "longtext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_users", x => x.id);
                })
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
                name: "journal_entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    title = table.Column<string>(type: "longtext", nullable: false),
                    content = table.Column<string>(type: "longtext", nullable: false),
                    sentiment = table.Column<string>(type: "longtext", nullable: false),
                    category = table.Column<string>(type: "longtext", nullable: false),
                    has_preview = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_journal_entries", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_tags", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "analytics_caches",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    week_start = table.Column<DateOnly>(type: "date", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    trend_percentage = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    ai_insight = table.Column<string>(type: "text", nullable: true),
                    ai_insight_localized = table.Column<string>(type: "text", nullable: true),
                    kpis = table.Column<string>(type: "json", nullable: true),
                    fluctuation_data = table.Column<string>(type: "json", nullable: true),
                    trend_data = table.Column<string>(type: "json", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_analytics_caches", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "word_clouds",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    words = table.Column<string>(type: "json", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_word_clouds", x => x.id);
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

            migrationBuilder.CreateTable(
                name: "entry_tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    entry_id = table.Column<int>(type: "int", nullable: false),
                    tag_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_entry_tags", x => x.id);
                    table.ForeignKey(
                        name: "f_k_entry_tags_journal_entries_entry_id",
                        column: x => x.entry_id,
                        principalTable: "journal_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_entry_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    entry_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "longtext", nullable: false),
                    url = table.Column<string>(type: "longtext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_media", x => x.id);
                    table.ForeignKey(
                        name: "f_k_media_journal_entries_entry_id",
                        column: x => x.entry_id,
                        principalTable: "journal_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "i_x_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_habits_user_id",
                table: "habits",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_habit_completion_logs_habit_id",
                table: "habit_completion_logs",
                column: "habit_id");

            migrationBuilder.CreateIndex(
                name: "i_x_entry_tags_entry_id_tag_id",
                table: "entry_tags",
                columns: new[] { "entry_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_entry_tags_tag_id",
                table: "entry_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "i_x_media_entry_id",
                table: "media",
                column: "entry_id");

            migrationBuilder.CreateIndex(
                name: "i_x_analytics_caches_user_id_week_start",
                table: "analytics_caches",
                columns: new[] { "user_id", "week_start" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_word_clouds_user_id",
                table: "word_clouds",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "entry_tags");
            migrationBuilder.DropTable(name: "media");
            migrationBuilder.DropTable(name: "habit_completion_logs");
            migrationBuilder.DropTable(name: "analytics_caches");
            migrationBuilder.DropTable(name: "word_clouds");
            migrationBuilder.DropTable(name: "journal_entries");
            migrationBuilder.DropTable(name: "tags");
            migrationBuilder.DropTable(name: "habits");
            migrationBuilder.DropTable(name: "users");
        }
    }
}
