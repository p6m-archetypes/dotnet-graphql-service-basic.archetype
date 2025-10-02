using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.API.Schema;
using {{ PrefixName }}{{ SuffixName }}.Client;
using Xunit.Abstractions;

namespace {{ PrefixName }}{{ SuffixName }}.IntegrationTests;

[Collection("ApplicationCollection")]
public class {{ PrefixName }}{{ SuffixName }}GraphQLIT(ITestOutputHelper testOutputHelper, ApplicationFixture applicationFixture)
{
    private readonly ApplicationFixture _applicationFixture = applicationFixture;
    private readonly {{ PrefixName }}{{ SuffixName }}Client _client = applicationFixture.GetClient();

    [Fact]
    public async Task Test_Create{{ PrefixName }}()
    {
        // Arrange
        var input = new Create{{ PrefixName }}Input { Name = Guid.NewGuid().ToString() };

        // Act
        var response = await _client.Create{{ PrefixName }}(input);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.{{ PrefixName }});
        Assert.NotNull(response.{{ PrefixName }}.Id);
        Assert.Equal(input.Name, response.{{ PrefixName }}.Name);
    }

    [Fact]
    public async Task Test_Get{{ PrefixName }}s()
    {
        testOutputHelper.WriteLine("Test_Get{{ PrefixName }}s");

        // Arrange
        var beforeResponse = await _client.Get{{ PrefixName }}s(new Get{{ PrefixName }}sRequest { StartPage = 1, PageSize = 10 });
        var beforeTotal = beforeResponse.TotalElements;

        // Create a new item
        var createInput = new Create{{ PrefixName }}Input { Name = Guid.NewGuid().ToString() };
        await _client.Create{{ PrefixName }}(createInput);

        // Act
        var response = await _client.Get{{ PrefixName }}s(new Get{{ PrefixName }}sRequest { StartPage = 1, PageSize = 10 });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(beforeTotal + 1, response.TotalElements);
        Assert.NotNull(response.{{ PrefixName }}s);
        Assert.NotEmpty(response.{{ PrefixName }}s);
    }

    [Fact]
    public async Task Test_Get{{ PrefixName }}()
    {
        // Arrange
        var createInput = new Create{{ PrefixName }}Input { Name = Guid.NewGuid().ToString() };
        var createResponse = await _client.Create{{ PrefixName }}(createInput);
        var createdId = createResponse.{{ PrefixName }}.Id;

        // Act
        var result = await _client.Get{{ PrefixName }}(createdId!);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.{{ PrefixName }});
        Assert.Equal(createdId, result.{{ PrefixName }}.Id);
        Assert.Equal(createInput.Name, result.{{ PrefixName }}.Name);
    }

    [Fact]
    public async Task Test_Update{{ PrefixName }}()
    {
        // Arrange
        var createInput = new Create{{ PrefixName }}Input { Name = Guid.NewGuid().ToString() };
        var createResponse = await _client.Create{{ PrefixName }}(createInput);
        var createdId = createResponse.{{ PrefixName }}.Id;

        var updateInput = new Update{{ PrefixName }}Input
        {
            Id = createdId!,
            Name = "Updated Name"
        };

        // Act
        var response = await _client.Update{{ PrefixName }}(updateInput);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.{{ PrefixName }});
        Assert.Equal(createdId, response.{{ PrefixName }}.Id);
        Assert.Equal("Updated Name", response.{{ PrefixName }}.Name);
    }

    [Fact]
    public async Task Test_Delete{{ PrefixName }}()
    {
        // Arrange
        var createInput = new Create{{ PrefixName }}Input { Name = Guid.NewGuid().ToString() };
        var createResponse = await _client.Create{{ PrefixName }}(createInput);
        var createdId = createResponse.{{ PrefixName }}.Id;

        // Act
        var response = await _client.Delete{{ PrefixName }}(createdId!);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);

        // Verify deletion - should throw exception when not found
        await Assert.ThrowsAsync<{{ PrefixName }}{{ SuffixName }}Client.GraphQLException>(() => _client.Get{{ PrefixName }}(createdId!));
    }

    [Fact]
    public async Task Test_Delete{{ PrefixName }}_NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act & Assert
        // GraphQL returns an error for entity not found
        var exception = await Assert.ThrowsAsync<{{ PrefixName }}{{ SuffixName }}Client.GraphQLException>(async () =>
        {
            await _client.Delete{{ PrefixName }}(nonExistentId);
        });

        // The exception message should indicate the entity was not found
        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Test_Pagination()
    {
        // Arrange - Create multiple items
        var itemsToCreate = 5;
        for (int i = 0; i < itemsToCreate; i++)
        {
            var input = new Create{{ PrefixName }}Input { Name = $"Test Item {i} - {Guid.NewGuid()}" };
            await _client.Create{{ PrefixName }}(input);
        }

        // Act - Get first page
        var firstPage = await _client.Get{{ PrefixName }}s(new Get{{ PrefixName }}sRequest { StartPage = 1, PageSize = 2 });

        // Assert
        Assert.NotNull(firstPage);
        Assert.Equal(2, firstPage.{{ PrefixName }}s.Count);
        Assert.True(firstPage.TotalElements >= itemsToCreate);

        // Act - Get next page
        var secondPage = await _client.Get{{ PrefixName }}s(new Get{{ PrefixName }}sRequest { StartPage = 2, PageSize = 2 });

        // Assert
        Assert.NotNull(secondPage);
        Assert.True(secondPage.{{ PrefixName }}s.Any());
    }
}
