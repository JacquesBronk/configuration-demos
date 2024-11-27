namespace consul_demo.MyApplicationConfiguration;

public class ApiCredential
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public string MethodType { get; set; }
    public bool IsAnonymous { get; set; }
}