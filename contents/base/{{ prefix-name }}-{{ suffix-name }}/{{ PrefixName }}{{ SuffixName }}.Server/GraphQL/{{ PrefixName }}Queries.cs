using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.API.Schema;
using {{ PrefixName }}{{ SuffixName }}.Core;

namespace {{ PrefixName }}{{ SuffixName }}.Server.GraphQL;

[ExtendObjectType("Query")]
public class {{ PrefixName }}Queries
{
    private readonly ILogger<{{ PrefixName }}Queries> _logger;

    public {{ PrefixName }}Queries(ILogger<{{ PrefixName }}Queries> logger)
    {
        _logger = logger;
    }

    [Authorize(Roles = new[] { "read", "write", "admin" })]
    public async Task<{{ PrefixName }}Dto?> Get{{ PrefixName }}(
        string id,
        [Service] {{ PrefixName }}{{ SuffixName }}Core core,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Get{{ PrefixName }} called with id: {Id}", id);

        try
        {
            var result = await core.Get{{ PrefixName }}(id);

            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("Get{{ PrefixName }} completed in {ExecutionTime}ms", executionTime);

            return result.{{ PrefixName }};
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Get{{ PrefixName }}");
            throw;
        }
    }

    [Authorize(Roles = new[] { "read", "write", "admin" })]
    public async Task<{{ PrefixName }}Connection> Get{{ PrefixName }}s(
        string? startPage,
        int? pageSize,
        [Service] {{ PrefixName }}{{ SuffixName }}Core core,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Get{{ PrefixName }}s called with startPage: {StartPage}, pageSize: {PageSize}",
            startPage, pageSize);

        try
        {
            var request = new Get{{ PrefixName }}sRequest
            {
                StartPage = int.TryParse(startPage, out var sp) ? sp : 1,
                PageSize = pageSize ?? 10
            };
            var result = await core.Get{{ PrefixName }}s(request);

            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("Get{{ PrefixName }}s completed in {ExecutionTime}ms with {Count} items",
                executionTime, result.{{ PrefixName }}s.Count);

            // Convert to {{ PrefixName }}Connection
            var totalPages = (int)Math.Ceiling((double)result.TotalElements / request.PageSize);
            var currentPage = request.StartPage;

            return new {{ PrefixName }}Connection
            {
                Items = result.{{ PrefixName }}s,
                TotalCount = (int)result.TotalElements,
                PageInfo = new PageInfo
                {
                    HasNextPage = currentPage < totalPages,
                    HasPreviousPage = currentPage > 1,
                    StartPage = currentPage.ToString(),
                    NextPage = currentPage < totalPages ? (currentPage + 1).ToString() : null,
                    PreviousPage = currentPage > 1 ? (currentPage - 1).ToString() : null
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Get{{ PrefixName }}s");
            throw;
        }
    }
}
