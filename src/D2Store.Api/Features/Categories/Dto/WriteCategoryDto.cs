namespace D2Store.Api.Features.Categories.Dto;

public record WriteCategoryDtoCreate
{
    public required string Name { get; init; }
}

public record WriteCategoryDtoUpdate
{
    public string? Name { get; init; }
}
