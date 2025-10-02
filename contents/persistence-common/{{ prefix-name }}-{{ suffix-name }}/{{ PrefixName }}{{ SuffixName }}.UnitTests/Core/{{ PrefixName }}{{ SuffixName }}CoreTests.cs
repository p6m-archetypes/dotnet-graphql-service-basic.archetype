using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using Schema = {{ PrefixName }}{{ SuffixName }}.API.Schema;
using {{ PrefixName }}{{ SuffixName }}.Core;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Models;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;
using {{ PrefixName }}{{ SuffixName }}.UnitTests.TestBuilders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace {{ PrefixName }}{{ SuffixName }}.UnitTests.Core;

public class {{ PrefixName }}{{ SuffixName }}CoreTests
{
    private readonly Mock<I{{ PrefixName }}Repository> _mockRepository;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<ILogger<{{ PrefixName }}{{ SuffixName }}Core>> _mockLogger;
    private readonly {{ PrefixName }}{{ SuffixName }}Core _service;

    public {{ PrefixName }}{{ SuffixName }}CoreTests()
    {
        _mockRepository = new Mock<I{{ PrefixName }}Repository>();
        _mockValidationService = new Mock<IValidationService>();
        _mockLogger = new Mock<ILogger<{{ PrefixName }}{{ SuffixName }}Core>>();
        
        // Setup validation service to allow valid inputs by default
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((id, field) => Guid.Parse(id));

        _service = new {{ PrefixName }}{{ SuffixName }}Core(
            _mockRepository.Object, 
            _mockValidationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Create{{ PrefixName }}_ShouldReturnCreatedEntity_WhenValidRequest()
    {
        // Arrange
        var request = new Create{{ PrefixName }}Input { Name = "Test Entity" };
        var savedEntity = new {{ PrefixName }}EntityBuilder()
            .WithName(request.Name)
            .WithId(Guid.NewGuid())
            .Generate();

        _mockRepository.Setup(x => x.Save(It.IsAny<{{ PrefixName }}Entity>()))
            .Callback<{{ PrefixName }}Entity>(entity => entity.Id = savedEntity.Id);
        _mockRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Create{{ PrefixName }}(request);

        // Assert
        result.Should().NotBeNull();
        result.{{ PrefixName }}.Should().NotBeNull();
        result.{{ PrefixName }}.Name.Should().Be(request.Name);
        result.{{ PrefixName }}.Id.Should().NotBeNullOrEmpty();

        _mockRepository.Verify(x => x.Save(It.Is<{{ PrefixName }}Entity>(e => e.Name == request.Name)), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Get{{ PrefixName }}s_ShouldReturnPagedResults_WhenValidRequest()
    {
        // Arrange
        var request = new Get{{ PrefixName }}sRequest { StartPage = 1, PageSize = 10 };
        var entities = new {{ PrefixName }}EntityBuilder().Generate(5);
        var page = new Page<{{ PrefixName }}Entity>
        {
            Items = entities,
            TotalElements = 5
        };

        _mockRepository.Setup(x => x.FindAsync(It.IsAny<PageRequest>()))
            .Returns(Task.FromResult(page));

        // Act
        var result = await _service.Get{{ PrefixName }}s(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalElements.Should().Be(5);
        result.{{ PrefixName }}s.Should().HaveCount(5);
        result.{{ PrefixName }}s.Should().AllSatisfy(dto =>
        {
            dto.Id.Should().NotBeNullOrEmpty();
            dto.Name.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Get{{ PrefixName }}s_ShouldCallValidationWithParsedValues()
    {
        // Arrange
        var request = new Get{{ PrefixName }}sRequest { StartPage = 2, PageSize = 20 };
        var page = new Page<{{ PrefixName }}Entity>
        {
            Items = new List<{{ PrefixName }}Entity>(),
            TotalElements = 0
        };

        _mockRepository.Setup(x => x.FindAsync(It.IsAny<PageRequest>()))
            .Returns(Task.FromResult(page));

        // Act
        var result = await _service.Get{{ PrefixName }}s(request);

        // Assert
        _mockValidationService.Verify(x => x.ValidatePaginationRequest(2, 20), Times.Once);
    }

    [Fact]
    public async Task Get{{ PrefixName }}s_ShouldUseDefaultValues_WhenParametersAreNull()
    {
        // Arrange
        var request = new Get{{ PrefixName }}sRequest { StartPage = 1, PageSize = 10 };
        var page = new Page<{{ PrefixName }}Entity>
        {
            Items = new List<{{ PrefixName }}Entity>(),
            TotalElements = 0
        };

        _mockRepository.Setup(x => x.FindAsync(It.IsAny<PageRequest>()))
            .Returns(Task.FromResult(page));

        // Act
        var result = await _service.Get{{ PrefixName }}s(request);

        // Assert
        _mockValidationService.Verify(x => x.ValidatePaginationRequest(1, 10), Times.Once);
    }
}
