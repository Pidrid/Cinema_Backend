﻿namespace Cinema_Backend.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
