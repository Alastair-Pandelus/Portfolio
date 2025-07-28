using Microsoft.EntityFrameworkCore;
using Portfolio.EntityModel;
using System.Text.Json.Serialization;

namespace Portfolio.Scrape.Model
{
    public class InstrumentInfoQueryResult
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("rows")]
        public List<Instrument> Rows { get; set; }
    }
}
