// Services/AuthService.cs
using System.Data;
using BCrypt.Net;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Logging;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Models;
using core8_rest_azure_service_bus.Helpers;

namespace core8_rest_azure_service_bus.Services
{
    public interface IAuthService
    {
        Task<RegisterDto> signup(RegisterDto registerDto);   
        Task<User> signin(LoginDto loginDto);
    }

    public class AuthService : IAuthService 
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<AuthService> _logger;

        // Primary constructor available in .NET Core 8
        public AuthService(IDbConnection dbConnection, ILogger<AuthService> logger) 
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<RegisterDto> signup(RegisterDto dto)
        {
            // 1. Check if Email already exists
            const string checkEmailSql = "SELECT COUNT(1) FROM users WHERE email = @Email";
            var emailExists = await _dbConnection.ExecuteScalarAsync<int>(checkEmailSql, new { Email = dto.Email }) > 0;
            
            if (emailExists)
            {
                _logger.LogWarning("Registration failed. Email {Email} already exists.", dto.Email);
                // Adjust to your desired Exception framework (e.g., standard C# exception if not using gRPC)
                throw new AppException($"The email address '{dto.Email}' is already registered.");
            }

            // 2. Check if Username already exists
            const string checkUsernameSql = "SELECT COUNT(1) FROM users WHERE username = @Username";
            var usernameExists = await _dbConnection.ExecuteScalarAsync<int>(checkUsernameSql, new { Username = dto.Username }) > 0;

            if (usernameExists)
            {
                _logger.LogWarning("Registration failed. Username {Username} already exists.", dto.Username);
                throw new AppException($"The username '{dto.Username}' is already registered.");
            }

            // 3. Hash password using BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            
            // 4. Insert User and return the newly generated ID (MS SQL Server specific)
            const string insertSql = @"
                INSERT INTO users (firstname, lastname, email, mobile, username, password, role_id) 
                OUTPUT INSERTED.id 
                VALUES (@Fname, @Lname, @Email, @Mobile, @Username, @Password, @roleid);";

            var insertedId = await _dbConnection.ExecuteScalarAsync<int>(insertSql, new { 
                Id = 0,
                Fname = dto.Firstname, 
                Lname = dto.Lastname, 
                Email = dto.Email, 
                Mobile = dto.Mobile, 
                Username = dto.Username, 
                Password = hashedPassword,
                Roleid = 2
            });

            // 5. Update DTO or return created user
            dto.Id = insertedId;
            
            return dto;
        }

        public async Task<User> signin(LoginDto dto) 
        {
            const string sql = "SELECT id, firstname, lastname, email, mobile, username, password, userpic, isactivated, isblocked, mailtoken, qrcodeurl FROM users WHERE username = @Username";
            
            // Fetch the user from the database using Dapper
            var user = await _dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { Username = dto.Username });
            
            // 1. Verify if user exists
            if (user == null)
            {
                _logger.LogWarning("User not found for username {Username}.", dto.Username);
                throw new AppException($"The user with username '{dto.Username}' was not found, please register.");
            }

            // 2. Verify password hash using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                _logger.LogWarning("Invalid password attempt for username {Username}.", dto.Username);
                throw new AppException("Invalid password attempt, please try again.");
            }

            return user;
        }

    }
}
