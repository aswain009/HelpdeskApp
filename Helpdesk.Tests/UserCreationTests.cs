using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Helpdesk.Tests;

public class UserCreationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserCreationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task Users_List_Should_Return_Seeded_And_Sorted()
    {
        var users = await _client.GetFromJsonAsync<List<User>>("/api/users");
        users.Should().NotBeNull();
        users!.Count.Should().BeGreaterThan(0);

        // Assert sorted by Name ascending
        var names = users.Select(u => u.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Get_User_ById_Should_Return_404_When_Not_Found()
    {
        var resp = await _client.GetAsync("/api/users/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Posting_Ticket_With_Inline_AssignedUser_Should_Not_Create_User()
    {
        // get baseline user count
        var before = await _client.GetFromJsonAsync<List<User>>("/api/users");
        var beforeCount = before!.Count;

        // try to create a ticket including a navigation object 'assignedUser'
        var payload = new
        {
            title = "Ticket with inline user (should be ignored)",
            description = (string?)null,
            status = TicketStatus.Open,
            assignedUser = new { name = "Inline User", email = "inline@example.com" }
        };

        var createResp = await _client.PostAsJsonAsync("/api/tickets", payload);
        // Endpoint should succeed and ignore the extra 'assignedUser' field
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<Ticket>();
        created.Should().NotBeNull();
        created!.AssignedUserId.Should().BeNull();

        // user count should remain the same (no new user created)
        var after = await _client.GetFromJsonAsync<List<User>>("/api/users");
        after!.Count.Should().Be(beforeCount);
    }

    [Fact]
    public async Task Posting_Ticket_With_AssignedUserId_Should_Not_Create_New_User()
    {
        // pick an existing user id
        var users = await _client.GetFromJsonAsync<List<User>>("/api/users");
        users!.Count.Should().BeGreaterThan(0);
        var beforeCount = users.Count;
        var userId = users.First().Id;

        var payload = new { title = "Assign existing user", description = (string?)null, status = TicketStatus.Open, assignedUserId = (int?)userId };
        var resp = await _client.PostAsJsonAsync("/api/tickets", payload);
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<Ticket>();
        created.Should().NotBeNull();
        created!.AssignedUserId.Should().Be(userId);

        // Ensure user count unchanged
        var after = await _client.GetFromJsonAsync<List<User>>("/api/users");
        after!.Count.Should().Be(beforeCount);
    }

    [Fact]
    public async Task Users_Post_Is_Not_Supported()
    {
        var resp = await _client.PostAsJsonAsync("/api/users", new { name = "X", email = "x@example.com" });
        // No POST endpoint exists; should not be successful (likely 404)
        resp.IsSuccessStatusCode.Should().BeFalse();
    }
}