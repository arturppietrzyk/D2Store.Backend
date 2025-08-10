using System.Net;
using System.Net.Http.Json;
using D2Store.Api.Features.Users.Dto;

namespace D2Store.Api.Tests.Integration.Controllers;

public class UserControllerTests : IClassFixture<D2StoreApiFactory>
{
    private readonly D2StoreApiFactory _apiFactory;

    public UserControllerTests(D2StoreApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RegisterUser_WithValidData_ReturnsOkAndCreatesUser()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var writeUserDto = new WriteUserDtoRegister
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password",
            PhoneNumber = "123456",
            Address = "Address"
        };
        // Act
        var response = await client.PostAsJsonAsync("http://localhost:5000/api/register-user", writeUserDto);
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}