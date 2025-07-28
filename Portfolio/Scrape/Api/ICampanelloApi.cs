using Portfolio.Scrape.Model;
using Refit;

namespace Portfolio.Scrape.Api
{
    public interface ICampanelloApi
    {
        [Headers("Content-Type: application/json",
                 "Cookie: cookiesession1=678A3ECB8F7B48E9BABE995963A26F4E")]
        [Post("/market-research/api/screener")]
        Task<InstrumentInfoQueryResult> GetInstrumentInfosAsync(InstrumentsPagedQuery instrumentQuery);

        [Get("/market-research/sub/{marketCode}/charts-performance")]
        Task<string> GetPerformanceAsync(string marketCode);

        [Get("/market-research/sub/{marketCode}/portfolio")]
        Task<string> GetPortfolioAsync(string marketCode);

        [Get("/market-research/sub/{marketCode}/risks-and-ratings")]
        Task<string> GetRiskAndRatings(string marketCode);

        // Search for securities by ISIN and possibly Sedol
        [Headers("x-auth-ajb: YouInvestDeviceToken token=278bc838-b246-4fa7-b066-5e231ebc086e")]
        [Get("/api/securities/search?text={text}&fields=bestName, researchUrl, marketCode&page=0&limit=30&showPenalized=false")]
        Task<Model.SearchResult.SearchResult> Search(string text);
    }
}
