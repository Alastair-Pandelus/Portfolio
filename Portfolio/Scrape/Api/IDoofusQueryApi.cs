using Refit;

namespace Portfolio.Scrape.Api
{
    public interface IDoofusQueryApi
    {
        [Get("/chart/{ySymbol}?includeAdjustedClose=true&interval=1d&period1=0&period2={period2}&userYfid=true&lang=en-US&region=US")]
        [Headers(
            "Accept: */*",
            "Host: query1.finance.yahoo.com",
            "user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3",
            "Connection: keep-alive"
        )]
        Task<string> ChartAsync(string ySymbol, long period2);
    }
}
