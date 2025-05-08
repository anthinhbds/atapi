using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using atmnr_api.Models;
using atmnr_api.Entities;
using atmnr_api.Data;
using Microsoft.EntityFrameworkCore;

namespace atmnr_api.Services;
public class TokenService : GenericService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _http;
    private readonly AtDbContext _dbContext;

    public TokenService(IConfiguration configuration, IHttpContextAccessor http, AtDbContext dbContext) : base(http, dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _http = http;
    }

    public async Task<(string AccessToken, string RefreshToken)> GenerateTokens(UserJwtTokenInfo user)
    {
        // Generate Access Token
        var accessToken = GenerateAccessToken(user);

        //Revoke old refresh token
        await RevokeRefreshToken(user.UserId);

        // Generate Refresh Token
        var refreshToken = GenerateRefreshToken();

        await SaveRefreshToken(user.UserId, refreshToken);

        await _dbContext.SaveChangesAsync();

        return (AccessToken: accessToken, RefreshToken: refreshToken);
    }

    public string GenerateAccessToken(UserJwtTokenInfo user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"])),
            SigningCredentials = creds,
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }

    private string GenerateRefreshToken()
    {
        // var randomBytes = new byte[32];
        Guid id = Guid.NewGuid();

        return id.ToString();
    }

    public async Task<bool> SaveRefreshToken(string userId, string refreshToken)
    {
        var ip = _http.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? _http.HttpContext.Connection.RemoteIpAddress?.ToString();
        var newRefreshToken = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            Createddate = DateTime.UtcNow,
            Ipaddress = ip,
            Expires = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"])),
            IsRevoked = false
        };

        await _dbContext.RefreshTokens.AddAsync(newRefreshToken);

        return true;
    }

    public async Task<bool> RevokeRefreshToken(string userId)
    {
        var entitites = await _dbContext.RefreshTokens.Where(f => f.UserId == userId && f.IsRevoked == false).ToListAsync();

        foreach (var entity in entitites)
        {
            entity.IsRevoked = true;
            entity.Revokedbyip = _http.HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        return true;
    }
}