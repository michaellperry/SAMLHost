namespace SAMLHost.Services;

public class SamlServiceProviderOptions
{
    public string EntityID { get; set; }
    public string AssertionConsumerServiceUrl { get; set; }
    public string AuthenticatedRedirectUrl { get; set; }
}