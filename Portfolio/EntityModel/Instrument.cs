using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Portfolio.EntityModel;

[Index(nameof(InstrumentType), nameof(Sedol), nameof(ISIN), nameof(ExchangeCode), nameof(Currency), IsUnique = true)]
public partial class Instrument
{
    public int Id { get; set; }

    public virtual ICollection<ProxyInstrument> Proxies { get; set; } = [];

    public string InstrumentType { get; set; } 

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("sedol")]
    public string Sedol { get; set; }

    [JsonPropertyName("isin")]
    public string ISIN { get; set; }

    [JsonPropertyName("ExchangeCode")]
    public string ExchangeCode { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    public string MarketCode { get; set; }
    public string Y_Symbol { get; set; }

    [JsonPropertyName("Symbol")]
    public string Symbol { get; set; }
    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("ReturnM0")]
    public double ReturnM0 { get; set; }

    [JsonPropertyName("OngoingCharge")]
    public double OngoingCharge { get; set; }

    [JsonPropertyName("StarRating")]
    public string StarRating { get; set; }

    [JsonPropertyName("RatingM36")]
    public int RatingM36 { get; set; }

    [JsonPropertyName("ReturnM1")]
    public double ReturnM1 { get; set; }

    [JsonPropertyName("ReturnM3")]
    public double ReturnM3 { get; set; }

    [JsonPropertyName("ReturnM6")]
    public double ReturnM6 { get; set; }

    [JsonPropertyName("ReturnM12")]
    public double ReturnM12 { get; set; }

    [JsonPropertyName("ReturnM36")]
    public double ReturnM36 { get; set; }

    [JsonPropertyName("ClosePriceChange")]
    public double ClosePriceChange { get; set; }

    [JsonPropertyName("Yield_M12")]
    public double Yield_M12 { get; set; }

    [JsonPropertyName("Price")]
    public double Price { get; set; }

    [JsonPropertyName("ClosePrice")]
    public double ClosePrice { get; set; }

    [JsonPropertyName("Medalist_RatingNumber")]
    public int Medalist_RatingNumber { get; set; }

    [JsonPropertyName("ReturnW1")]
    public double ReturnW1 { get; set; }

    [JsonPropertyName("InvestmentTypeId")]
    public string InvestmentTypeId { get; set; }

    [JsonPropertyName("ExchangeTradedShare")]
    public bool ExchangeTradedShare { get; set; }

    [JsonPropertyName("LegalStructureName")]
    public string LegalStructureName { get; set; }

    [JsonPropertyName("LegalStructureId")]
    public string LegalStructureId { get; set; }

    [JsonPropertyName("YR_ReturnM12_1")]
    public double YR_ReturnM12_1 { get; set; }

    [JsonPropertyName("YR_ReturnM12_2")]
    public double YR_ReturnM12_2 { get; set; }

    [JsonPropertyName("YR_ReturnM12_3")]
    public double YR_ReturnM12_3 { get; set; }

    [JsonPropertyName("Distribution")]
    public string Distribution { get; set; }

    [JsonPropertyName("PERatio")]
    public double PERatio { get; set; }

    [JsonPropertyName("priceCurrencyId")]
    public string PriceCurrencyId { get; set; }

    [JsonPropertyName("SustainabilityRating")]
    public int? SustainabilityRating { get; set; }

    [JsonPropertyName("CustomBuyFee")]
    public double? CustomBuyFee { get; set; }

    [JsonPropertyName("ReturnM60")]
    public double? ReturnM60 { get; set; }

    [JsonPropertyName("Intraday.Bid")]
    public double? IntradayBid { get; set; }

    [JsonPropertyName("Intraday.Ask")]
    public double? IntradayAsk { get; set; }

    [JsonPropertyName("YR_ReturnM12_4")]
    public double? YR_ReturnM12_4 { get; set; }

    [JsonPropertyName("YR_ReturnM12_5")]
    public double? YR_ReturnM12_5 { get; set; }

    [JsonPropertyName("ReturnM120")]
    public double? ReturnM120 { get; set; }

    [JsonPropertyName("IMASectorId")]
    public string IMASectorId { get; set; }

    [JsonPropertyName("IMASectorName")]
    public string IMASectorName { get; set; }

    public double? StockLong { get; set; }
    public double? StockShort { get; set; }
    public double? BondLong { get; set; }
    public double? BondShort { get; set; }
    public double? CashLong { get; set; }
    public double? CashShort { get; set; }
    public double? OtherLong { get; set; }
    public double? OtherShort { get; set; }
    public double? RatioUnitedStates { get; set; }
    public double? RatioTechnology { get; set; }
    public int? EquityHoldings { get; set; }
    public int? BondHoldings { get; set; }
    public int? RiskRating { get; set; }
    public double? MaxDrawdown { get; set; }

    public double? Correlation { get; set; }
    public List<Price> Prices { get; set; }
    public bool Watchlist { get; set; } = false;
    public string Comment { get; set; } = string.Empty;
}