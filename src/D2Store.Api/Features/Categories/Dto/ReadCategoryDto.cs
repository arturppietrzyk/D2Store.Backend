namespace D2Store.Api.Features.Categories.Dto;

public record ReadCategoryDto(Guid CategoryId, string Name, DateTime AddedDate, DateTime LastModified);