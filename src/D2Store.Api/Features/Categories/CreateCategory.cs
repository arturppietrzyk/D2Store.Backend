using D2Store.Api.Features.Categories.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;

namespace D2Store.Api.Features.Categories;

public record CreateCategoryCommand(string Name, bool isAdmin) : IRequest<Result<ReadCategoryDto>>;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Result<ReadCategoryDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<CreateCategoryCommand> _validator;

    public CreateCategoryHandler(AppDbContext dbContext, IValidator<CreateCategoryCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }
    
    /// <summary>
    /// Coordinates validation, mapping and creating of a category. Returns the created category in a form of a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadCategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if(!request.isAdmin)
        {
            return Result.Failure<ReadCategoryDto>(Error.Forbidden);
        }
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ReadCategoryDto>(validationResult.Error);
        }
        var createCategory = await CreateCategoryAsync(request, cancellationToken);
        var categoryDto = MapToReadCategoryDto(createCategory);
        return Result.Success(categoryDto);
    }

    /// <summary>
    /// Validates the input.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("CreateCategory.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Creates the category and persists it to the database. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Category> CreateCategoryAsync(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(request.Name);
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return category;
    }

    /// <summary>
    /// Maps the retrieved category into the ReadCategoryDto which is returned as the response. 
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    private static ReadCategoryDto MapToReadCategoryDto(Category category)
    {
        return new ReadCategoryDto(
            category.CategoryId,
            category.Name,
            category.AddedDate,
            category.LastModified
        );
    }
}

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().WithMessage("Name is required.");
    }
}