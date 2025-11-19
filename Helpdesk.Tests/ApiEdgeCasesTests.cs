using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Helpdesk.Tests;

public class ApiEdgeCasesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiEdgeCasesTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task Post_With_Nonexistent_AssignedUserId_Should_Return_400()
    {
        var payload = new { title = "Bad assign", description = "desc", status = TicketStatus.Open, assignedUserId = 999999 };
        var resp = await _client.PostAsJsonAsync("/api/tickets", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_With_Nonexistent_AssignedUserId_Should_Return_400()
    {
        // create
        var created = await (await _client.PostAsJsonAsync("/api/tickets", new { title = "to update", description = (string?)null, status = TicketStatus.Open, assignedUserId = (int?)null }))
            .Content.ReadFromJsonAsync<Ticket>();

        var updatePayload = new { id = created!.Id, title = created.Title, description = created.Description, status = created.Status, assignedUserId = 888888 };
        var resp = await _client.PutAsJsonAsync($"/api/tickets/{created.Id}", updatePayload);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_With_Id_Mismatch_Should_Return_400()
    {
        var created = await (await _client.PostAsJsonAsync("/api/tickets", new { title = "mismatch", description = (string?)null, status = TicketStatus.Open, assignedUserId = (int?)null }))
            .Content.ReadFromJsonAsync<Ticket>();

        var body = new { id = created!.Id + 1, title = created.Title, description = created.Description, status = created.Status, assignedUserId = created.AssignedUserId };
        var resp = await _client.PutAsJsonAsync($"/api/tickets/{created.Id}", body);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Filter_By_AssignedUserId_Should_Work()
    {
        // get a valid user
        var users = await _client.GetFromJsonAsync<List<User>>("/api/users");
        users!.Count.Should().BeGreaterThan(0);
        var userId = users.First().Id;

        // create ticket
        var ticket = await (await _client.PostAsJsonAsync("/api/tickets", new { title = "assign-filter", description = (string?)null, status = TicketStatus.Open, assignedUserId = (int?)null }))
            .Content.ReadFromJsonAsync<Ticket>();

        // assign
        var assignResp = await _client.PostAsJsonAsync($"/api/tickets/{ticket!.Id}/assign", new { assignedUserId = (int?)userId });
        assignResp.EnsureSuccessStatusCode();

        // filter should include
        var filtered = await _client.GetFromJsonAsync<List<Ticket>>($"/api/tickets?assignedUserId={userId}");
        filtered!.Any(t => t.Id == ticket.Id).Should().BeTrue();

        // filter by someone else should exclude
        var otherId = users.Max(u => u.Id) + 9999; // non-existing id
        var filteredNone = await _client.GetFromJsonAsync<List<Ticket>>($"/api/tickets?assignedUserId={otherId}");
        filteredNone!.Any(t => t.Id == ticket.Id).Should().BeFalse();
    }
    [Fact]
    public async Task Assign_On_Nonexistent_Ticket_Should_Return_404()
    {
        var resp = await _client.PostAsJsonAsync("/api/tickets/999999/assign", new { assignedUserId = (int?)null });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Nonexistent_Ticket_Should_Return_404()
    {
        var resp = await _client.DeleteAsync("/api/tickets/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_Nonexistent_Ticket_Should_Return_404()
    {
        var resp = await _client.GetAsync("/api/tickets/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Additional edge-case tests (e.g., model validation or FK checks) can be added later once
    // test-host behavior is harmonized across environments.
}
