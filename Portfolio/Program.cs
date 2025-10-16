using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Portfolio.Calculation;
using Portfolio.Scrape;
using Portfolio.Scrape.Api;
using Refit;
using Microsoft.EntityFrameworkCore;
using Portfolio.EntityModel;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

ServiceCollection services = new();

ConfigureServices(services, configuration);

// Run the app
var provider = services.BuildServiceProvider();
var app = provider.GetRequiredService<Application>();
await app.RunAsync();

static void ConfigureServices(ServiceCollection services, IConfigurationRoot configuration)
{
    var campanelloBaseUrl = configuration.GetValue<string>("BaseUrls:Campanello");
    var doofusLookupBaseUrl = configuration.GetValue<string>("BaseUrls:DoofusLookup");
    var doofusQueryBaseUrl = configuration.GetValue<string>("BaseUrls:DoofusQuery");

    services.AddSingleton<Application>();
    services.AddSingleton<IConfiguration>(configuration);
    services.AddScoped<IScraper, Scraper>();
    services.AddScoped<ICalc, Calc>();
    services.AddTransient<UrlDecodeHandler>();
    services.AddTransient<PassthroughHandler>();

    services.AddDbContext<PortfolioContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("PortfolioConnection")));

    var retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutRejectedException>()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromMilliseconds(750));

    services
        .AddRefitClient<ICampanelloApi>()
        .ConfigureHttpClient(client =>
        {
            client.BaseAddress = new Uri(campanelloBaseUrl);
        })
        .AddPolicyHandler(retryPolicy)
        .AddHttpMessageHandler<UrlDecodeHandler>();

    services
        .AddRefitClient<IDoofusLookupApi>()
        .ConfigureHttpClient(client =>
        {
            client.BaseAddress = new Uri(doofusLookupBaseUrl);
        })
        .AddPolicyHandler(retryPolicy);

    services
        .AddRefitClient<IDoofusQueryApi>()
        .ConfigureHttpClient(client =>
        {
            client.BaseAddress = new Uri(doofusQueryBaseUrl);
        })
        .AddPolicyHandler(retryPolicy)
        .AddHttpMessageHandler<PassthroughHandler>();

    services.AddHttpClient(nameof(IScraper)).AddPolicyHandler(retryPolicy);
}
