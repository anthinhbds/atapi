using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using ApiV1.Models;

namespace atmnr_api.Models
{
    public class UserRegisterInfo
    {
        public string? Userid { get; set; } = String.Empty;
        public string? Password { get; set; } = String.Empty;
        public string? Name { get; set; } = String.Empty;
    }
    public class UserLoginModel
    {
        public string? UserId { get; set; } = String.Empty;
        public string? Password { get; set; } = String.Empty;
    }

    public class UserJwtTokenInfo
    {
        public string? UserId { get; set; } = String.Empty;
        public string? Name { get; set; } = String.Empty;
    }

    public class UserResultInfo
    {
        public string UserId { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string AccessToken { get; set; } = String.Empty;
        public string RefreshToken { get; set; } = String.Empty;
        public string[] ClaimType { get; set; } = [];
        public string[] ClaimValue { get; set; } = [];
    }

    public class RefreshTokenRequest
    {
        public string? RefreshToken { get; set; }
    }

    public class ChangePasswordModel
    {
        public string UserId { get; set; }
        public string Oldpassword { get; set; }
        public string Newpassword { get; set; }
        public string Confirmpasssword { get; set; }
    }

}