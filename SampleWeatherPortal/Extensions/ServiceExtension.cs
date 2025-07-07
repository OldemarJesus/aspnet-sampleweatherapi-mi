

using Microsoft.Identity.Client;

namespace SampleWeatherPortal.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddHttpClientForSampleWeatherApi(this IServiceCollection services, ConfigurationManager configuration)
    {

        // load configuration settings
        var appIdSettings = configuration.GetSection("AppIDSettings");
        var daemonAppClientId = appIdSettings["DaemonAppClientId"];
        var daemonAppTenantId = appIdSettings["DaemonAppTenantId"];
        var sampleWeatherApiBaseUrl = appIdSettings["SampleWeatherApiBaseUrl"]!;
        var authority = $"https://login.microsoftonline.com/{daemonAppTenantId}";
        var daemonAppScopes = appIdSettings.GetSection("DaemonAppScopes").Get<List<string>>();

        var accessToken = string.Empty;

        if (sampleWeatherApiBaseUrl.Contains("localhost"))
        {
            // get the access token for the SampleWeatherApi
            var daemonAppClientSecret = appIdSettings["DaemonAppClientSecret"];
            var app = ConfidentialClientApplicationBuilder
            .Create(daemonAppClientId)
            .WithAuthority(authority)
            .WithClientSecret(daemonAppClientSecret)
            .Build();

            accessToken = app.AcquireTokenForClient(daemonAppScopes).ExecuteAsync().GetAwaiter().GetResult().AccessToken;
            Console.WriteLine($"Access Token From Secret: {accessToken}");
        }
        else
        {
            // get the access token for the SampleWeatherApi using Managed Identity
            var miClientId = appIdSettings["DaemonManagedIdentityClientId"];
            var miAudience = appIdSettings["DaemonManagedIdentityAudience"];
            var miAssertionProvider = async (AssertionRequestOptions _) =>
            {
                var miApplication = ManagedIdentityApplicationBuilder
              .Create(Microsoft.Identity.Client.AppConfig.ManagedIdentityId.WithUserAssignedClientId(miClientId))
              .Build();

                var miResult = await miApplication.AcquireTokenForManagedIdentity(miAudience)
              .ExecuteAsync()
              .ConfigureAwait(false);
                return miResult.AccessToken;
            };

            // Create a confidential client application with the assertion.
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(daemonAppClientId)
              .WithAuthority(authority, false)
              .WithClientAssertion(miAssertionProvider)
              .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
              .Build();

            // Get the federated app token for the storage account
            AuthenticationResult result = app.AcquireTokenForClient(daemonAppScopes).ExecuteAsync().GetAwaiter().GetResult();
            accessToken = result.AccessToken;
            // log the access token for debugging purposes
            Console.WriteLine($"Access Token From MI: {accessToken}");
        }

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
