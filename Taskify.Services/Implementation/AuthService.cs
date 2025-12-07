using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Taskify.DataStore.Migrations;
using Taskify.DataStore.Repositorise.Interface;
using Taskify.Domain.Constant;
using Taskify.Domain.Entities;
using Taskify.Services.DTOs;
using Taskify.Services.Interface;
using Taskify.Services.Utilities;

namespace Taskify.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _usermanager;
        private readonly RoleManager<IdentityRole> _role;
        private readonly IMapper _mapper;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtConfig _jwtConfig;
        private readonly IAppRepository<RefreshToken> _refToken;

        public AuthService(
            UserManager<AppUser> usermanager,
            IMapper mapper,
            RoleManager<IdentityRole> role,
            IJwtTokenService tokenService,
            IOptions<JwtConfig> opt,
            IAppRepository<RefreshToken> refToken)
        {
            _usermanager = usermanager;
            _mapper = mapper;
            _jwtTokenService = tokenService;
            _jwtConfig = opt.Value;
            _refToken = refToken;
            _role = role;
            }
        //Register service
        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto model, string ipAddress)
        {
            // If the caller supplied a username, ensure it's not already taken.
            var requestedUserName = string.IsNullOrWhiteSpace(model.UserName) ? null : model.UserName.Trim();
            if (!string.IsNullOrWhiteSpace(requestedUserName))
            {
                var user = await _usermanager.FindByNameAsync(requestedUserName);
                if (user != null)
                    return ApiResponseBuilder.Fail<AuthResponseDto>("UserName has been taken", statusCode: StatusCodes.Status409Conflict);
            }

            // Ensure email is unique
            var userEmail = await _usermanager.FindByEmailAsync(model.Email);
            if (userEmail != null)
                return ApiResponseBuilder.Fail<AuthResponseDto>("User with same email already exist", statusCode: StatusCodes.Status409Conflict);

            // Generate username if none was provided using FirstName.LastName and a numeric suffix when needed
            string finalUserName;
            if (!string.IsNullOrWhiteSpace(requestedUserName))
            {
                finalUserName = requestedUserName.ToLowerInvariant();
            }
            else
            {
                // build a sanitized base username from first and last name
                var raw = $"{model.FirstName}.{model.LastName}".ToLowerInvariant();
                var sb = new System.Text.StringBuilder();
                foreach (var ch in raw)
                {
                    if (char.IsLetterOrDigit(ch) || ch == '.')
                        sb.Append(ch);
                }

                var baseName = sb.ToString();
                if (string.IsNullOrWhiteSpace(baseName))
                    baseName = "user";

                // attempt uniqueness by appending random numbers (and fallback to short guid)
                finalUserName = baseName;
                var attempt = 0;
                while (await _usermanager.FindByNameAsync(finalUserName) != null)
                {
                    attempt++;
                    var suffix = RandomNumberGenerator.GetInt32(100, 1000); // 3-digit suffix
                    finalUserName = $"{baseName}{suffix}";
                    if (attempt > 10)
                    {
                        finalUserName = $"{baseName}{Guid.NewGuid().ToString().Split('-')[0]}";
                        break;
                    }
                }
            }

            var registerUser = _mapper.Map<AppUser>(model);
            registerUser.UserName = finalUserName.ToLowerInvariant();

            var result = await _usermanager.CreateAsync(registerUser, model.Password);
            if (!result.Succeeded)
                return ApiResponseBuilder.Fail<AuthResponseDto>("Registeration failed", statusCode: StatusCodes.Status404NotFound);

            var addUserRole = await _usermanager.AddToRoleAsync(registerUser, UserRole.PROJECTMANAGER);
            var userRole = await _usermanager.GetRolesAsync(registerUser);

            // include security stamp in token so we can invalidate tokens when stamp changes
            var secStamp = await _usermanager.GetSecurityStampAsync(registerUser);

            // create refresh token and persist
            var refreshToken = GenerateRefreshToken(ipAddress);
            registerUser.RefreshTokens.Add(refreshToken);
            await _refToken.AddAsync(refreshToken);
            await _refToken.SaveChangesAsync();

            var token = new AuthResponseDto
            {
                Token = _jwtTokenService.GetJwtToken(roles: userRole, userId: registerUser.Id, username: registerUser.UserName, securityStamp: secStamp),
                // Use minutes to match JwtTokenService (which uses AddMinutes)
                ExpirationTime = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationTime),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.Expires
            };
            return ApiResponseBuilder.Success<AuthResponseDto>(data: token);
        }

        //Login service
        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto model, string ipAddress)
        {
            //Find user by enail
            var user = await _usermanager.FindByEmailAsync(model.Email);
            if (user == null)
                return ApiResponseBuilder.Fail<AuthResponseDto>("Invalid Email or Password", statusCode: StatusCodes.Status404NotFound);
            //Verify password
            var verifyPassword = await _usermanager.CheckPasswordAsync(user, model.Password);
            if (verifyPassword == false)
                return ApiResponseBuilder.Fail<AuthResponseDto>("Invalid Email or Password", statusCode: StatusCodes.Status404NotFound);
            //Get role
            var userRole = await _usermanager.GetRolesAsync(user);

            //Generate jwt token
            var secStamp = await _usermanager.GetSecurityStampAsync(user);

            // create refresh token and persist
            var refreshToken = GenerateRefreshToken(ipAddress);
            user.RefreshTokens.Add(refreshToken);
            await _refToken.AddAsync(refreshToken);
            await _refToken.SaveChangesAsync();
           
            var token = new AuthResponseDto
            {
                Token = _jwtTokenService.GetJwtToken(roles: userRole, user.Id, username:user.UserName!, securityStamp: secStamp),
                ExpirationTime = DateTime.UtcNow.AddHours(_jwtConfig.ExpirationTime),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.Expires
            };
            return ApiResponseBuilder.Success<AuthResponseDto>(data: token);
        }

        // Logout - revoke refresh tokens and rotate security stamp to invalidate issued JWTs
        public async Task<ApiResponse<string>> LogoutAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ApiResponseBuilder.Fail<string>("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);

            var user = await _usermanager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ApiResponseBuilder.Fail<string>("User not found", statusCode: StatusCodes.Status404NotFound);

            // Revoke active refresh tokens
            var now = DateTime.UtcNow;
            foreach (var rt in user.RefreshTokens.Where(t => t.IsActive))
            {
                rt.RevokedAt = now;
                rt.ReasonRevoked = "User logout";
            }

            // rotate security stamp so existing JWTs containing old stamp are considered invalid
            var stampResult = await _usermanager.UpdateSecurityStampAsync(user);
            if (!stampResult.Succeeded)
                return ApiResponseBuilder.Fail<string>("Failed to logout user", statusCode: StatusCodes.Status500InternalServerError);

            var updateRes = await _usermanager.UpdateAsync(user);
            if (!updateRes.Succeeded)
                return ApiResponseBuilder.Fail<string>("Failed to logout user", statusCode: StatusCodes.Status500InternalServerError);

            return ApiResponseBuilder.Success<string>("Logout successful");
        }

        // Refresh token rotation
        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return ApiResponseBuilder.Fail<AuthResponseDto>("Invalid refresh token", statusCode: StatusCodes.Status400BadRequest);

            var user = await _usermanager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            if (user == null)
                return ApiResponseBuilder.Fail<AuthResponseDto>("Invalid refresh token", statusCode: StatusCodes.Status404NotFound);

            var rt = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
            if (rt == null || !rt.IsActive)
                return ApiResponseBuilder.Fail<AuthResponseDto>("Refresh token is not active", statusCode: StatusCodes.Status400BadRequest);

            // rotate
            var newRt = GenerateRefreshToken(ipAddress);
            rt.RevokedAt = DateTime.UtcNow;
            rt.RevokedByIp = ipAddress;
            rt.ReplacedByToken = newRt.Token;
            rt.ReasonRevoked = "Rotated by refresh";

            user.RefreshTokens.Add(newRt);
            var updateResult = await _usermanager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ApiResponseBuilder.Fail<AuthResponseDto>("Failed to rotate refresh token", statusCode: StatusCodes.Status500InternalServerError);

            var userRole = await _usermanager.GetRolesAsync(user);
            var secStamp = await _usermanager.GetSecurityStampAsync(user);
            var authToken = new AuthResponseDto
            {
                Token = _jwtTokenService.GetJwtToken(roles: userRole, userId: user.Id, username: user.UserName!, securityStamp: secStamp),
                ExpirationTime = DateTime.UtcNow.AddHours(_jwtConfig.ExpirationTime),
                RefreshToken = newRt.Token,
                RefreshTokenExpiration = newRt.Expires
            };

            return ApiResponseBuilder.Success<AuthResponseDto>(data: authToken);
        }

        public async Task<ApiResponse<string>> RevokeRefreshTokenAsync(string refreshToken, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return ApiResponseBuilder.Fail<string>("Invalid refresh token", statusCode: StatusCodes.Status400BadRequest);

            var user = await _usermanager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            if (user == null)
                return ApiResponseBuilder.Fail<string>("Refresh token not found", statusCode: StatusCodes.Status404NotFound);

            var rt = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
            if (rt == null || !rt.IsActive)
                return ApiResponseBuilder.Fail<string>("Token already revoked or expired", statusCode: StatusCodes.Status400BadRequest);

            rt.RevokedAt = DateTime.UtcNow;
            rt.RevokedByIp = ipAddress;
            rt.ReasonRevoked = "Revoked by user";
            var updateResult = await _usermanager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ApiResponseBuilder.Fail<string>("Failed to revoke token", statusCode: StatusCodes.Status500InternalServerError);

            return ApiResponseBuilder.Success<string>("Refresh token revoked");
        }

        // helper
        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7),
                CreateAT = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        public async Task<ApiResponse<string>> AddRoleAsync(string model)
        {
            var role = await _role.FindByNameAsync(model);
            if(role != null)
            {
                return ApiResponseBuilder.Fail<string>("Role not found", statusCode: StatusCodes.Status404NotFound);
            }
            var result = await _role.CreateAsync(new IdentityRole(model));
            if (!result.Succeeded)
            {
                return ApiResponseBuilder.Fail<string>("Failed to add role", statusCode: StatusCodes.Status500InternalServerError);
            }
            return ApiResponseBuilder.Success<string>("Role added successfully");
        }
    }
}
