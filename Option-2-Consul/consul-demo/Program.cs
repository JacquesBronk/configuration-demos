using consul_demo.MyApplicationConfiguration;
using Microsoft.Extensions.Options;
using Winton.Extensions.Configuration.Consul;
using Microsoft.FeatureManagement;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddConsul("appsettings/myapp",
        options =>
        {
            options.ConsulConfigurationOptions = cco =>
            {
                cco.Address = new Uri("http://consul:8500");
            };
            options.ReloadOnChange = true; 
            options.Optional = true;
            options.OnLoadException = exceptionContext => throw exceptionContext.Exception;
        });

builder.Services.AddFeatureManagement();
builder.Services.Configure<ApplicationConfiguration>(builder.Configuration);
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/config", (IOptionsMonitor<ApplicationConfiguration> configOptions) =>
{
    var config = configOptions.CurrentValue;
    return Results.Ok(config);
}).WithName("GetAllConfig")
.WithTags("Feature Flags");

app.MapGet("/feature", async (IFeatureManager featureManager) =>
{
    if (await featureManager.IsEnabledAsync("UseFeatureA"))
    {
        return Results.Ok("Feature A is enabled.");
    }
    else
    {
        return Results.StatusCode(423);
    }
    
}).WithName("UseFeature")
.WithTags("Feature Usage");;

app.Run();