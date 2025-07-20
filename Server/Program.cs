using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("ShoppingApp"); // Replace with UseSqlServer in production
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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

    // For demo purposes we just return a dummy token
    var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    return Results.Ok(new { token });
});

app.Run();

record UserDto(string FullName, string Email, string Password);
record LoginDto(string Email, string Password);
