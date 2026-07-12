// // Services/JWTServiceManage.cs
// using System.Data;
// using Dapper;
// using Microsoft.IdentityModel.Tokens;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Microsoft.Data.SqlClient;
// using core8_rest_azure_service_bus.Models;

// namespace core8_rest_azure_service_bus.Services
// {
//     public interface IJWTServiceManage
//     {
//         JWTTokens Authenticate(User users);
//     }

//     public class JWTServiceManage : IJWTServiceManage
//     {
//         private readonly IConfiguration _configuration;
//         private readonly IDbConnection _dbConnection;

 
//         public JWTServiceManage(IConfiguration configuration, IDbConnection dbConnection)
//         {
//             _configuration = configuration;
//             _dbConnection = dbConnection;
//         }

//         public JWTTokens Authenticate(User users)
//         {
//             const string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";
//             var userExists = _dbConnection.ExecuteScalar<bool>(query, new { users.Username, users.Password });

//             if (!userExists)
//             {
//                 // Fix CS8603: Returns null safely by marking the return type as nullable, 
//                 // or throw an exception if an invalid login should crash/stop execution.
//                 throw new UnauthorizedAccessException("Invalid username or password.");
//             }

//             // Fix CS8604 (Line 40): Fallback to empty string if config is missing, or throw error
//             string jwtKey = _configuration["JWT:Key"] 
//                 ?? throw new InvalidOperationException("JWT Key is missing from configuration.");
                
//             var tokenHandler = new JwtSecurityTokenHandler();
//             var tokenKey = Encoding.UTF8.GetBytes(jwtKey);

//             // Fix CS8604 (Line 46): Ensure users.Username is not null before passing to Claim
//             string username = users.Username 
//                 ?? throw new ArgumentNullException(nameof(users), "Username cannot be null.");

//             var tokenDescriptor = new SecurityTokenDescriptor
//             {
//                 Subject = new ClaimsIdentity(new Claim[]
//                 {
//                     new Claim(ClaimTypes.Name, username)
//                 }),
//                 Expires = DateTime.UtcNow.AddMinutes(15),
//                 SigningCredentials = new SigningCredentials(
//                     new SymmetricSecurityKey(tokenKey), 
//                     SecurityAlgorithms.HmacSha256Signature)
//             };

//             var token = tokenHandler.CreateToken(tokenDescriptor);
            
//             return new JWTTokens { Token = tokenHandler.WriteToken(token) };
//         }

//     }    
// }
