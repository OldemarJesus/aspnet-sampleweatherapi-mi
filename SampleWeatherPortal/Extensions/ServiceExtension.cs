using Microsoft.Identity.Client;

namespace SampleWeatherPortal.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddHttpClientForSampleWeatherApi(this IServiceCollection services, ConfigurationManager configuration)
    {
        // load configuration settings
        var appIdSettings = configuration.GetSection("AppIDSettings");
        var daemonAppClientId = appIdSettings["DaemonAppClientId"];
        var daemonAppClientSecret = appIdSettings["DaemonAppClientSecret"];
        var daemonAppTenantId = appIdSettings["DaemonAppTenantId"];
        var sampleWeatherApiBaseUrl = appIdSettings["SampleWeatherApiBaseUrl"]!;
        var authority = $"https://login.microsoftonline.com/{daemonAppTenantId}";
        var daemonAppScopes = appIdSettings.GetSection("DaemonAppScopes").Get<List<string>>();

        // get the access token for the SampleWeatherApi
        var app = ConfidentialClientApplicationBuilder
            .Create(daemonAppClientId)
            .WithAuthority(authority)
            .WithClientSecret(daemonAppClientSecret)
            .Build();
        
        var accessToken = app.AcquireTokenForClient(daemonAppScopes).ExecuteAsync().GetAwaiter().GetResult().AccessToken;

        // add HttpClient for SampleWeatherApi
        services.AddHttpClient("SampleWeatherApi", client =>
        {
            client.BaseAddress = new Uri(sampleWeatherApiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        });
        return services;
    }
}
