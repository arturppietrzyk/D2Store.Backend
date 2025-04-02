using MediatR;
using Microsoft.AspNetCore.Mvc;
using D2Store.Api.Features.Customers.Dto;

namespace D2Store.Api.Features.Customers;

[ApiController]
[Route("api/")]
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("customer")]
    public async Task<IActionResult> CreateCustomer([FromBody] WriteCustomerDtoCreate writeCustomerDto)
    {
        var result = await _mediator.Send(new CreateCustomerCommand(writeCustomerDto.FirstName, writeCustomerDto.LastName, writeCustomerDto.Email, writeCustomerDto.PhoneNumber, writeCustomerDto.Address));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetOrderById(Guid customerId)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(customerId));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _mediator.Send(new GetCustomersQuery(pageNumber, pageSize));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpPatch("customer/{customerId}")]
    public async Task<IActionResult> UpdateCustomer(Guid customerId, [FromBody] WriteCustomerDtoUpdate writeCustomerDto)
    {
        var result = await _mediator.Send(new UpdateCustomerCommand(customerId, writeCustomerDto.FirstName, writeCustomerDto.LastName, writeCustomerDto.Email, writeCustomerDto.PhoneNumber, writeCustomerDto.Address));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpDelete("customer/{customerId}")]
    public async Task<IActionResult> DeleteOrder(Guid customerId)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand(customerId));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }
}
