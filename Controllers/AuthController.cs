using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using atmnr_api.Models;
using atmnr_api.Authorization;
using atmnr_api.Entities;
using atmnr_api.Data;
using atmnr_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace atmnr_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthReposity _authRepo;
        private readonly AtDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthReposity authRepo, AtDbContext dbContext, IHttpContextAccessor http, IConfiguration configuration)
        {
            _authRepo = authRepo;
            _dbContext = dbContext;
            _configuration = configuration;
            _http = http;
        }

        [HttpPost("registerAT")]
        public async Task<ActionResult<WrapModel<int>>> Register(UserRegisterInfo request)
        {
            var response = await _authRepo.Register(
                new User { UserId = request.Userid, Name = request.Name }, request.Password
            );
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<WrapModel<UserResultInfo>>> Login(UserLoginModel request)
        {
            var response = await _authRepo.Login(request.UserId, request.Password);
            return WrapModel<UserResultInfo>.Ok(response);
        }

        [HttpPost("changePassword")]
        public async Task<ActionResult<WrapModel<bool>>> ChangePassword(ChangePasswordModel model)
        {
            if (model.Newpassword != model.Confirmpasssword)
            {
                return WrapModel<bool>.Fail("Mật khẩu và Xác nhận mật khẩu không giống nhau");
            }
            var msg = await _authRepo.ChangePassword(model);
            if (string.IsNullOrEmpty(msg)) return WrapModel<bool>.Ok(true);
            return WrapModel<bool>.Fail(msg);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var refreshToken = await _dbContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.IsRevoked == false);

            if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow || refreshToken.IsRevoked)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == refreshToken.UserId);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }


            // Generate new tokens
            var tokenService = new TokenService(_configuration, _http, _dbContext);
            var accessToken = tokenService.GenerateAccessToken(new UserJwtTokenInfo { UserId = user.UserId, Name = user.Name });
            var claims = await _dbContext.UserClaims.Where(uc => uc.UserId == user.UserId).ToListAsync();
            var claimType = claims.Select(c => c.ClaimId).ToArray();
            var claimvalue = claims.Select(c => c.Claimvalue).ToArray();

            return Ok(new UserResultInfo
            {
                UserId = user.UserId,
                Name = user.Name,
                AccessToken = accessToken,
                ClaimType = claimType,
                ClaimValue = claimvalue
            });
        }

        [HttpPost("Logout/{refreshToken}")]
        public async Task<ActionResult<WrapModel<UserResultInfo>>> Logout(string refreshToken)
        {
            var response = await _authRepo.Logout(refreshToken);
            return WrapModel<UserResultInfo>.Ok(response);
        }
    }
}