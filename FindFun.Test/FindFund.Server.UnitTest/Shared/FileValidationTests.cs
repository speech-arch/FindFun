using FindFun.Server.Shared.File;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace FindFund.Server.UnitTest.Shared;

public class FileValidationTests
{
    [Fact]
    public void ValidateFile_WhenFileTooLarge_ReturnsValidationResult()
    {
        var file = Substitute.For<IFormFile>();
        file.Length.Returns((10 << 20) + 1); // > 10 MB
        file.FileName.Returns("large.jpg");

        var result = FileValidation.ValidateFile(file).ToList();

        result.Should().ContainSingle();
        var vr = result.Single();
        vr.ErrorMessage.Should().Be("file exceeded or is below the permitted size.");
        vr.MemberNames.Should().Contain("file");
    }

    [Fact]
    public void ValidateFile_WhenInvalidExtension_ReturnsValidationResult()
    {
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(1024);
        file.FileName.Returns("badfile.txt");

        var result = FileValidation.ValidateFile(file).ToList();

        result.Should().ContainSingle();
        var vr = result.Single();
        vr.ErrorMessage.Should().Be("file has an invalid file extension.");
        vr.MemberNames.Should().Contain("file");
    }

    [Fact]
    public void ValidateFile_WhenValidFile_ReturnsNoValidationResult()
    {
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(1024);
        file.FileName.Returns("good.PNG");

        var result = FileValidation.ValidateFile(file).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateFiles_WhenNoFilesProvided_ReturnsValidationResult()
    {
        IFormFileCollection files = Substitute.For<IFormFileCollection>();
        files.Count.Returns(0);
        var results = FileValidation.ValidateFiles(files).ToList();

        results.Should().ContainSingle();
        var vr = results.Single();
        vr.ErrorMessage.Should().Be("files No files were provided.");
        vr.MemberNames.Should().Contain("files");
    }
    [Fact]
    public void ValidateFiles_WhenSomeInvalidFileProvided_ReturnsValidationResult()
    {
        var validFile = Substitute.For<IFormFile>();
        validFile.Length.Returns(1024);
        validFile.FileName.Returns("good.PNG");
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(1024);
        file.FileName.Returns("badfile.txt");
        var results = FileValidation.ValidateFiles(new FormFileCollection { validFile, file }).ToList();
        results.Should().ContainSingle();
    }

    [Fact]
    public void GetRelativePathFromUrl_WhenAbsoluteUrl_ReturnsTrimmedPath()
    {
        var url = "https://host/some/path/image.png";
        var rel = FileValidation.GetRelativePathFromUrl(url);
        rel.Should().Be("some/path/image.png");
    }

    [Fact]
    public void GetRelativePathFromUrl_WhenRelative_ReturnsUnchanged()
    {
        var url = "folder/image.png";
        var rel = FileValidation.GetRelativePathFromUrl(url);
        rel.Should().Be(url);
    }
}
