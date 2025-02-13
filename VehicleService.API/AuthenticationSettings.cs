namespace VehicleService.API;

public class AuthenticationSettings
{
    public TokenOptions TokenOptions { get; set; }
    public List<PolicyOptions> Policies { get; set; }
}

public class TokenOptions
{
    public string Scheme { get; set; } = null!;
    public string Authority { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public bool RequireHttpsMetadata { get; set; } = false;
}

public class PolicyOptions
{
    public string Name { get; set; } = null!;
    public List<ClaimsSettings> Claims { get; set; } = [];
}

public class ClaimsSettings
{
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
}