﻿using System.ComponentModel.DataAnnotations;

namespace Website.Classes
{
    public struct UpdatedEmail
    {
        [EmailAddress]
        public string Email { get; set; }
        [Password]
        public string Password { get; set; }
        public string Token { get; set; }
    }
}