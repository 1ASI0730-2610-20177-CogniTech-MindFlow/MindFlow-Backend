using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Mindflow_backend.Migrations
{
    public partial class FixMissingColumnsAndTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use stored procedure to add columns idempotently (MySQL driver treats @ as parameter)
            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS __AddColumnIfNotExists;" +
                "CREATE PROCEDURE __AddColumnIfNotExists(IN tbl VARCHAR(64), IN col VARCHAR(64), IN colDef VARCHAR(500)) " +
                "BEGIN " +
                "  IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = tbl AND COLUMN_NAME = col) THEN " +
                "    SET @ddl = CONCAT('ALTER TABLE `', tbl, '` ADD COLUMN `', col, '` ', colDef); " +
                "    PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt; " +
                "  END IF; " +
                "END;");

            migrationBuilder.Sql("CALL __AddColumnIfNotExists('users', 'google_id', 'VARCHAR(255) NULL');");
            migrationBuilder.Sql("CALL __AddColumnIfNotExists('users', 'name', 'VARCHAR(100) NULL');");
            migrationBuilder.Sql("CALL __AddColumnIfNotExists('users', 'occupation', 'VARCHAR(100) NULL');");
            migrationBuilder.Sql("CALL __AddColumnIfNotExists('journal_entries', 'ai_response', 'TEXT NULL');");

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS __AddColumnIfNotExists;");

            // Index on google_id (idempotent via procedure)
            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS __AddIndexIfNotExists;" +
                "CREATE PROCEDURE __AddIndexIfNotExists(IN tbl VARCHAR(64), IN idx VARCHAR(128), IN idxDef VARCHAR(500)) " +
                "BEGIN " +
                "  IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = tbl AND INDEX_NAME = idx) THEN " +
                "    SET @ddl = idxDef; " +
                "    PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt; " +
                "  END IF; " +
                "END;");

            migrationBuilder.Sql("CALL __AddIndexIfNotExists('users', 'i_x_users_google_id', 'CREATE UNIQUE INDEX i_x_users_google_id ON users (google_id)');");
            migrationBuilder.Sql("CALL __AddIndexIfNotExists('journal_entries', 'i_x_journal_entries_user_id_date', 'CREATE INDEX i_x_journal_entries_user_id_date ON journal_entries (user_id, date)');");

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS __AddIndexIfNotExists;");

            // Tables — CREATE TABLE IF NOT EXISTS (no @ issue)
            migrationBuilder.Sql(
                "CREATE TABLE IF NOT EXISTS password_reset_tokens (" +
                "  id INT AUTO_INCREMENT PRIMARY KEY," +
                "  user_id INT NOT NULL," +
                "  token VARCHAR(64) NOT NULL," +
                "  expires_at DATETIME(6) NOT NULL," +
                "  used TINYINT(1) NOT NULL DEFAULT 0," +
                "  UNIQUE INDEX i_x_password_reset_tokens_token (token)," +
                "  INDEX i_x_password_reset_tokens_user_id (user_id)" +
                ");");

            migrationBuilder.Sql(
                "CREATE TABLE IF NOT EXISTS subscriptions (" +
                "  id INT AUTO_INCREMENT PRIMARY KEY," +
                "  user_id INT NOT NULL," +
                "  plan VARCHAR(20) NOT NULL," +
                "  status VARCHAR(20) NOT NULL," +
                "  stripe_customer_id VARCHAR(100) NULL," +
                "  stripe_subscription_id VARCHAR(100) NULL," +
                "  expires_at DATETIME NULL," +
                "  created_at DATETIME NULL," +
                "  updated_at DATETIME NULL," +
                "  UNIQUE INDEX i_x_subscriptions_user_id (user_id)," +
                "  INDEX i_x_subscriptions_stripe_customer_id (stripe_customer_id)" +
                ");");

            migrationBuilder.Sql(
                "CREATE TABLE IF NOT EXISTS device_tokens (" +
                "  id INT AUTO_INCREMENT PRIMARY KEY," +
                "  user_id INT NOT NULL," +
                "  token VARCHAR(512) NOT NULL," +
                "  platform VARCHAR(20) NOT NULL DEFAULT 'web'," +
                "  created_at DATETIME NULL," +
                "  updated_at DATETIME NULL," +
                "  UNIQUE INDEX i_x_device_tokens_token (token)," +
                "  INDEX i_x_device_tokens_user_id (user_id)" +
                ");");

            migrationBuilder.Sql(
                "CREATE TABLE IF NOT EXISTS support_tickets (" +
                "  id INT AUTO_INCREMENT PRIMARY KEY," +
                "  user_id INT NOT NULL," +
                "  user_email VARCHAR(255) NOT NULL," +
                "  subject VARCHAR(255) NOT NULL," +
                "  message LONGTEXT NOT NULL," +
                "  status VARCHAR(20) NOT NULL DEFAULT 'open'," +
                "  created_at DATETIME(6) NULL," +
                "  updated_at DATETIME(6) NULL," +
                "  INDEX i_x_support_tickets_user_id (user_id)" +
                ");");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS support_tickets;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS device_tokens;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS subscriptions;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS password_reset_tokens;");
            migrationBuilder.Sql("ALTER TABLE journal_entries DROP COLUMN IF EXISTS ai_response;");
            migrationBuilder.Sql("DROP INDEX i_x_users_google_id ON users;");
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS google_id;");
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS name;");
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS occupation;");
        }
    }
}