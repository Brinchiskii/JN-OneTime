using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OneTime.Api.Tests.TestHelpers;
using OneTime.Core.Models;
using Xunit;

namespace OneTime.Api.Tests.Endpoints;

public class ProjectsControllerTests
{
    private static HttpClient CreateClient()
    {
        var factory = new OneTimeApiFactory();
        return factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns_Ok_With_Projects()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("api/Projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var projects = await response.Content.ReadFromJsonAsync<List<Project>>();
        Assert.NotNull(projects);
        Assert.NotEmpty(projects!);
        
        Assert.Contains(projects!, p => p.ProjectId == 1 && p.Name == "Test Project 1");
        Assert.Contains(projects!, p => p.ProjectId == 2 && p.Name == "Test Project 2");
    }

    [Fact]
    public async Task GetAll_Returns_NoContent_When_No_Projects()
    {
        using var factory = new EmptyProjectsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("api/Projects");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    /// <summary>
    /// Test-only factory that starts from the normal seeded DB, then removes all projects
    /// to validate the controller's 204 NoContent branch.
    /// </summary>
    private sealed class EmptyProjectsApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationsTesting");

            builder.ConfigureServices(services =>
            {
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<OneTimeContext>();

                // Reuse the same reset behavior as OneTimeApiFactory
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Seed the usual data first (so all required entities/config are present),
                // then remove projects to simulate an empty Projects table.
                SeedFromMainFactory(context);

                context.Projects.RemoveRange(context.Projects);
                context.SaveChanges();
            });
        }

        private static void SeedFromMainFactory(OneTimeContext context)
        {
            // Minimal inline seed matching OneTimeApiFactory's projects seed
            // (we only need projects to exist before we delete them)
            context.Projects.AddRange(
                new Project { ProjectId = 1, Name = "Test Project 1" },
                new Project { ProjectId = 2, Name = "Test Project 2" }
            );
            context.SaveChanges();
        }
    }
}