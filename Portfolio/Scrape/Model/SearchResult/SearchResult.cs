using Newtonsoft.Json;

namespace Portfolio.Scrape.Model.SearchResult
{
    public class SearchResult
    {
        //public bool success { get; set; }
        public Data data { get; set; }
    }

    public class AdditonalData
    {
        public string kiidDocument { get; set; }
        public object sidDocument { get; set; }
        public string managementFee { get; set; }
        public string ongoingCharge { get; set; }
        public string initialCharge { get; set; }
    }

    public class Data
    {
        //public int pageNo { get; set; }
        //public int totalPageCount { get; set; }
        public int itemCount { get; set; }
        //public int totalItemsCount { get; set; }
        //public double scoreThreshold { get; set; }
        public List<Result> results { get; set; }
    }

    public class Quote
    {
        public object lastTradeDate { get; set; }
        public int time { get; set; }
        public string lastPriceTime { get; set; }
        public double price { get; set; }
        public string priceFormatted { get; set; }
        public object bid { get; set; }
        public object bidFormatted { get; set; }
        public object ask { get; set; }
        public object askFormatted { get; set; }
        public object volume { get; set; }
        public object volumeFormatted { get; set; }
        public object priceQuality { get; set; }
        public object marketStatus { get; set; }
        public object openPrice { get; set; }
        public object openPriceFormatted { get; set; }
        public double lastClosePrice { get; set; }
        public string lastClosePriceFormatted { get; set; }
        public string currencyCode { get; set; }
        public string currencySymbol { get; set; }
        public int dayChange { get; set; }
        public string dayChangeFormatted { get; set; }
        public double dayChangePercentage { get; set; }
        public string dayChangePercentageFormatted { get; set; }
        public object rolling52WeekHigh { get; set; }
        public object rolling52WeekHighDate { get; set; }
        public object rolling52WeekLow { get; set; }
        public object rolling52WeekLowDate { get; set; }
        public object rolling52WeekAverageDailyVolume { get; set; }
        public object calendar52WeekHigh { get; set; }
        public object calendar52WeekHighDate { get; set; }
        public object calendar52WeekLow { get; set; }
        public object calendar52WeekLowDate { get; set; }
        public object calendar52WeekAverageDailyVolume { get; set; }
    }

    public class Result
    {
        //public int providerId { get; set; }
        //public int morningstarRating { get; set; }
        //public string id { get; set; }
        //public string providerSecurityId { get; set; }
        //public string providerDataUrl { get; set; }
        //public string name { get; set; }
        //public string legalName { get; set; }
        //public object longName { get; set; }
        //public string bestName { get; set; }
        //public string searchDescription { get; set; }
        //public int type { get; set; }
        //public string typeDescription { get; set; }
        //public object orderSize { get; set; }
        //public object lotSize { get; set; }
        //public string researchUrl { get; set; }
        //public string symbol { get; set; }
        public string isin { get; set; }
        public string sedol { get; set; }
        public string currencyCode { get; set; }
        //public string currencySymbol { get; set; }
        public string market { get; set; }
        public string marketCode { get; set; }
        //public object sector { get; set; }
        //public string category { get; set; }
        //public int ajbellFund { get; set; }
        //public int cgtStatus { get; set; }
        //public string cgtStatusDescription { get; set; }
        //public Tradable tradable { get; set; }
        //public int kiidType { get; set; }
        //public int complex { get; set; }
        //public int state { get; set; }
        //public int priip { get; set; }
        //public bool kiidRequired { get; set; }
        //public string kiidDocument { get; set; }
        //public int kiidDocumentSource { get; set; }
        //public object sidDocument { get; set; }
        //public AdditonalData additonalData { get; set; }
        //public bool isInternational { get; set; }
        //public List<string> supportedOrderTypes { get; set; }
        //public Quote quote { get; set; }
        //public object providerData { get; set; }
        //public double searchScore { get; set; }
        //public int normalizedScore { get; set; }
        //public string paginationToken { get; set; }
    }

    public class Tradable
    {
        public bool buyable { get; set; }
        public bool sellable { get; set; }
        public bool ri { get; set; }
        public bool tradable { get; set; }
        public bool dripable { get; set; }
        public TradableProducts tradableProducts { get; set; }
    }

    public class TradableProducts
    {
        [JsonProperty("type-1")]
        public bool type1 { get; set; }

        [JsonProperty("type-3")]
        public bool type3 { get; set; }

        [JsonProperty("type-4")]
        public bool type4 { get; set; }

        [JsonProperty("type-2")]
        public bool type2 { get; set; }
    }
}
