using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

public class DbInitializer
{
    private readonly IConfiguration _configuration;

    public DbInitializer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        // 1. Get base connection string (assumed to target the master/default db)
        var baseConnectionString = _configuration.GetConnectionString("DefaultConnection");
        var dbName = _configuration["DatabaseSettings:DbName"] ?? "core8_dapper";

        // Use SqlConnectionStringBuilder to manipulate MSSQL connection strings
        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = "master" // Strip specific DB to connect to server level
        };
        var serverConnectionString = builder.ConnectionString;

        builder.InitialCatalog = dbName; // Set target database
        var targetConnectionString = builder.ConnectionString;

        // 2. Create database if it does not exist (MSSQL Server syntax)
        using (var connection = new SqlConnection(serverConnectionString))
        {
            await connection.OpenAsync();
            var createDbSql = $@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{dbName}')
                BEGIN
                    CREATE DATABASE [{dbName}];
                END";
            await connection.ExecuteAsync(createDbSql);
        }

        // 3. Create tables using the target connection
        using (var connection = new SqlConnection(targetConnectionString))
        {
            await connection.OpenAsync();
            await CreateTablesAsync(connection);
        }
    }

    private static async Task CreateTablesAsync(IDbConnection connection)
    {

        const string createUsersAndRolesTable = @"
            IF OBJECT_ID('roles', 'U') IS NULL
            BEGIN
                CREATE TABLE [roles] (
                    [id] INT IDENTITY(1,1) NOT NULL,
                    [created_at] DATETIME2(3) DEFAULT NULL,
                    [updated_at] DATETIME2(3) DEFAULT NULL,
                    [deleted_at] DATETIME2(3) DEFAULT NULL,
                    [name] NVARCHAR(25) NOT NULL,
                    CONSTRAINT [PK_roles] PRIMARY KEY CLUSTERED ([id] ASC)
                );
            END

            IF OBJECT_ID('Users', 'U') IS NULL
            BEGIN
                CREATE TABLE core8_dapper.dbo.users (
                    id int IDENTITY(0,1) NOT NULL,
                    role_id INT FOREIGN KEY REFERENCES roles(id),                    
                    firstname varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                    lastname varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                    email varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                    mobile varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                    username varchar(100) COLLATE SQL_Latin1_General_CP1_CS_AS NULL,
                    password varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                    isactivated int DEFAULT 0 NULL,
                    isblocked int DEFAULT 0 NULL,
                    mailtoken int DEFAULT 0 NULL,
                    userpic varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS DEFAULT 'pix.png' NULL,
                    secret text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                    qrcodeurl text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                    [created_at] DATETIME2(3) DEFAULT NULL,
                    [updated_at] DATETIME2(3) DEFAULT NULL,
                    [deleted_at] DATETIME2(3) DEFAULT NULL,
                    CONSTRAINT users_PK PRIMARY KEY (id)
                );
                CREATE UNIQUE NONCLUSTERED INDEX users_UN_EMAIL ON core8_dapper.dbo.users (email);
                CREATE UNIQUE NONCLUSTERED INDEX users_UN_USERNAME ON core8_dapper.dbo.users (username);            
            END

            IF OBJECT_ID('UserRoles', 'U') IS NULL
            BEGIN
                CREATE TABLE UserRoles (
                    UserId INT NOT NULL,
                    RoleId INT NOT NULL,
                    PRIMARY KEY (UserId, RoleId),
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
                );
            END";


        const string createProductsAndCategoriesTable = @"
            IF OBJECT_ID('Categories', 'U') IS NULL
            BEGIN
                CREATE TABLE Categories (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL
                );
            END

            IF OBJECT_ID('Products', 'U') IS NULL
            BEGIN
            CREATE TABLE core8_dapper.dbo.products (
                id int IDENTITY(0,1) NOT NULL,
                category_id INT FOREIGN KEY REFERENCES categories(id),
                descriptions varchar(100) NULL,
                qty int DEFAULT 0 NULL,
                unit varchar(100) NULL,
                costprice decimal(18,2) NULL,
                sellprice decimal(18,2) NULL,
                saleprice decimal(18,2) NULL,
                productpicture varchar(100) NULL,
                alertstocks int DEFAULT 0 NULL,
                criticalstocks int DEFAULT 0 NULL,
                [created_at] DATETIME2(3) DEFAULT NULL,
                [updated_at] DATETIME2(3) DEFAULT NULL,
                [deleted_at] DATETIME2(3) DEFAULT NULL,
                CONSTRAINT products_PK PRIMARY KEY (id),
                CONSTRAINT products_UN_DESCRIPTIONS UNIQUE (descriptions)
            );
            END";

        const string createSales = @"
            IF OBJECT_ID('sales', 'U') IS NULL
            BEGIN
            CREATE TABLE core8_dapper.dbo.sales (
                id int IDENTITY(0,1) NOT NULL,
                salesamount decimal(18,2) NULL,
                salesdate datetime2(0) NULL,
                CONSTRAINT sales_PK PRIMARY KEY (id)
            );
            END";

        
        await connection.ExecuteAsync(createUsersAndRolesTable);
        await connection.ExecuteAsync(createProductsAndCategoriesTable);
        await connection.ExecuteAsync(createSales);
    }
}
