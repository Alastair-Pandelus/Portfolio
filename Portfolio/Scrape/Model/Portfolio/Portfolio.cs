using Newtonsoft.Json;

namespace Portfolio.Scrape.Model.NextData
{
    public class PortfolioData
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

    public class AdditonalData
    {
        public string kiidDocument { get; set; }
        public object sidDocument { get; set; }
        public string managementFee { get; set; }
        public string ongoingCharge { get; set; }
        public string initialCharge { get; set; }
    }

    public class Asset
    {
        public int id { get; set; }
        public double value { get; set; }
        public string label { get; set; }
    }

    public class AssetCategory
    {
        public int id { get; set; }
        public double value { get; set; }
        public string label { get; set; }
    }

    public class AssetNet
    {
        public int id { get; set; }
        public double value { get; set; }
        public string label { get; set; }
    }

    public class AssetShort
    {
        public int id { get; set; }
        public double value { get; set; }
        public string label { get; set; }
    }

    public class Attributes
    {
        public string title { get; set; }
        public string id { get; set; }
        public List<string> @class { get; set; }
        public string name { get; set; }
    }

    public class LocalizedOptionsAttributes
    {
    }

    public class Below
    {
        public Link link { get; set; }
        public List<Below> below { get; set; }
        public bool dropdownsubmenu { get; set; }
    }

    public class BondStatistics
    {
        public int styleBox { get; set; }
        public double effectiveMaturity { get; set; }
        public int averageCreditQuality { get; set; }
        public double modifiedDuration { get; set; }
        public string averageCreditQualityLabel { get; set; }
    }

    public class BondStatisticsCategory
    {
        public double effectiveMaturity { get; set; }
        public int averageCreditQuality { get; set; }
        public double modifiedDuration { get; set; }
        public string averageCreditQualityLabel { get; set; }
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

    public class BreakdownValue
    {
        public int id { get; set; }
        public double value { get; set; }
        public string label { get; set; }
    }

    public class ConfiguredService
    {
        public int serviceId { get; set; }
        public string description { get; set; }
        public int state { get; set; }
    }

    public class CreditQuality
    {
        public double value { get; set; }
        public int id { get; set; }
        public string label { get; set; }
        public double benchmarkValue { get; set; }
        public int benchmarkId { get; set; }
        public string benchmarkLabel { get; set; }
    }

    public class Data
    {
        public string title { get; set; }
        public string message { get; set; }
    }

    public class EquityValuation
    {
        public double priceEarnings { get; set; }
        public double priceBook { get; set; }
        public double priceSales { get; set; }
        public double priceCashFlow { get; set; }
        public double dividendYieldFactor { get; set; }
        public double longProjectedEarningsGrowth { get; set; }
        public double historicalEarningsGrowth { get; set; }
        public double salesGrowth { get; set; }
        public double cashFlowGrowth { get; set; }
        public double bookValueGrowth { get; set; }
        public int styleBox { get; set; }
    }

    public class EquityValuationCategory
    {
        public double priceEarnings { get; set; }
        public double priceBook { get; set; }
        public double priceSales { get; set; }
        public double priceCashFlow { get; set; }
        public double dividendYieldFactor { get; set; }
        public double longProjectedEarningsGrowth { get; set; }
        public double historicalEarningsGrowth { get; set; }
        public double salesGrowth { get; set; }
        public double cashFlowGrowth { get; set; }
        public double bookValueGrowth { get; set; }
    }

    public class FigaroData
    {
        public bool isaEligibility { get; set; }
        public bool sippEligibility { get; set; }
    }

    public class FixedIncomeSector
    {
        public double value { get; set; }
        public string type { get; set; }
        public string label { get; set; }
        public double benchmarkValue { get; set; }
        public string benchmarkType { get; set; }
        public string benchmarkLabel { get; set; }
    }

    public class FixedIncomeSectorLevel2
    {
        public double value { get; set; }
        public string type { get; set; }
        public string label { get; set; }
        public double benchmarkValue { get; set; }
        public string benchmarkType { get; set; }
        public string benchmarkLabel { get; set; }
    }

    public class Footer
    {
        public Link link { get; set; }
        public List<Below> below { get; set; }
        public bool dropdownsubmenu { get; set; }
    }

    public class FundSize
    {
        public string currency { get; set; }
        public DateTime dayEndDate { get; set; }
        public int dayEndValue { get; set; }
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
        //public int cgtStatus { get; set; }
        public string cgtStatusDescription { get; set; }
        public Tradable tradable { get; set; }
        //public int? kiidType { get; set; }
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
        //public Instrument instrument { get; set; }
        //public Settings settings { get; set; }
        //public object marketOpen { get; set; }
        //public Nav nav { get; set; }
        public Portfolio portfolio { get; set; }
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
        public List<LocalizedOptionsAttributes> attributes { get; set; }
        public ItemAttributes item_attributes { get; set; }
    }

    public class MarketCapitalisation
    {
        public double notClassified { get; set; }
        public List<BreakdownValue> breakdownValues { get; set; }
    }

    public class MarketCapitalisationCat
    {
        public double notClassified { get; set; }
        public List<BreakdownValue> breakdownValues { get; set; }
    }

    public class MaturityDistribution
    {
        public string type { get; set; }
        public double value { get; set; }
        public string label { get; set; }
    }

    public class MaturityDistributionCategory
    {
        public string type { get; set; }
        public double value { get; set; }
        public string label { get; set; }
    }

    //public class Nav
    //{
    //    public List<Header> header { get; set; }
    //    public List<TopOfPageNavigation> topOfPageNavigation { get; set; }
    //    public List<Footer> footer { get; set; }
    //    public bool rebrandedHeaderFooter { get; set; }
    //}

    public class PageProps
    {
        public InstrumentData instrumentData { get; set; }
    }

    public class Portfolio
    {
        //public string name { get; set; }
        //public DateTime date { get; set; }
        //public DateTime suppressionExpiresDate { get; set; }
        public int numberOfEquities { get; set; }
        public int numberOfBonds { get; set; }
        //public List<Top10Holding> top10Holdings { get; set; }
        //public EquityValuation equityValuation { get; set; }
        //public EquityValuationCategory equityValuationCategory { get; set; }
        //public BondStatistics bondStatistics { get; set; }
        //public BondStatisticsCategory bondStatisticsCategory { get; set; }
        public List<Asset> asset { get; set; }
        public List<AssetShort> assetShort { get; set; }
        //public List<AssetNet> assetNet { get; set; }
        //public List<AssetCategory> assetCategory { get; set; }
        public List<Sector> sector { get; set; }
        //public List<SectorCategory> sectorCategory { get; set; }
        public List<Region> region { get; set; }
        //public List<RegionCategory> regionCategory { get; set; }
        //public MarketCapitalisationCat marketCapitalisationCat { get; set; }
        //public MarketCapitalisation marketCapitalisation { get; set; }
        //public List<CreditQuality> creditQuality { get; set; }
        //public List<FixedIncomeSector> fixedIncomeSectors { get; set; }
        //public List<FixedIncomeSectorLevel2> fixedIncomeSectorLevel2 { get; set; }
        //public List<FundSize> fundSize { get; set; }
        //public List<ShareClassSize> shareClassSize { get; set; }
        //public List<MaturityDistribution> maturityDistribution { get; set; }
        //public List<MaturityDistributionCategory> maturityDistributionCategory { get; set; }
    }

    public class Props
    {
        public PageProps pageProps { get; set; }
        //public bool __N_SSP { get; set; }
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
        public double dayChange { get; set; }
        public double dayChangePct { get; set; }
        public FigaroData figaroData { get; set; }
        public string isin { get; set; }
        public string sedol { get; set; }
        public string currency { get; set; }
        public double lastPrice { get; set; }
        public string lastPriceDate { get; set; }
        public double latestNavPrice { get; set; }
        public string latestNavDate { get; set; }
        public string pricingFrequency { get; set; }
        public double ongoingCharge { get; set; }
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

    public class ReadonlyConfiguration
    {
        public string key { get; set; }
        public bool enabled { get; set; }
        public Data data { get; set; }
    }

    public class Region
    {
        public int id { get; set; }
        public double value { get; set; }
        public string label { get; set; }
        public string superRegion { get; set; }
    }

    public class RegionCategory
    {
        public int id { get; set; }
        public double value { get; set; }
        public string label { get; set; }
        public string superRegion { get; set; }
    }

    public class Sector
    {
        public int id { get; set; }
        public double value { get; set; }
        public string label { get; set; }
        public string superSector { get; set; }
    }

    public class SectorCategory
    {
        public int id { get; set; }
        public double value { get; set; }
        public string label { get; set; }
        public string superSector { get; set; }
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

    public class ShareClassSize
    {
        public string currency { get; set; }
        public DateTime dayEndDate { get; set; }
        public int dayEndValue { get; set; }
    }

    public class Top10Holding
    {
        public string id { get; set; }
        public string isin { get; set; }
        public string name { get; set; }
        public string currency { get; set; }
        public string country { get; set; }
        public double weighting { get; set; }
        public int marketValue { get; set; }
        public int sharesHeld { get; set; }
        public int? changeinSharesHeld { get; set; }
        public string marketValueCurrency { get; set; }
        public string holdingType { get; set; }
        public string maturityDate { get; set; }
        public string changeinHolding { get; set; }
        public string sedol { get; set; }
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
