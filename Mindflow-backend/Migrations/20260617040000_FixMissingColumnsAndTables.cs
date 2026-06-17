using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mindflow_backend.Migrations
{
    public partial class FixMissingColumnsAndTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: only add columns/tables if they don't exist yet

            // users: google_id, name, occupation
            migrationBuilder.Sql("""
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'users' AND COLUMN_NAME = 'google_id');
                SET @sql = IF(@col_exists = 0, 'ALTER TABLE users ADD COLUMN google_id VARCHAR(255) NULL', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            """);
            migrationBuilder.Sql("""
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'users' AND COLUMN_NAME = 'name');
                SET @sql = IF(@col_exists = 0, 'ALTER TABLE users ADD COLUMN name VARCHAR(100) NULL', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            """);
            migrationBuilder.Sql("""
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'users' AND COLUMN_NAME = 'occupation');
                SET @sql = IF(@col_exists = 0, 'ALTER TABLE users ADD COLUMN occupation VARCHAR(100) NULL', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            """);
            migrationBuilder.Sql("""
                SET @idx_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'users' AND INDEX_NAME = 'i_x_users_google_id');
                SET @sql = IF(@idx_exists = 0, 'CREATE UNIQUE INDEX i_x_users_google_id ON users (google_id)', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            """);

            // journal_entries: ai_response
            migrationBuilder.Sql("""
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'journal_entries' AND COLUMN_NAME = 'ai_response');
                SET @sql = IF(@col_exists = 0, 'ALTER TABLE journal_entries ADD COLUMN ai_response TEXT NULL', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            """);

            // password_reset_tokens
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS password_reset_tokens (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL,
                    token VARCHAR(64) NOT NULL,
                    expires_at DATETIME(6) NOT NULL,
                    used TINYINT(1) NOT NULL DEFAULT 0,
                    UNIQUE INDEX i_x_password_reset_tokens_token (token),
                    INDEX i_x_password_reset_tokens_user_id (user_id)
                );
            """);

            // subscriptions
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS subscriptions (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL,
                    plan VARCHAR(20) NOT NULL,
                    status VARCHAR(20) NOT NULL,
                    stripe_customer_id VARCHAR(100) NULL,
                    stripe_subscription_id VARCHAR(100) NULL,
                    expires_at DATETIME NULL,
                    created_at DATETIME NULL,
                    updated_at DATETIME NULL,
                    UNIQUE INDEX i_x_subscriptions_user_id (user_id),
                    INDEX i_x_subscriptions_stripe_customer_id (stripe_customer_id)
                );
            """);

            // device_tokens
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS device_tokens (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL,
                    token VARCHAR(512) NOT NULL,
                    platform VARCHAR(20) NOT NULL DEFAULT 'web',
                    created_at DATETIME NULL,
                    updated_at DATETIME NULL,
                    UNIQUE INDEX i_x_device_tokens_token (token),
                    INDEX i_x_device_tokens_user_id (user_id)
                );
            """);

            // support_tickets
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS support_tickets (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL,
                    user_email VARCHAR(255) NOT NULL,
                    subject VARCHAR(255) NOT NULL,
                    message LONGTEXT NOT NULL,
                    status VARCHAR(20) NOT NULL DEFAULT 'open',
                    created_at DATETIME(6) NULL,
                    updated_at DATETIME(6) NULL,
                    INDEX i_x_support_tickets_user_id (user_id)
                );
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS support_tickets");
            migrationBuilder.Sql("DROP TABLE IF EXISTS device_tokens");
            migrationBuilder.Sql("DROP TABLE IF EXISTS subscriptions");
            migrationBuilder.Sql("DROP TABLE IF EXISTS password_reset_tokens");
            migrationBuilder.Sql("ALTER TABLE journal_entries DROP COLUMN IF EXISTS ai_response");
            migrationBuilder.Sql("DROP INDEX IF EXISTS i_x_users_google_id ON users");
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS google_id");
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS name");
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS occupation");
        }
    }
}
