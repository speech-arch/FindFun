using System.Net;
using System.Net.Http.Json;
using FindFun.Server.Features.Parks.Create;
using FindFun.Server.Features.Parks.Get;
using FluentAssertions;

namespace FindFund.Server.IntegrationTest;

public class GetParksIntegrationTest : IClassFixture<WebAplicationCustomFactory>
{
    private readonly WebAplicationCustomFactory _factory;
    private readonly HttpClient _httpClient;
    public GetParksIntegrationTest(WebAplicationCustomFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }
    [Theory]
    [MemberData(nameof(WebApplicationTestData.ValidFileData), MemberType = typeof(WebApplicationTestData))]
    public async Task GetParks_ShouldReturnOkWithParks_WhenParksExist(RequestCaseData requestCaseData)
    {
        HttpResponseMessage response = await WebApplicationTestData.PostAsync(requestCaseData, _factory, _httpClient);
        // Assert
        response.EnsureSuccessStatusCode();
        var createParkresponse = await response.Content.ReadFromJsonAsync<CreateParkResponse>();
        createParkresponse.Should().NotBeNull();
        createParkresponse.ParkId.Should().BeGreaterThan(0);
        var parksResponse = await _httpClient.GetAsync("/api/parks");
        parksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var parks = await parksResponse.Content.ReadFromJsonAsync<PagedParksResponse>();
        parks.Should().NotBeNull();
        parks!.Items.Should().NotBeEmpty();
        parks!.Items.Count.Should().BeGreaterThan(0);
        parks.Items.Should().ContainSingle(p => p.Id == createParkresponse.ParkId.ToString());
    }
    [Fact]
    public async Task GetParks_ShouldReturnOkWithEmptyList_WhenNoParksExist()
    {
        var parksResponse = await _httpClient.GetAsync("/api/parks");
        parksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var parks = await parksResponse.Content.ReadFromJsonAsync<PagedParksResponse>();
        parks.Should().NotBeNull();
        parks!.Items.Should().BeEmpty();
        parks.TotalCount.Should().Be(0);
    }
}
