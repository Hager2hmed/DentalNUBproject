﻿namespace DentalNUB.Entities

{
    public record LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}