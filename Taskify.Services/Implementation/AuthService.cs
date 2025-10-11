using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
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
        private readonly IMapper _mapper;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtConfig _jwtConfig;

        public AuthService(UserManager<AppUser> usermanager, IMapper mapper, IJwtTokenService tokenService, IOptions<JwtConfig>opt)
        {
            _usermanager = usermanager;
            _mapper = mapper;
            _jwtTokenService = tokenService;
            _jwtConfig = opt.Value;
        }
        //Register service
        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto model)
        {
            var user = await _usermanager.FindByNameAsync(model.UserName);
            if (user != null)
                return ApiResponseBuilder.Fail<AuthResponseDto>("UserName has been taken", statusCode: StatusCodes.Status409Conflict);
            var userEmail = await _usermanager.FindByEmailAsync(model.Email);
            if (user != null)
                return ApiResponseBuilder.Fail<AuthResponseDto>("User with same email already exist", statusCode: StatusCodes.Status409Conflict);
            var registerUser = _mapper.Map<AppUser>(model);
            registerUser.UserName = model.UserName.ToLower();
            var result = await _usermanager.CreateAsync(registerUser, model.Password);
            if (!result.Succeeded)
                return ApiResponseBuilder.Fail<AuthResponseDto>("Registeration failed", statusCode: StatusCodes.Status404NotFound);
            var addUserRole = await _usermanager.AddToRoleAsync(registerUser, UserRole.PROJECTMANAGER);
            var userRole = await _usermanager.GetRolesAsync(registerUser);
            var token = new AuthResponseDto
            {
                Token = _jwtTokenService.GetJwtToken(roles: userRole, userId: registerUser.Id, username: registerUser.UserName),
                ExpirationTime = DateTime.UtcNow.AddHours(_jwtConfig.ExpirationTime)
            };
            return ApiResponseBuilder.Success<AuthResponseDto>(data:token);
        }

        //Login service
        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto model)
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
            var token = new AuthResponseDto
            {
                Token = _jwtTokenService.GetJwtToken(roles: userRole, user.Id, username:user.UserName),
                ExpirationTime = DateTime.UtcNow.AddHours(_jwtConfig.ExpirationTime)
            };
            return ApiResponseBuilder.Success<AuthResponseDto>(data: token);
        }
    }
}
