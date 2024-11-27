namespace consul_demo.MyApplicationConfiguration;

public class ApplicationConfiguration
{
    public ApiCredentialConfiguration ApiCredentialConfiguration { get; set; }
    public FeatureManagement FeatureManagement { get; set; }
    public LoggerFilterOptions Logging { get; set; }
}