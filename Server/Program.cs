using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Server.Data;
using Server.Models;
using BCrypt.Net;

string GenerateJwtToken(User user, string secret)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(secret);
    var descriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email)
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(descriptor);
    return tokenHandler.WriteToken(token);
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("ShoppingApp"); // Replace with UseSqlServer in production
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtSecret = builder.Configuration["JwtSecret"] ?? "supersecret";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/register", async (ApplicationDbContext db, UserDto userDto) =>
{
    if (await db.Users.AnyAsync(u => u.Email == userDto.Email))
    {
        return Results.BadRequest("Email already registered");
    }

    var user = new User
    {
        FullName = userDto.FullName,
        Email = userDto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/api/users/{user.Id}", new { user.Id, user.FullName, user.Email });
});

app.MapPost("/api/login", async (ApplicationDbContext db, LoginDto loginDto) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
    if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var token = GenerateJwtToken(user, jwtSecret);
    return Results.Ok(new { token });
});

app.MapGet("/api/profile", async (ClaimsPrincipal userPrincipal, ApplicationDbContext db) =>
{
    var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
    {
        return Results.Unauthorized();
    }

    if (!int.TryParse(userIdClaim.Value, out var userId))
    {
        return Results.Unauthorized();
    }

    var user = await db.Users.FindAsync(userId);
    if (user == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new { user.Id, user.FullName, user.Email });
}).RequireAuthorization();

app.MapPut("/api/profile", async (ClaimsPrincipal userPrincipal, ApplicationDbContext db, UpdateUserDto updateDto) =>
{
    var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
        return Results.Unauthorized();
    }

    var user = await db.Users.FindAsync(userId);
    if (user == null)
    {
        return Results.NotFound();
    }

    user.FullName = updateDto.FullName ?? user.FullName;
    user.Email = updateDto.Email ?? user.Email;
    await db.SaveChangesAsync();

    return Results.Ok(new { user.Id, user.FullName, user.Email });
}).RequireAuthorization();

app.MapPost("/api/change-password", async (ClaimsPrincipal userPrincipal, ApplicationDbContext db, ChangePasswordDto dto) =>
{
    var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
        return Results.Unauthorized();
    }

    var user = await db.Users.FindAsync(userId);
    if (user == null)
    {
        return Results.NotFound();
    }

    if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
    {
        return Results.BadRequest("Invalid old password");
    }

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
    await db.SaveChangesAsync();

    return Results.Ok();
}).RequireAuthorization();

app.Run();

record UserDto(string FullName, string Email, string Password);
record LoginDto(string Email, string Password);
record UpdateUserDto(string? FullName, string? Email);
record ChangePasswordDto(string OldPassword, string NewPassword);
