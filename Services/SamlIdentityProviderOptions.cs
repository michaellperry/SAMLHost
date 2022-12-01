namespace SAMLHost.Services;

public class SamlIdentityProviderOptions
{
    public string EntityId { get; set; }
    public string SingleSignOnServiceUrl { get; set; }
    public string? LogoutUrl { get; set; }
    public string CertificateFileName { get; set; }
}