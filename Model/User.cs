// Models/User.cs
using Microsoft.AspNetCore.Mvc;


namespace TrafficDataApi.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
