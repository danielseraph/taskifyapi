﻿namespace Taskify.Services.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpirationTime { get; set; }
    }
}
