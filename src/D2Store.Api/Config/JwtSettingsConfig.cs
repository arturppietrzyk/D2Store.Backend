namespace D2Store.Api.Config;

public class JwtSettingsConfig
{
    public const string SectionName = "JwtSettings";
    public required string Secret { get; init; }
    public required int ExpiryMinutes { get; init; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}
