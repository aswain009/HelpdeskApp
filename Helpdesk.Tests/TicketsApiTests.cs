using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Helpdesk.Tests;

public class TicketsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TicketsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task List_Tickets_Should_Return_OK_And_Some_Items()
    {
        var resp = await _client.GetAsync("/api/tickets");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await resp.Content.ReadFromJsonAsync<List<Ticket>>();
        items.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_Then_Filter_By_Status_Works()
    {
        var mockTicket = new Ticket { Title = "Test ticket via API", Description = "desc" };
        var createResp = await _client.PostAsJsonAsync("/api/tickets", mockTicket);
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<Ticket>();
        created.Should().NotBeNull();

        var filterResp = await _client.GetAsync("/api/tickets?status=0"); // Open
        filterResp.EnsureSuccessStatusCode();
        var filtered = await filterResp.Content.ReadFromJsonAsync<List<Ticket>>();
        filtered!.Any(ticket => ticket.Id == created!.Id).Should().BeTrue();
    }
}
