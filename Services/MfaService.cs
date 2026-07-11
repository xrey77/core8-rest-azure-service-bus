// Services/MfaServices.cs
using System.Data;
using BCrypt.Net;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Logging;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Models;
using core8_rest_azure_service_bus.Helpers;
using Google.Authenticator;
using QRCoder;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;



namespace core8_rest_azure_service_bus.Services
{

    public interface IMfaService {
        Task<User> ActivateMfa(int id, MfaActivateDto dto);
        Task<User> VerifyOtp(int id, VerifyOtpDto dto);
        
    }

    public class MfaService : IMfaService {

        private readonly IDbConnection dbConnection;
        private readonly IConfiguration configuration;  
        private readonly ILogger<MfaService> logger;

        public MfaService(IDbConnection _dbConnection, ILogger<MfaService> _logger, IConfiguration _configuration) {
            dbConnection = _dbConnection;
            configuration = _configuration;
            logger = _logger;
        }

        public async Task<User> ActivateMfa(int id, MfaActivateDto dto) {

            const string sqlUser = "SELECT id,email FROM users WHERE Id = @Id";             
            var userExists = await dbConnection.QueryFirstOrDefaultAsync<User>(sqlUser, new { Id = id });        

            if (userExists is not null)
            {
                if (dto.TwoFactorEnabled == true) {

                    string issuer = "Barclays Bank";
                    string accountSecretKey = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10); // 10-char secret
                    
                    TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                    var setupInfo = tfa.GenerateSetupCode(issuer, userExists.Email, accountSecretKey, false, 3);

                    // Generate QR Code as base64 using QRCoder
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(setupInfo.QrCodeSetupImageUrl, QRCodeGenerator.ECCLevel.Q);
                    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);
                    string qrCodeBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";


                    const string updateSql = @"UPDATE users SET secret = @Secret, qrcodeurl = @Qrcodeurl WHERE id = @Id";
                    await dbConnection.ExecuteAsync(updateSql, new 
                    { 
                        Secret = accountSecretKey, 
                        Qrcodeurl = qrCodeBase64,
                        Id = id 
                    });

                    const string sql = "SELECT secret, qrcodeurl FROM users WHERE Id = @Id";             
                    var user = await dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });        
                    if (user is not null) {
                        return user;
                    }
                    throw new AppException(".");
                } else {

                    const string updateSql = @"UPDATE users SET secret = null, qrcodeurl = null WHERE id = @Id";
                    await dbConnection.ExecuteAsync(updateSql, new 
                    { 
                        Id = id 
                    });
                    logger.LogWarning("Multi-Factor Authenticator has been disabled.");
                    throw new AppException("Multi-Factor Authenticator has been disabled.");
                }
            }
            else
            {
                logger.LogWarning("User ID does not exist.");
                throw new AppException($"The user id '{id}' does not exist.");
            }
        }


        public async Task<User> VerifyOtp(int id, VerifyOtpDto dto) {
             const string sqlUser = "SELECT id,secret FROM users WHERE Id = @Id";             
            var userExists = await dbConnection.QueryFirstOrDefaultAsync<User>(sqlUser, new { Id = id });        
            if (userExists is not null) {


                if (userExists.Secret is null) {
                    throw new AppException("Multi-Factor Authenticator is not yet activated.");
                }

                TwoFactorAuthenticator twoFactor =  new TwoFactorAuthenticator();
                bool isValid = twoFactor.ValidateTwoFactorPIN(userExists.Secret, dto.Otp , false);
                if (isValid)
                {
                    return userExists;

                } else {
                    throw new AppException("Invalid OTP code, please try again.");

                }                

            } else {
                logger.LogWarning("User ID does not exist.");
                throw new AppException($"The user id '{id}' does not exist.");
            }
           
        }


    }
}