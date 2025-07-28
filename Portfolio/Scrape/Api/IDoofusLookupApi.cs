using Portfolio.Scrape.Model;
using Refit;

namespace Portfolio.Scrape.Api
{
    public interface IDoofusLookupApi
    {
        [Get("/search?q={ISIN}&newsCount=0")]
        [Headers(
            "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36 Edg/137.0.0.0",
            "Accept: */*",
            "Host: query2.finance.yahoo.com",
            "Accept-Encoding: gzip, deflate, br",
            "Connection: keep-alive"
        )]
        Task<QuoteResult> LookupAsync(string ISIN);
    }
}
