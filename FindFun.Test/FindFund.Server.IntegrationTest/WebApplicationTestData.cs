using System.Net.Http.Headers;
using FindFun.Server.Shared;
using FindFun.Server.Shared.File;
using Microsoft.AspNetCore.Http;

namespace FindFund.Server.IntegrationTest;

public static class WebApplicationTestData
{
    public static IEnumerable<object[]> InvalidRequestData()
    {
        yield return new object[]
        {
            new RequestCaseData(
                Locality: "AnyLocality",
                FormFieldName: "ParkImages",
                FileName: "invalid.txt",
                FileBytes: [0x00, 0x01, 0x02],
                ContentType: "text/plain",
                ExpectedErrorKey: "file",
                ExpectedErrorMessage: "file has an invalid file extension.")
        };

        yield return new object[]
        {
            new RequestCaseData(
                Locality: "AnyLocality",
                FormFieldName: "ParkImages",
                FileName: "empty.png",
                FileBytes: [],
                ContentType: "image/png",
                ExpectedErrorKey: "file",
                ExpectedErrorMessage: "file exceeded or is below the permitted size.")
        };

        yield return new object[]
        {
            new RequestCaseData(
                Locality: "AnyLocality",
                FormFieldName: "ParkImages",
                FileName: "image.png",
                FileBytes: [0x00, 0x01, 0x02],
                ContentType: "image/png",
                ExpectedErrorKey: "Coordinates",
                ExpectedErrorMessage: "Invalid coordinate format.",
                Coordinates: "12")
        };

        yield return new object[]
        {
            new RequestCaseData(
                Locality: "AnyLocality",
                FormFieldName: "ParkImages",
                FileName: "image.png",
                FileBytes: [0x00, 0x01, 0x02],
                ContentType: "image/png",
                ExpectedErrorKey: "EntranceFee",
                ExpectedErrorMessage: "Entrance fee must be greater than 0.",
                EntranceFee: "0")
        };
    }

    public static IEnumerable<object[]> ValidFileData()
    {
        yield return new object[] {
            new RequestCaseData(
                Locality: "AnyLocality",
                FormFieldName: "ParkImages",
                FileName: "image.png",
                FileBytes: [0x89, 0x50, 0x4E, 0x47],
                ContentType: "image/png",
                ExpectedErrorKey: null,
                ExpectedErrorMessage: null)
        };
        yield return new object[] {
            new RequestCaseData(
                Locality: "AnyLocality",
                FormFieldName: "ParkImages",
                FileName: "photo.jpg",
                FileBytes: [0xFF, 0xD8, 0xFF],
                ContentType: "image/jpeg",
                ExpectedErrorKey: null,
                ExpectedErrorMessage: null)
        };
    }
    public static async Task<HttpResponseMessage> PostAsync(RequestCaseData requestCase, WebAplicationCustomFactory factory, HttpClient httpClient)
    {
        await factory.AddMunicipality();
        var multipart = WebApplicationTestData.CreateBaseMultipart(factory.MunicipalityName, requestCase);
        AddFiles(requestCase.FormFieldName!, requestCase.FileName!, requestCase.FileBytes!, requestCase.ContentType!, multipart);
        var response = await httpClient.PostAsync("/api/parks", multipart);
        return response;
    }
    public static void AddFiles(string formFieldName, string fileName, byte[] fileBytes, string contentType, MultipartFormDataContent multipart)
    {
        var byteContent = new ByteArrayContent(fileBytes);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        multipart.Add(byteContent, formFieldName, fileName);
    }
    public static async Task<List<Result<string>>> UpLoadFile(FileUpLoad fileUpLoad)
    {

        var bytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        var ms = new MemoryStream(bytes);
        IFormFile file = new FormFile(ms, 0, ms.Length, "ParkImages", "test.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
        var files = new FormFileCollection { file };

        List<Result<string>>? results = await fileUpLoad.FilesUpLoader(files, "integration-tests", CancellationToken.None);
        return results;
    }
    public static MultipartFormDataContent CreateBaseMultipart(string locality, RequestCaseData requestCaseData)
    {
        return new MultipartFormDataContent
        {
            { new StringContent(requestCaseData.ParkName), "Name" },
            { new StringContent("A nice park"), "Description" },
            { new StringContent("Tester"), "Organizer" },
            { new StringContent(requestCaseData.IsFree.ToString()), "IsFree" },
            { new StringContent(requestCaseData.EntranceFee), "EntranceFee" },
            { new StringContent(requestCaseData.AmenityGroup), "Amenities" },
            { new StringContent("Public"), "ParkType" },
            { new StringContent(requestCaseData.ClosingSchedule), "ClosingSchedule" },
            { new StringContent(requestCaseData.Coordinates), "Coordinates" },
            { new StringContent("Some formatted address"), "FormattedAddress" },
            { new StringContent("Main Street"), "Street" },
            { new StringContent("1"), "Number" },
            { new StringContent("ABC123"), "AgeRecommendation" },
            { new StringContent(locality), "Locality" },
            { new StringContent("12345"), "PostalCode" }
        };
    }
}

public sealed record RequestCaseData(
    string Locality,
    string? FormFieldName,
    string? FileName,
    byte[]? FileBytes,
    string? ContentType,
    string? ExpectedErrorKey,
    string? ExpectedErrorMessage,
    string AmenityGroup = "Playground:Children area",
    string Coordinates = "-3.70379,40.41678",
    string ClosingSchedule = "",
    string EntranceFee = "5.00",
    bool IsFree = false,
    string ParkName = "Test Park");

