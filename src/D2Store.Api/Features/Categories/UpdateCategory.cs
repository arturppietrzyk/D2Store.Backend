using D2Store.Api.Features.Categories.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Categories;

public record UpdateCategoryCommand(Guid CategoryId, string? Name, bool IsAdmin) : IRequest<Result>;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpdateCategoryCommand> _validator;

    public UpdateCategoryHandler(AppDbContext dbContext, IValidator<UpdateCategoryCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval, mapping and updating of a specific category. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin)
        {
            return Result.Failure(Error.Forbidden);
        }
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }
        var categoryResult = await GetCategoryAsync(request.CategoryId, cancellationToken);
        if (categoryResult.IsFailure)
        {
            return Result.Failure<ReadCategoryDto>(categoryResult.Error);
        }
        var updateCategoryResult = await UpdateCategoryAsync(categoryResult.Value, request, cancellationToken);
        if (updateCategoryResult.IsFailure)
        {
            return Result.Failure(updateCategoryResult.Error);
        }
        return Result.Success();
    }

    /// <summary>
    /// Validates the input.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("UpdateCategory.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Loads a category object based on the CategoryId.
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Category>> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<Category>(Error.NotFound);
        }
        return Result.Success(category);
    }

    /// <summary>
    /// Updates the category and persists the changes in the database table. 
    /// </summary>
    /// <param name="category"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> UpdateCategoryAsync(Category category, UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var isUpdatedResult = category.Update(request.Name);
        if (isUpdatedResult.IsFailure)
        {
            return Result.Failure(isUpdatedResult.Error);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }


    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(p => p.Name).NotEmpty().When(p => p.Name is not null).WithMessage("Name cannot be empty if provided.");
            RuleFor(p => p)
                .Must(p =>
                    p.Name is not null)
                    .WithMessage("Field (Name) must be provided.");
        }
    }
}