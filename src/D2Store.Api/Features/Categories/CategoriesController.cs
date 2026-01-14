using D2Store.Api.Features.Categories.Dto;
using D2Store.Api.Shared;
using D2Store.Api.Shared.Enums;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D2Store.Api.Features.Categories;

[ApiController]
[Route("api/")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] WriteCategoryDtoCreate dtoCreate, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new CreateCategoryCommand(dtoCreate.Name, isAdmin), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            return BadRequest(result.Error);
        }
        return CreatedAtAction(nameof(GetCategory), new { categoryId = result.Value.CategoryId }, result.Value);
    }

    [HttpGet("categories/{categoryId}")]
    public async Task<IActionResult> GetCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoryQuery(categoryId), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest();
        }
        return Ok(result.Value);
    }

    [HttpPatch("categories/{categoryId}")]
    public async Task<IActionResult> UpdateCategory(Guid categoryId, [FromBody] WriteCategoryDtoUpdate dtoUpdate, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new UpdateCategoryCommand(categoryId, dtoUpdate.Name, isAdmin), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            if (result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    [HttpDelete("categories/{categoryId}")]
    public async Task<IActionResult> DeleteCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new DeleteCategoryCommand(categoryId, isAdmin), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            if (result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return NoContent();
    }
}