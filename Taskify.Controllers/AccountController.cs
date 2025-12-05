using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskify.Services.DTOs;
using Taskify.Services.Interface;

namespace Taskify.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUser;
        public AccountController(IAuthService authService, ICurrentUserService currentUser)
        {
            _authService = authService;
            _currentUser = currentUser;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var user = await _authService.RegisterAsync(model, ip);
            return StatusCode(user.StatusCode, user);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var user = await _authService.LoginAsync(model, ip);
            return StatusCode(user.StatusCode, user);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh([FromBody] RefreshTokenRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var res = await _authService.RefreshTokenAsync(model.RefreshToken, ip);
            return StatusCode(res.StatusCode, res);
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<ActionResult> Revoke([FromBody] RefreshTokenRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var res = await _authService.RevokeRefreshTokenAsync(model.RefreshToken, ip);
            return StatusCode(res.StatusCode, res);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = _currentUser.GetUserId();
            var res = await _authService.LogoutAsync(userId ?? string.Empty);
            return StatusCode(res.StatusCode, res);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _currentUser.GetCurrentUserAsync();
            return Ok(user);
        }
    }
}
