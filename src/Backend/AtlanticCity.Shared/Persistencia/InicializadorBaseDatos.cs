using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Logging;

namespace AtlanticCity.Compartido.Persistencia
{
    public static class InicializadorBaseDatos
    {
        // Enforces database existence and baseline data
        public static async Task InicializarAsync(string connectionString, ILogger log)
        {
            try 
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                string dbName = builder.InitialCatalog;
                builder.InitialCatalog = "master"; 

                using (var masterConn = new SqlConnection(builder.ConnectionString))
                {
                    await masterConn.ExecuteAsync($@"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name='{dbName}') CREATE DATABASE [{dbName}]");
                }

                using var connection = new SqlConnection(connectionString);

                // 1. Stored Procedures (Idempotent creation)
                await connection.ExecuteAsync(@"
                    -- SP for Massive Loading
                    EXEC('CREATE OR ALTER PROCEDURE SP_InsertProductUnique 
                        @BatchId UNIQUEIDENTIFIER, @Code VARCHAR(50), @Name VARCHAR(100), @Period VARCHAR(20) AS 
                        IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = @Code AND Period = @Period)
                            INSERT INTO Products (BatchId, ProductCode, ProductName, Period, Status) VALUES (@BatchId, @Code, @Name, @Period, ''Cargado'')
                        ELSE
                            INSERT INTO Products (BatchId, ProductCode, ProductName, Period, Status) VALUES (@BatchId, @Code, @Name, @Period, ''Existente'')');

                    -- SP for Batch Status Updates
                    EXEC('CREATE OR ALTER PROCEDURE SP_UpdateBatchStatus 
                        @Id UNIQUEIDENTIFIER, @Status VARCHAR(50), @Total INT = 0, @Success INT = 0, @Error INT = 0 AS 
                        UPDATE ProcessBatches 
                        SET Status = @Status, TotalRecords = @Total, SuccessCount = @Success, ErrorCount = @Error, 
                            EndTime = CASE WHEN @Status IN (''Finalizado'', ''Notificado'', ''Rechazado'', ''Error'') THEN GETDATE() ELSE EndTime END 
                        WHERE Id = @Id');
                ");

                // 2. Default Seed Client
                await connection.ExecuteAsync(@"
                    IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
                    BEGIN
                        INSERT INTO Users (Username, PasswordHash, Email, Role) 
                        VALUES ('admin', 'password123', 'admin@atlanticcity.com', 'Admin');
                    END
                ");
            }
            catch (Exception ex)
            {
                log.LogError($"[DB] Initialization error: {ex.Message}");
                throw;
            }
        }
    }
}
