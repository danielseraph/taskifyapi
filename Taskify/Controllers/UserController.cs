using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskify.Domain.Constant;
using Taskify.Services.DTOs;
using Taskify.Services.Interface;

namespace Taskify.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("getAllUsers")]
        [Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUserAsync();
            return Ok(users);
        }

        [HttpPut("UpdateUserProfile{id}")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserDto dto)
        {
            var isAdmin = User.IsInRole("Admin");
            var result = await _userService.UpdateUserProfileAsync(dto);
            return Ok(result);
        }

        [HttpGet("getUserById")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUser()
        {
            var user = await _userService.GetUserAsync();
            return Ok(user);
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var user = await _userService.GetUsers();
            return Ok(user);
        }


        //public async Task<IActionResult> GetCurrentUser()
        //{
        //    var user = await _currentUser.GetCurrentUserAsync();
        //    if (user == null)
        //        return Unauthorized("User not found or not logged in");

        //    return Ok(user);
        //}
    }
}
