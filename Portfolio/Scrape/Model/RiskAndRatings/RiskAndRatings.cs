using Newtonsoft.Json;

namespace Portfolio.Scrape.Model.RiskAndRatings
{
    public class RiskAndRatingsData
    {
        public Props props { get; set; }
        //public string page { get; set; }
        //public Query query { get; set; }
        //public string buildId { get; set; }
        //public string assetPrefix { get; set; }
        //public bool isFallback { get; set; }
        //public bool gssp { get; set; }
        //public List<object> scriptLoader { get; set; }
    }

    public class Props
    {
        public PageProps pageProps { get; set; }
        //public bool __N_SSP { get; set; }
    }

    public class AdditonalData
    {
        public string kiidDocument { get; set; }
        public object sidDocument { get; set; }
        public string managementFee { get; set; }
        public string ongoingCharge { get; set; }
        public string initialCharge { get; set; }
    }

    public class Attributes
    {
        public string title { get; set; }
        public string id { get; set; }
        public List<string> @class { get; set; }
        public string name { get; set; }
    }

    public class Below
    {
        public Link link { get; set; }
        public List<Below> below { get; set; }
        public bool dropdownsubmenu { get; set; }
    }

    public class Benchmark
    {
        public string type { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }

    public class Brand
    {
        public string name { get; set; }
        public string code { get; set; }
        public string pricingModel { get; set; }
        public object valuationFormat { get; set; }
        public string supportPhone { get; set; }
        public string supportPhoneGeneric { get; set; }
        public string tradeSupportHours { get; set; }
    }

    public class ConfiguredService
    {
        public int serviceId { get; set; }
        public string description { get; set; }
        public int state { get; set; }
    }

    public class Data
    {
        public string title { get; set; }
        public string message { get; set; }
    }

    public class FigaroData
    {
        public bool isaEligibility { get; set; }
        public bool sippEligibility { get; set; }
    }

    public class Footer
    {
        public Link link { get; set; }
        public List<Below> below { get; set; }
        public bool dropdownsubmenu { get; set; }
    }

    public class Header
    {
        public Link link { get; set; }
        public List<Below> below { get; set; }
        public bool dropdownsubmenu { get; set; }
    }

    public class Instrument
    {
        public int providerId { get; set; }
        public int morningstarRating { get; set; }
        public string id { get; set; }
        public string providerSecurityId { get; set; }
        public string providerDataUrl { get; set; }
        public string name { get; set; }
        public string legalName { get; set; }
        public object longName { get; set; }
        public string bestName { get; set; }
        public string searchDescription { get; set; }
        public int type { get; set; }
        public string typeDescription { get; set; }
        public object orderSize { get; set; }
        public object lotSize { get; set; }
        public string researchUrl { get; set; }
        public string symbol { get; set; }
        public string isin { get; set; }
        public string sedol { get; set; }
        public string currencyCode { get; set; }
        public string currencySymbol { get; set; }
        public string market { get; set; }
        public string marketCode { get; set; }
        public object sector { get; set; }
        public string category { get; set; }
        public int ajbellFund { get; set; }
        //public int? cgtStatus { get; set; }
        public string cgtStatusDescription { get; set; }
        public Tradable tradable { get; set; }
        public int? kiidType { get; set; }
        //public int complex { get; set; }
        //public int state { get; set; }
        //public int priip { get; set; }
        public bool kiidRequired { get; set; }
        public string kiidDocument { get; set; }
        public int? kiidDocumentSource { get; set; }
        public object sidDocument { get; set; }
        public AdditonalData additonalData { get; set; }
        public bool isInternational { get; set; }
        public List<string> supportedOrderTypes { get; set; }
        public Quote quote { get; set; }
        public object providerData { get; set; }
        public object searchScore { get; set; }
        public object normalizedScore { get; set; }
        public object paginationToken { get; set; }
        public FigaroData figaroData { get; set; }
    }

    public class InstrumentData
    {
        //public List<object> favorites { get; set; }
        //public Quote quote { get; set; }
        //public InstrumentInfo instrumentInfo { get; set; }
        //public SecuritySummary securitySummary { get; set; }
        public Instrument instrument { get; set; }
        //public Settings settings { get; set; }
        //public object marketOpen { get; set; }
        //public Nav nav { get; set; }
        public Ratings ratings { get; set; }
        //public Performance performance { get; set; }
    }

    public class InstrumentInfo
    {
        public string type { get; set; }
        public string msid { get; set; }
    }

    public class ItemAttributes
    {
        public string id { get; set; }
        public string @class { get; set; }
        public string style { get; set; }
    }

    public class Link
    {
        public string link_path { get; set; }
        public string link_title { get; set; }
        public LocalizedOptions localized_options { get; set; }
    }

    public class LocalizedOptions
    {
        public int always_visible { get; set; }
        public int alter { get; set; }
        public Attributes attributes { get; set; }
        public ItemAttributes item_attributes { get; set; }
    }

    public class Misc
    {
        public Morningstar morningstar { get; set; }
    }

    public class Morningstar
    {
        public string baseUrl { get; set; }
        public string solutionKey { get; set; }
    }

    public class Nav
    {
        public List<Header> header { get; set; }
        public List<TopOfPageNavigation> topOfPageNavigation { get; set; }
        public List<Footer> footer { get; set; }
        public bool rebrandedHeaderFooter { get; set; }
    }

    public class PageProps
    {
        public InstrumentData instrumentData { get; set; }
        //public Misc misc { get; set; }
    }

    public class Performance
    {
        public DateTime asOfDate { get; set; }
        public List<Benchmark> benchmarks { get; set; }
    }

    public class Query
    {
        public string marketCode { get; set; }
    }

    public class Quote
    {
        public string __typename { get; set; }
        public string name { get; set; }
        public string nameShort { get; set; }
        public string marketCode { get; set; }
        public string exchangeCode { get; set; }
        public double? dayChange { get; set; }
        public double? dayChangePct { get; set; }
        public FigaroData figaroData { get; set; }
        public string isin { get; set; }
        public string sedol { get; set; }
        public string currency { get; set; }
        public double? lastPrice { get; set; }
        public string lastPriceDate { get; set; }
        public double? latestNavPrice { get; set; }
        public string latestNavDate { get; set; }
        public string pricingFrequency { get; set; }
        public double? ongoingCharge { get; set; }
        public bool ucits { get; set; }
        public string instrumentStructure { get; set; }
        public string sectorName { get; set; }
        public int yield { get; set; }
        public int rating { get; set; }
        public string dealingCutOffTime { get; set; }
        public object incomeFrequency { get; set; }
        public string symbol { get; set; }
        public object lastTradeDate { get; set; }
        public int time { get; set; }
        public string lastPriceTime { get; set; }
        public double? price { get; set; }
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
        public double? lastClosePrice { get; set; }
        public string lastClosePriceFormatted { get; set; }
        public string currencyCode { get; set; }
        public string currencySymbol { get; set; }
        public string dayChangeFormatted { get; set; }
        public double? dayChangePercentage { get; set; }
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

    public class Ratings
    {
        //public int analystRating { get; set; }
        public int? SRRI { get; set; }
        //public DateTime SRRIDate { get; set; }
        //public object sustainabilityScore { get; set; }
        //public DateTime sustainabilityDate { get; set; }
        //public object sustainabilityRating { get; set; }
        //public object sustainabilityRank { get; set; }
        //public object sustainabilityPercentRank { get; set; }
        //public object sustainabilityHistoricalScore { get; set; }
        //public string analystRatingLabel { get; set; }
        //public List<RiskRating> riskRating { get; set; }
        //public List<RiskStatistic> riskStatistics { get; set; }
    }

    public class ReadonlyConfiguration
    {
        public string key { get; set; }
        public bool enabled { get; set; }
        public Data data { get; set; }
    }

    public class RiskRating
    {
        public string period { get; set; }
        public double? riskAdjustedReturn { get; set; }
        public int rating { get; set; }
        public int riskRating { get; set; }
        public string riskRatingLabel { get; set; }
        public int performanceRating { get; set; }
        public string performanceRatingLabel { get; set; }
        public double? categoryRank { get; set; }
        public DateTime date { get; set; }
    }

    public class RiskStatistic
    {
        public string period { get; set; }
        public double? alpha { get; set; }
        public double? beta { get; set; }
        public double? rSquared { get; set; }
        public double? arithmeticMean { get; set; }
        public double? sortinoRatio { get; set; }
        public double? standardDeviation { get; set; }
        public double? sharpeRatio { get; set; }
        public int? noPositiveMonths { get; set; }
        public int? noNegativeMonths { get; set; }
        public double? worstMonth { get; set; }
        public object recoveryDate { get; set; }
    }

    public class SecuritySummary
    {
        public object marketCode { get; set; }
    }

    public class Settings
    {
        public int timestamp { get; set; }
        public int timeout { get; set; }
        public bool isAdmin { get; set; }
        public Brand brand { get; set; }
        public List<object> permissions { get; set; }
        public bool enableMemoAssets { get; set; }
        public object ipAddress { get; set; }
        public bool isWso2Login { get; set; }
        public List<ConfiguredService> configuredServices { get; set; }
        public ReadonlyConfiguration readonlyConfiguration { get; set; }
        public string mentionMePromotionEndpoint { get; set; }
        public object zopaMigrated { get; set; }
        public object updateDetailsMigration { get; set; }
        public List<string> tabs { get; set; }
        public bool gdprRedirect { get; set; }
        public string appId { get; set; }
        public List<string> vulnerableCustomerCategoriesComplexInstrumentWarning { get; set; }
        public bool backofficeMaintenance { get; set; }
        public string baseUrl { get; set; }
    }

    public class TopOfPageNavigation
    {
        public Link link { get; set; }
        public List<object> below { get; set; }
        public bool dropdownsubmenu { get; set; }
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
