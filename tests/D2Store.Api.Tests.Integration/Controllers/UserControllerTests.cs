using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using D2Store.Api.Features.Users.Dto;

namespace D2Store.Api.Tests.Integration.Controllers;

public class UserControllerTests : IClassFixture<D2StoreApiFactory>
{
    private readonly D2StoreApiFactory _apiFactory;

    public UserControllerTests(D2StoreApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    // RegisterUser Tests
    [Fact]
    public async Task RegisterUser_ReturnsOk_WhenRegisterUserSucceeds()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var writeUserDto = new WriteUserDtoRegister
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@gmail.com",
            Password = "Password",
            PhoneNumber = "123456",
            Address = "Address"
        };
        // Act
        var response = await client.PostAsJsonAsync("http://localhost:5000/api/register-user", writeUserDto);
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    //LoginUser Tests
    [Fact]
    public async Task LoginUser_ReturnsOk_WhenCredentialsAreValid()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var writeUserDto = new WriteUserDtoLogin
        {
            Email = "john@gmail.com",
            Password = "Password"
        };
        // Act
        var response = await client.PostAsJsonAsync("http://localhost:5000/api/login-user", writeUserDto);
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    //GetUser Tests
    [Fact]
    public async Task GetUser_ReturnsOk_WhenAuthenticatedUserRequestsOwnData()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var writeUserDto = new WriteUserDtoRegister
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            Password = "Password",
            PhoneNumber = "1234567890",
            Address = "123 Test Street"
        };
        await client.PostAsJsonAsync("http://localhost:5000/api/register-user", writeUserDto);
        var writeUserDtoLogin = new WriteUserDtoLogin
        {
            Email = writeUserDto.Email,
            Password = writeUserDto.Password
        };
        var loginResponse = await client.PostAsJsonAsync("http://localhost:5000/api/login-user", writeUserDtoLogin);
        var token = await loginResponse.Content.ReadAsStringAsync();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        // Act
        var response = await client.GetAsync($"http://localhost:5000/api/user/{userId}");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var userDto = await response.Content.ReadFromJsonAsync<ReadUserDto>();
        Assert.NotNull(userDto);
        Assert.Equal(writeUserDto.Email, userDto.Email);
    }

    // GetUsers Tests
    [Fact]
    public async Task GetUsers_ReturnsForbidden_WhenNonAdmin()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var writeUserDto = new WriteUserDtoRegister
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            Password = "Password",
            PhoneNumber = "1234567890",
            Address = "123 Test Street"
        };
        await client.PostAsJsonAsync("http://localhost:5000/api/register-user", writeUserDto);
        var loginDto = new WriteUserDtoLogin
        {
            Email = writeUserDto.Email,
            Password = writeUserDto.Password
        };
        var loginResponse = await client.PostAsJsonAsync("http://localhost:5000/api/login-user", loginDto);
        var token = await loginResponse.Content.ReadAsStringAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        // Act
        var response = await client.GetAsync("http://localhost:5000/api/users");
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    //DeleteUser Tests
    [Fact]
    public async Task DeleteUser_ReturnsOk_WhenDeleteUserSucceeds()
    {
        // Arrange
        var client = _apiFactory.CreateClient();
        var writeUserDto = new WriteUserDtoRegister
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            Password = "Password",
            PhoneNumber = "1234567890",
            Address = "Test Street"
        };
        await client.PostAsJsonAsync("http://localhost:5000/api/register-user", writeUserDto);
        var writeUserDtoLogin = new WriteUserDtoLogin
        {
            Email = writeUserDto.Email,
            Password = writeUserDto.Password
        };
        var loginResponse = await client.PostAsJsonAsync("http://localhost:5000/api/login-user", writeUserDtoLogin);
        var token = await loginResponse.Content.ReadAsStringAsync();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        // Act
        var response = await client.DeleteAsync($"http://localhost:5000/api/user/{userId}");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        await client.PostAsJsonAsync("http://localhost:5000/api/register-user", writeUserDto);
        var writeUserDtoLogin = new WriteUserDtoLogin
        {
            Email = writeUserDto.Email,
            Password = writeUserDto.Password
        };
        var loginResponse = await client.PostAsJsonAsync("http://localhost:5000/api/login-user", writeUserDtoLogin);
        loginResponse.EnsureSuccessStatusCode();
        var tokenString = await loginResponse.Content.ReadAsStringAsync();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);
        var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
        var updateDto = new WriteUserDtoUpdate
        {
            Address = "New Updated Address"
        };
        // Act
        var response = await client.PatchAsJsonAsync($"http://localhost:5000/api/user/{userId}", updateDto);
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}