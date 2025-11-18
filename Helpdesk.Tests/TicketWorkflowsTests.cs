using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Helpdesk.Tests;

public class TicketWorkflowsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TicketWorkflowsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task Create_Ticket_Should_Succeed_And_Default_To_Open()
    {
        var toCreate = new Ticket { Title = "API create test", Description = "desc" };
        var resp = await _client.PostAsJsonAsync("/api/tickets", toCreate);
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<Ticket>();
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Status.Should().Be(TicketStatus.Open);
        created.CreatedAt.Should().NotBe(default);
        created.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Close_Ticket_Should_Update_Status_And_Timestamp()
    {
        // create first
        var createResp = await _client.PostAsJsonAsync("/api/tickets", new Ticket { Title = "Close me" });
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<Ticket>();
        var originalUpdated = created!.UpdatedAt;

        // update to Closed
        created.Status = TicketStatus.Closed;
        var updateResp = await _client.PutAsJsonAsync($"/api/tickets/{created.Id}", created);
        updateResp.EnsureSuccessStatusCode();
        var updated = await updateResp.Content.ReadFromJsonAsync<Ticket>();
        updated!.Status.Should().Be(TicketStatus.Closed);
        updated.UpdatedAt.Should().BeAfter(originalUpdated);
    }

    [Fact]
    public async Task Change_Assignee_Should_Succeed_And_Validate_User()
    {
        // get a valid user id
        var users = await _client.GetFromJsonAsync<List<User>>("/api/users");
        users!.Count.Should().BeGreaterThan(0);
        var userId = users.First().Id;

        // create ticket
        var created = await (await _client.PostAsJsonAsync("/api/tickets", new Ticket { Title = "Assign me" })).Content
            .ReadFromJsonAsync<Ticket>();

        // assign to valid user
        var assignOk = await _client.PostAsJsonAsync($"/api/tickets/{created!.Id}/assign", new { AssignedUserId = (int?)userId });
        assignOk.EnsureSuccessStatusCode();
        var assigned = await assignOk.Content.ReadFromJsonAsync<Ticket>();
        assigned!.AssignedUserId.Should().Be(userId);

        // assign to non-existent user -> 400
        var badAssign = await _client.PostAsJsonAsync($"/api/tickets/{created!.Id}/assign", new { AssignedUserId = (int?)999999 });
        badAssign.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // unassign -> null
        var unassign = await _client.PostAsJsonAsync($"/api/tickets/{created!.Id}/assign", new { AssignedUserId = (int?)null });
        unassign.EnsureSuccessStatusCode();
        var unassigned = await unassign.Content.ReadFromJsonAsync<Ticket>();
        unassigned!.AssignedUserId.Should().BeNull();
    }

    [Fact]
    public async Task Change_Status_In_Progress_Should_Filter_Correctly()
    {
        var created = await (await _client.PostAsJsonAsync("/api/tickets", new Ticket { Title = "Change status" })).Content
            .ReadFromJsonAsync<Ticket>();
        created!.Status = TicketStatus.InProgress;
        var updateResp = await _client.PutAsJsonAsync($"/api/tickets/{created.Id}", created);
        updateResp.EnsureSuccessStatusCode();

        var filtered = await _client.GetFromJsonAsync<List<Ticket>>("/api/tickets?status=InProgress");
        filtered!.Any(ticket => ticket.Id == created.Id && ticket.Status == TicketStatus.InProgress).Should().BeTrue();
    }
}
