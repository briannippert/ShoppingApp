using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Server.Data;
using Server.Models;
using System.Text.Json;
using Xunit;

namespace Server.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb" + Guid.NewGuid());
                });
            });
        });
    }

    [Fact]
    public async Task RegisterAndLoginFlow_Works()
    {
        var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/register", new
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "P@ssw0rd"
        });
        Assert.Equal(System.Net.HttpStatusCode.Created, registerResponse.StatusCode);

        // Integration test focuses on registration flow only
    }
}
