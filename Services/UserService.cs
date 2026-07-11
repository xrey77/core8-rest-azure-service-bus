// Services/UserService.cs
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using core8_rest_azure_service_bus.Models;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.dto;

namespace core8_rest_azure_service_bus.Services
{
    public interface IUserService {
        Task<User> GetUserId(int id);
        Task<UpdateUserDto> UpdateUserProfile(int id, UpdateUserDto dto);
        Task<ChangePasswordDto> ChangeUserPassword(int id, ChangePasswordDto dto);
        Task<bool> UploadPicture(int id, string filename);
    }

    public class UserService: IUserService {

        private readonly IDbConnection dbConnection;
        private readonly ILogger<UserService> logger;

        public UserService(IDbConnection _dbConnection, ILogger<UserService> _logger) {
            dbConnection = _dbConnection;
            logger = _logger;
        }

        public async Task<User> GetUserId(int id) {
            const string sql = @"SELECT id, firstname, lastname, email, mobile, username, userpic, isactivated, isblocked, mailtoken, qrcodeurl FROM users WHERE id = @Id";                   
            var user = await dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });                        
            if (user != null) {
                return user;
            } else {
                logger.LogWarning("User ID does not exist.");
                throw new AppException($"The user id '{id}' does not exist.");
            }
        }

        public async Task<UpdateUserDto> UpdateUserProfile(int id, UpdateUserDto dto)
        {
            const string sqlUser = "SELECT COUNT(1) FROM users WHERE Id = @Id";
            var userExists = await dbConnection.ExecuteScalarAsync<int>(sqlUser, new { Id = id }) > 0;

            if (userExists)
            {
                const string updateSql = @"
                    UPDATE users 
                    SET firstname = @Firstname, lastname = @Lastname, mobile = @Mobile 
                    WHERE id = @Id";

                // Use ExecuteAsync instead of ExecuteScalarAsync
                await dbConnection.ExecuteAsync(updateSql, new 
                { 
                    dto.Firstname, 
                    dto.Lastname, 
                    dto.Mobile, 
                    Id = id 
                });

                return dto;
            }
            else
            {
                logger.LogWarning("User ID does not exist.");
                throw new AppException($"The user id '{id}' does not exist.");
            }
        }


        public async Task<ChangePasswordDto> ChangeUserPassword(int id, ChangePasswordDto dto) {
            const string sqlUser = "SELECT COUNT(1) FROM users WHERE Id = @Id";
            var userExists = await dbConnection.ExecuteScalarAsync<int>(sqlUser, new { Id = id }) > 0;

            if (userExists)
            {
                const string updateSql = @"UPDATE users SET password = @Password WHERE id = @Id";
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                await dbConnection.ExecuteAsync(updateSql, new 
                { 
                    Password = hashedPassword, 
                    Id = id 
                });

                return dto;
            }
            else
            {
                logger.LogWarning("User ID does not exist.");
                throw new AppException($"The user id '{id}' does not exist.");
            }
        }


        public async Task<bool> UploadPicture(int id, string filename) {
            const string sqlUser = "SELECT COUNT(1) FROM users WHERE Id = @Id";
            var userExists = await dbConnection.ExecuteScalarAsync<int>(sqlUser, new { Id = id }) > 0;

            if (userExists)
            {
                const string updateSql = @"UPDATE users SET userpic = @Userpic WHERE id = @Id";
                await dbConnection.ExecuteAsync(updateSql, new 
                { 
                    Userpic = filename, 
                    Id = id 
                });

                return true;
            }
            else
            {
                logger.LogWarning("User ID does not exist.");
                throw new AppException($"The user id '{id}' does not exist.");
            }

        }

    }
}
