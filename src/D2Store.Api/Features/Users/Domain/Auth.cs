namespace D2Store.Api.Features.Users.Domain;

public class Auth
{
    public string AccessToken { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }

    private Auth(string accessToken, DateTime expiresAt)
    {
        AccessToken = accessToken;
        ExpiresAt = expiresAt;
    }

    public static Auth Login(string accessToken, DateTime expiresAt)
    {
        var auth = new Auth(accessToken, expiresAt);
        return auth;
    }
}