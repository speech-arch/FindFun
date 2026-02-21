using FindFun.Server.Shared;
using FindFun.Server.Shared.Validations;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FindFund.Server.IntegrationTest;

public class CreateParkIntegrationTests : IClassFixture<WebAplicationCustomFactory>
{
    private readonly WebAplicationCustomFactory _factory;
    private readonly HttpClient _httpClient;
    private const string BadRequest = "Bad Request";
    public CreateParkIntegrationTests(WebAplicationCustomFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }
    [Fact]
    public async Task CreatePark_ShouldReturnBadRequest_WhenNoDataProvided()
    {
        var response = await _httpClient.PostAsync("/parks", new MultipartFormDataContent());
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails.Status.Should().Be((int)response.StatusCode);
        validationProblemDetails.Title.Should().Be(BadRequest);
        validationProblemDetails.Type.Should().Be(ProblemDetailsConstants.BadRequest);
    }
    [Fact]
    public async Task CreatePark_ShouldReturnBadRequest_WhenRequiredFieldsMissing()
    {
        var multipart = new MultipartFormDataContent
        {
            { new StringContent("Test Park"), "Name" },
            { new StringContent("A nice park"), "Description" }
        };
        var response = await _httpClient.PostAsync("/parks", multipart);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails.Status.Should().Be((int)response.StatusCode);
        validationProblemDetails.Title.Should().Be(BadRequest);
    }

    [Theory]
    [MemberData(nameof(WebApplicationTestData.ValidFileData), MemberType = typeof(WebApplicationTestData))]
    public async Task CreatePark_ShouldReturnBadRequest_WhenValidationFails(RequestCaseData requestCaseData)
    {
        var multipart = WebApplicationTestData.CreateBaseMultipart(string.Empty, requestCaseData);
        var response = await _httpClient.PostAsync("/parks", multipart);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails.Status.Should().Be((int)response.StatusCode);
        validationProblemDetails.Title.Should().Be("One or more validation errors occurred.", "the locality cannot be empty");
    }
    [Theory]
    [MemberData(nameof(WebApplicationTestData.ValidFileData), MemberType = typeof(WebApplicationTestData))]
    public async Task CreatePark_ShouldReturnBadRequest_WhenLocalityNotFound(RequestCaseData testCase)
    {
        var multipart = WebApplicationTestData.CreateBaseMultipart("NonExistentLocality", testCase);
        AddFiles(testCase.FormFieldName!, testCase.FileName!, testCase.FileBytes!, testCase.ContentType!, multipart);

        var response = await _httpClient.PostAsync("/parks", multipart);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails.Status.Should().Be((int)response.StatusCode);
        validationProblemDetails.Title.Should().Be(BadRequest);
        validationProblemDetails!.Errors.Should().ContainKey("Locality").And.HaveCount(1);
        var localityErrors = validationProblemDetails.Errors["Locality"];
        localityErrors.Should().HaveCount(1).And.Contain("Locality not found.");
    }

    [Theory]
    [MemberData(nameof(WebApplicationTestData.ValidFileData), MemberType = typeof(WebApplicationTestData))]
    public async Task CreatePark_ShouldCreates_WhenValidDataProvided_ReturnsId(RequestCaseData requestCaseData)
    {
        HttpResponseMessage response = await PostAsync(requestCaseData);
        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var createdId = JsonSerializer.Deserialize<int>(responseString);
        createdId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreatePark_ShouldReturnConflict_WhenAddressAlreadyExists()
    {
        await _factory.AddMunicipality();
        await _factory.AddParkWithAddressAsync(_factory.MunicipalityName);


        var requestCaseData = new RequestCaseData(
            Locality: _factory.MunicipalityName,
            FormFieldName: "ParkImages",
            FileName: "image.png",
            FileBytes: [0x89, 0x50, 0x4E, 0x47],
            ContentType: "image/png",
            ExpectedErrorKey: null,
            ExpectedErrorMessage: null);

        var multipart = WebApplicationTestData.CreateBaseMultipart(_factory.MunicipalityName, requestCaseData);
        AddFiles("ParkImages", "image.png", [0x89, 0x50, 0x4E, 0x47], "image/png", multipart);
        var response = await _httpClient.PostAsync("/parks", multipart);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Errors.Should().ContainKey("Address");
    }

    [Theory]
    [MemberData(nameof(WebApplicationTestData.InvalidRequestData), MemberType = typeof(WebApplicationTestData))]
    public async Task CreatePark_ShouldReturnBadRequest_ForVariousInvalidRequests(RequestCaseData testCase)
    {

        await _factory.AddMunicipality();

        var multipart = WebApplicationTestData.CreateBaseMultipart(_factory.MunicipalityName, testCase);

        AddFiles(testCase.FormFieldName!, testCase.FileName!, testCase.FileBytes!, testCase.ContentType!, multipart);

        var response = await _httpClient.PostAsync("/parks", multipart);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails.Status.Should().Be((int)response.StatusCode);

        if (!string.IsNullOrEmpty(testCase.ExpectedErrorKey))
        {
            validationProblemDetails!.Errors.Should().ContainKey(testCase.ExpectedErrorKey).And.HaveCount(1);
            var errors = validationProblemDetails.Errors[testCase.ExpectedErrorKey];
            errors.Should().HaveCount(1).And.Contain(testCase.ExpectedErrorMessage);
        }
    }
    [Theory]
    [MemberData(nameof(WebApplicationTestData.ValidFileData), MemberType = typeof(WebApplicationTestData))]
    public async Task CreatePark_Persists_ClosingSchedule_Images_And_Amenity(RequestCaseData requestCaseData)
    {
        var requestWithSchedule = requestCaseData with { ClosingSchedule = "Mon:09-17" };

        // Act
        var response = await PostAsync(requestWithSchedule);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var createdId = JsonSerializer.Deserialize<int>(responseString);
        createdId.Should().BeGreaterThan(0);

        var park = await _factory.GetParkByIdAsync(createdId);

        park.Should().NotBeNull("park should be created and retrievable from DB");
        park!.ClosingSchedule.Should().NotBeNull("closing schedule should be saved when provided");
        park.Images.Should().NotBeNull().And.NotBeEmpty("uploaded images should be associated with the park");
        park.Amenities.Should().NotBeNull().And.NotBeEmpty("amenity should be added to the park");
        park.Amenities.First().Amenity.Should().NotBeNull();
        park.Amenities.First().Amenity!.Name.Should().Be(ValidationHelper.ParseAmenityGroup(requestWithSchedule.AmenityGroup).Data.Item1);
    }

    private async Task<HttpResponseMessage> PostAsync(RequestCaseData requestCase)
    {
        await _factory.AddMunicipality();
        var multipart = WebApplicationTestData.CreateBaseMultipart(_factory.MunicipalityName, requestCase);
        AddFiles(requestCase.FormFieldName!, requestCase.FileName!, requestCase.FileBytes!, requestCase.ContentType!, multipart);
        var response = await _httpClient.PostAsync("/parks", multipart);
        return response;
    }

    private static void AddFiles(string formFieldName, string fileName, byte[] fileBytes, string contentType, MultipartFormDataContent multipart)
    {
        var byteContent = new ByteArrayContent(fileBytes);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        multipart.Add(byteContent, formFieldName, fileName);
    }

}
