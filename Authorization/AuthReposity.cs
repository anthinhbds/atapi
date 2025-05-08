using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using atmnr_api.Data;
using atmnr_api.Entities;
using atmnr_api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using atmnr_api.Services;
using Microsoft.AspNetCore.Authentication;

namespace atmnr_api.Authorization
{
    public class AuthReposity : IAuthReposity
    {
        private readonly AtDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AuthReposity(AtDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserResultInfo> Login(string userid, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId.ToLower().Equals(userid.ToLower()));

            if (user is null)
            {
                throw new Exception("User Not Found!");
            }
            else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                throw new Exception("Password invalid!");
            }
            else
            {
                TokenService tokenService = new TokenService(_configuration, _httpContextAccessor, _context);
                var (accessToken, refreshToken) = await tokenService.GenerateTokens(new UserJwtTokenInfo
                {
                    UserId = user.UserId,
                    Name = user.Name
                });
                var claims = await _context.UserClaims.Where(uc => uc.UserId == user.UserId).ToListAsync();
                var claimType = claims.Select(c => c.ClaimId).ToArray();
                var claimvalue = claims.Select(c => c.Claimvalue).ToArray();
                return new UserResultInfo
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ClaimType = claimType,
                    ClaimValue = claimvalue
                };
            }
        }

        public async Task<WrapModel<string>> Register(User user, string password)
        {
            var response = new WrapModel<string>();
            if (await UserExists(user.UserId))
            {
                response.Success = false;
                response.Message = "User already exists.";
                return response;
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.CreatedBy = "SYS";
            // user.CreatedDate = DateTime.Now;
            user.UpdatedBy = "SYS";
            // user.LastUpdate = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            response.Data = user.UserId;
            return response;
        }

        public async Task<string> ChangePassword(ChangePasswordModel model)
        {
            string errMsg = "";
            string userId = model.UserId;
            var userEntity = await _context.Users.Where(u => u.UserId.ToLower() == userId.ToLower()).FirstOrDefaultAsync();

            if (userEntity == null) return "User not found";

            if (!VerifyPasswordHash(model.Oldpassword, userEntity.PasswordHash, userEntity.PasswordSalt)) return "Mật khẩu hiện tại không đúng.";

            CreatePasswordHash(model.Newpassword, out byte[] passwordHash, out byte[] passwordSalt);

            userEntity.PasswordHash = passwordHash;
            userEntity.PasswordSalt = passwordSalt;
            userEntity.CreatedBy = userId;
            userEntity.UpdatedBy = userId;
            userEntity.LastUpdate = DateTime.UtcNow;

            // _context.Users.Update(userEntity);

            var rfTokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId && rt.IsRevoked == false).ToListAsync();
            foreach (var rfToken in rfTokens)
            {
                rfToken.IsRevoked = true;
                rfToken.Revokedbyip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                // _context.RefreshTokens.Update(rfToken);
            }

            await _context.SaveChangesAsync();

            return errMsg;
        }

        public async Task<bool> UserExists(string userid)
        {
            if (await _context.Users.AnyAsync(u => u.UserId.ToLower() == userid.ToLower()))
            {
                return true;
            }
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.UserId),
                new Claim(ClaimTypes.Name,user.Name),
            };

            var appToken = _configuration.GetSection("JwtSettings:Key").Value;
            if (appToken is null)
                throw new Exception("App Token is Null");

            SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appToken));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<UserResultInfo> Logout(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token && rt.IsRevoked == false);
            if (refreshToken is null)
            {
                throw new Exception("Token not found");
            }
            refreshToken.IsRevoked = true;
            refreshToken.Revokedbyip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
            await _context.SaveChangesAsync();
            // await _httpContextAccessor.HttpContext.SignOutAsync();
            return new UserResultInfo
            {
                UserId = "",
                Name = "",
                AccessToken = "",
                RefreshToken = "",
                ClaimType = [],
                ClaimValue = []
            };
        }
    }
}