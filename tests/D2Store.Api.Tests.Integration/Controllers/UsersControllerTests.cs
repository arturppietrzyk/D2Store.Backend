using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using D2Store.Api.Features.Users.Dto;

namespace D2Store.Api.Tests.Integration.Controllers;

public class UsersControllerTests : IClassFixture<D2StoreApiFactory>
{
    private readonly D2StoreApiFactory _apiFactory;

    public UsersControllerTests(D2StoreApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    // RegisterUser Tests
    [Fact]
    public async Task RegisterUser_ReturnsCreated_WhenRegisterUserSucceeds()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var dtoRegister = new WriteUserDtoRegister
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@gmail.com",
            Password = "Password",
            PhoneNumber = "123456",
            Address = "Address"
        };
        // Act
        var response = await client.PostAsJsonAsync("http://localhost:5000/api/users", dtoRegister);
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    //LoginUser Tests
    [Fact]
    public async Task LoginUser_ReturnsOk_WhenCredentialsAreValid()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var dtoLogin = new WriteUserDtoLogin
        {
            Email = "john@gmail.com",
            Password = "Password"
        };
        // Act
        var response = await client.PostAsJsonAsync("http://localhost:5000/api/users/login", dtoLogin);
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    //GetUser Tests
    [Fact]
    public async Task GetUser_ReturnsOk_WhenAuthenticatedUserRequestsOwnData()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var dtoRegister = new WriteUserDtoRegister
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            Password = "Password",
            PhoneNumber = "1234567890",
            Address = "123 Test Street"
        };
        await client.PostAsJsonAsync("http://localhost:5000/api/users", dtoRegister);
        var dtoLogin = new WriteUserDtoLogin
        {
            Email = dtoRegister.Email,
            Password = dtoRegister.Password
        };
        var loginResponse = await client.PostAsJsonAsync("http://localhost:5000/api/users/login", dtoLogin);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<ReadAuthDto>();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(authResponse!.AccessToken);
        var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
        // Act
        var response = await client.GetAsync($"http://localhost:5000/api/users/{userId}");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var userDto = await response.Content.ReadFromJsonAsync<ReadUserDto>();
        Assert.NotNull(userDto);
        Assert.Equal(dtoLogin.Email, userDto.Email);
    }

    // GetUsers Tests
    [Fact]
    public async Task GetUsers_ReturnsForbidden_WhenNonAdmin()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var dtoRegister = new WriteUserDtoRegister
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            Password = "Password",
            PhoneNumber = "1234567890",
            Address = "123 Test Street"
        };
        await client.PostAsJsonAsync("http://localhost:5000/api/users", dtoRegister);
        var loginDto = new WriteUserDtoLogin
        {
            Email = dtoRegister.Email,
            Password = dtoRegister.Password
        };
        var loginResponse = await client.PostAsJsonAsync("http://localhost:5000/api/users/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<ReadAuthDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);
        // Act
        var response = await client.GetAsync("http://localhost:5000/api/users");
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    //UpdateUser Tests
    [Fact]
    public async Task UpdateUser_ReturnsOkAndUpdatedId_WhenUserIsOwner()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var writeUserDto = new WriteUserDtoRegister
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.update@test.com",
            Password = "Password",
            PhoneNumber = "1234567890",
            Address = "Old Address"
        };
        await client.PostAsJsonAsync("http://localhost:5000/api/users", writeUserDto);
        var writeUserDtoLogin = new WriteUserDtoLogin
        {
            Email = writeUserDto.Email,
            Password = writeUserDto.Password
        };
        var loginResponse = await client.PostAsJsonAsync("http://localhost:5000/api/users/login", writeUserDtoLogin);
        loginResponse.EnsureSuccessStatusCode();
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<ReadAuthDto>();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(authResponse!.AccessToken);
        var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
        var updateDto = new WriteUserDtoUpdate
        {
            Address = "New Updated Address"
        };
        // Act
        var response = await client.PatchAsJsonAsync($"http://localhost:5000/api/users/{userId}", updateDto);
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    //DeleteUser Tests
    [Fact]
    public async Task DeleteUser_ReturnsNoContent_WhenDeleteUserSucceeds()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var dtoRegister = new WriteUserDtoRegister
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            Password = "Password",
            PhoneNumber = "1234567890",
            Address = "Test Street"
        };
        await client.PostAsJsonAsync("http://localhost:5000/api/users", dtoRegister);
        var dtoLogin = new WriteUserDtoLogin
        {
            Email = dtoRegister.Email,
            Password = dtoRegister.Password
        };
        var loginResponse = await client.PostAsJsonAsync("http://localhost:5000/api/users/login", dtoLogin);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<ReadAuthDto>();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(authResponse!.AccessToken);
        var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
        // Act
        var response = await client.DeleteAsync($"http://localhost:5000/api/users/{userId}");
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}