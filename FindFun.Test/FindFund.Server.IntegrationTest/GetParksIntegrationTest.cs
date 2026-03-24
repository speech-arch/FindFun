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

    [Fact]
    public async Task GetParks_ShouldSupportSearchFilter()
    {
        // arrange - create two parks via API to keep the test end-to-end
        var blueRequest = new RequestCaseData(
            Locality: string.Empty,
            FormFieldName: "ParkImages",
            FileName: "image.png",
            FileBytes: [0x89, 0x50, 0x4E, 0x47],
            ContentType: "image/png",
            ExpectedErrorKey: null,
            ExpectedErrorMessage: null,
            AmenityGroup: "Playground:Children area",
            Coordinates: "-3.70379,40.41678",
            ClosingSchedule: string.Empty,
            EntranceFee: "5.00",
            IsFree: false,
            ParkName: "Blue Park");

        var redRequest = blueRequest with { ParkName = "Red Park", Coordinates = "-3.70380,40.41679" };

        var blueResponse = await WebApplicationTestData.PostAsync(blueRequest, _factory, _httpClient);
        blueResponse.EnsureSuccessStatusCode();
        var redResponse = await WebApplicationTestData.PostAsync(redRequest, _factory, _httpClient);
        redResponse.EnsureSuccessStatusCode();

        // act
        var parksResponse = await _httpClient.GetAsync("/api/parks?search=Blue");

        // assert
        parksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var parks = await parksResponse.Content.ReadFromJsonAsync<PagedParksResponse>();
        parks.Should().NotBeNull();
        parks!.Items.Should().ContainSingle(p => p.Name == "Blue Park");
    }

    [Fact]
    public async Task GetParks_ShouldFilterByName()
    {
        // arrange - create two parks via API (each post creates its own municipality)
        var parkARequest = new RequestCaseData(
            Locality: string.Empty,
            FormFieldName: "ParkImages",
            FileName: "image.png",
            FileBytes: [0x89, 0x50, 0x4E, 0x47],
            ContentType: "image/png",
            ExpectedErrorKey: null,
            ExpectedErrorMessage: null,
            ParkName: "Park A");

        var parkBRequest = parkARequest with { ParkName = "Park B" };

        var responseA = await WebApplicationTestData.PostAsync(parkARequest, _factory, _httpClient);
        responseA.EnsureSuccessStatusCode();
        var createA = await responseA.Content.ReadFromJsonAsync<CreateParkResponse>();
        var parkA = await _factory.GetParkByIdAsync(createA!.ParkId);

        var responseB = await WebApplicationTestData.PostAsync(parkBRequest, _factory, _httpClient);
        responseB.EnsureSuccessStatusCode();

        // act - query for parks by name of park A
        var parksResponse = await _httpClient.GetAsync($"/api/parks?search={parkA!.Name}");

        // assert
        parksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var parks = await parksResponse.Content.ReadFromJsonAsync<PagedParksResponse>();
        parks.Should().NotBeNull();
        parks!.Items.Should().OnlyContain(p => p.Name == parkA!.Name);
        parks.Items.Should().ContainSingle(p => p.Id == parkA.Id.ToString());
    }

    [Fact]
    public async Task GetParks_ShouldSupportPaginationAndSortingByName()
    {
        // arrange - create 15 parks via API
        for (int i = 0; i < 15; i++)
        {
            var req = new RequestCaseData(
                Locality: string.Empty,
                FormFieldName: "ParkImages",
                FileName: "image.png",
                FileBytes: [0x89, 0x50, 0x4E, 0x47],
                ContentType: "image/png",
                ExpectedErrorKey: null,
                ExpectedErrorMessage: null,
                ParkName: $"Park{i}");

            var resp = await WebApplicationTestData.PostAsync(req, _factory, _httpClient);
            resp.EnsureSuccessStatusCode();
        }

        var parksResponse = await _httpClient.GetAsync("/api/parks?page=2&pageSize=10&sortBy=name&sortDirection=desc");

        // assert
        parksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var parks = await parksResponse.Content.ReadFromJsonAsync<PagedParksResponse>();
        parks.Should().NotBeNull();
        parks!.Page.Should().Be(2);
        parks.PageSize.Should().Be(10);
        parks.Items.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetParks_ShouldFilterByRadiusAndSortByDistance()
    {
        // arrange - create near and far parks via API
        var nearReq = new RequestCaseData(
            Locality: string.Empty,
            FormFieldName: "ParkImages",
            FileName: "image.png",
            FileBytes: [0x89, 0x50, 0x4E, 0x47],
            ContentType: "image/png",
            ExpectedErrorKey: null,
            ExpectedErrorMessage: null,
            Coordinates: "-3.70379,40.41678",
            ParkName: "Near Park");

        var farReq = nearReq with { Coordinates = "-3.80,40.42", ParkName = "Far Park" };

        var nearResp = await WebApplicationTestData.PostAsync(nearReq, _factory, _httpClient);
        nearResp.EnsureSuccessStatusCode();
        var farResp = await WebApplicationTestData.PostAsync(farReq, _factory, _httpClient);
        farResp.EnsureSuccessStatusCode();

        // act - search around the near park coordinates with small radius
        var latitude = 40.41678;
        var longitude = -3.70379;
        var parksResponse = await _httpClient.GetAsync($"/api/parks?latitude={latitude}&longitude={longitude}&radiusKm=1");

        // assert
        parksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var parks = await parksResponse.Content.ReadFromJsonAsync<PagedParksResponse>();
        parks.Should().NotBeNull();
        parks!.Items.Should().Contain(p => p.Name == "Near Park");
        parks.Items.Should().NotContain(p => p.Name == "Far Park");
    }
}
