using Microsoft.Extensions.Logging;
using {{ PrefixName }}{{ SuffixName }}.API;
using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Models;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;

namespace {{ PrefixName }}{{ SuffixName }}.Core;

public class {{ PrefixName }}{{ SuffixName }}Core : I{{ PrefixName }}{{ SuffixName }}
{
    private readonly I{{ PrefixName }}Repository _repository;
    private readonly IValidationService _validationService;
    private readonly ILogger<{{ PrefixName }}{{ SuffixName }}Core> _logger;

    public {{ PrefixName }}{{ SuffixName }}Core(
        I{{ PrefixName }}Repository repository,
        IValidationService validationService,
        ILogger<{{ PrefixName }}{{ SuffixName }}Core> logger)
    {
        _repository = repository;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<Create{{ PrefixName }}Response> Create{{ PrefixName }}(Create{{ PrefixName }}Input input)
    {
        _validationService.ValidateCreateRequest(input);

        var entity = new {{ PrefixName }}Entity
        {
            Name = input.Name
        };

        _repository.Save(entity);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Created {{ PrefixName }} with ID {Id}", entity.Id);

        return new Create{{ PrefixName }}Response
        {
            {{ PrefixName }} = MapToDto(entity)
        };
    }

    public async Task<Get{{ PrefixName }}sResponse> Get{{ PrefixName }}s(Get{{ PrefixName }}sRequest request)
    {
        return await Get{{ PrefixName }}sAsync(request);
    }

    public async Task<Get{{ PrefixName }}sResponse> Get{{ PrefixName }}sAsync(Get{{ PrefixName }}sRequest request)
    {
        // Validate pagination request first
        _validationService.ValidatePaginationRequest(request.StartPage, request.PageSize);

        // Normalize pagination parameters
        var startPage = Math.Max(1, request.StartPage);
        var pageSize = Math.Min(100, Math.Max(1, request.PageSize));

        var pageRequest = new PageRequest
        {
            StartPage = startPage,
            PageSize = pageSize
        };

        var page = await _repository.FindAsync(pageRequest);

        return new Get{{ PrefixName }}sResponse
        {
            {{ PrefixName }}s = page.Items.Select(MapToDto).ToList(),
            TotalElements = page.TotalElements
        };
    }

    public async Task<Get{{ PrefixName }}Response> Get{{ PrefixName }}(string id)
    {
        return await Get{{ PrefixName }}Async(id);
    }

    public async Task<Get{{ PrefixName }}Response> Get{{ PrefixName }}Async(string id)
    {
        var guidId = _validationService.ValidateAndParseId(id, "id");
        
        var entity = await _repository.FindByIdAsync(guidId);
        if (entity == null)
        {
            throw new EntityNotFoundException("{{ PrefixName }}", guidId.ToString());
        }

        return new Get{{ PrefixName }}Response
        {
            {{ PrefixName }} = MapToDto(entity)
        };
    }

    public async Task<Update{{ PrefixName }}Response> Update{{ PrefixName }}(Update{{ PrefixName }}Input input)
    {
        return await Update{{ PrefixName }}Async(input);
    }

    public async Task<Update{{ PrefixName }}Response> Update{{ PrefixName }}Async(Update{{ PrefixName }}Input input)
    {
        _validationService.ValidateUpdateRequest(input);

        var guidId = _validationService.ValidateAndParseId(input.Id, "id");
        
        var entity = await _repository.FindByIdAsync(guidId);
        if (entity == null)
        {
            throw new EntityNotFoundException("{{ PrefixName }}", guidId.ToString());
        }

        entity.Name = input.Name;

        _repository.Update(entity);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Updated {{ PrefixName }} with ID {Id}", entity.Id);

        return new Update{{ PrefixName }}Response
        {
            {{ PrefixName }} = MapToDto(entity)
        };
    }

    public async Task<Delete{{ PrefixName }}Response> Delete{{ PrefixName }}(string id)
    {
        return await Delete{{ PrefixName }}Async(id);
    }

    public async Task<Delete{{ PrefixName }}Response> Delete{{ PrefixName }}Async(string id)
    {
        var guidId = _validationService.ValidateAndParseId(id, "id");
        
        var entity = await _repository.FindByIdAsync(guidId);
        if (entity == null)
        {
            throw new EntityNotFoundException("{{ PrefixName }}", guidId.ToString());
        }

        _repository.Delete(entity);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Deleted {{ prefixName }} with ID {Id}", entity.Id);

        return new Delete{{ PrefixName }}Response { Success = true };
    }

    private static {{ PrefixName }}Dto MapToDto({{ PrefixName }}Entity entity)
    {
        return new {{ PrefixName }}Dto
        {
            Id = entity.Id.ToString(),
            Name = entity.Name ?? string.Empty
        };
    }
}
