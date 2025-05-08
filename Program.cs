
using Microsoft.EntityFrameworkCore;
using atmnr_api.Services;
using Mapster;
using MapsterMapper;
using atmnr_api.Models.Mappings;
using atmnr_api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using atmnr_api.Helpers;
using atmnr_api.SignalRHub;


var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

var allowedOrigins = configuration.GetSection("Cors")["AllowedOrigins"];

services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder
                .AllowCredentials()
                .WithOrigins(allowedOrigins.Split(';'))
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

MapsterConfig.Register();
services.AddMapster();

// var mapperConfig = new MapperConfiguration(mc =>
// {
//     mc.AddProfile(new Domain2ModelMappingProfile());
//     mc.AddProfile(new Model2DomainMappingProfile());
// });
// services.AddSingleton(mapperConfig.CreateMapper());

builder.Services.AddSignalR();

builder.Services.AddHostedService<ApartmentExpireJob>();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
        // options.JsonSerializerOptions.Converters.Add(new GuidJsonConverter());
    });
services.AddDbContext<AtDbContext>(options =>
{
    options
    .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found."));

});
ServiceHelper.AddServiceSys(services);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[]{}
    }});

    // c.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetSection("JwtSettings:Issuer").Value!,
            ValidAudience = builder.Configuration.GetSection("JwtSettings:Audience").Value!,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                    .GetBytes(builder.Configuration.GetSection("JwtSettings:Key").Value!)),
            ClockSkew = TimeSpan.Zero


        };
    });

builder.Services.AddHttpContextAccessor();
services.AddHttpContextAccessor();

//For - ubuntu
// builder.WebHost.ConfigureKestrel(serverOptions =>
// {
//     serverOptions.ListenAnyIP(5000); // Cho HTTP
// });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// app.MapHub<AlertHub>("/alerthub");
app.MapHub<AlertHub>("/api/alerthub");
app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
