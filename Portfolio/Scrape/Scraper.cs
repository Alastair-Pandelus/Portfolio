using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Portfolio.EntityModel;
using Portfolio.Scrape.Api;
using Portfolio.Scrape.Model;
using Portfolio.Scrape.Model.Chart;
using Portfolio.Scrape.Model.SearchResult;

namespace Portfolio.Scrape
{
    public interface IScraper
    {
        Task ScrapeInstruments();
        Task CleanInstruments();
        Task ScrapeMarketCodes();
        Task ScrapeHoldings();
        Task ScrapeYSymbols();
        Task ScrapeRisk();
        Task ScrapePrices();
        Task AddMissingInstruments();
    };

    public class Scraper : IScraper
    {
        private readonly ICampanelloApi _campanelloApi;
        private readonly IDoofusLookupApi _doofusLookupApi;
        private readonly IDoofusQueryApi _doofusQueryApi;

        public Scraper(ICampanelloApi campanelloApi, IDoofusLookupApi doofusLookupApi, IDoofusQueryApi doofusQueryApi)
        {
            _campanelloApi = campanelloApi;
            _doofusLookupApi = doofusLookupApi;
            _doofusQueryApi = doofusQueryApi;
        }

        public async Task ScrapeInstruments()
        {
            var scrapedInstruments = await GetInstruments();

            SaveInstruments(scrapedInstruments);
        }

        private async Task<List<Instrument>> GetInstruments()
        {
            var instruments = new List<Instrument>();
            foreach (InstrumentType instrumentType in Enum.GetValues(typeof(InstrumentType)))
            {
                var instrumentsByType = await GetInstrumentsByType(instrumentType);
                instruments.AddRange(instrumentsByType);
            }

            return instruments;
        }

        private async Task<List<Instrument>> GetInstrumentsByType(InstrumentType instrumentType)
        {
            var instruments = new List<Instrument>();
            int? total = null;
            int rowsPerPage = 250;

            for(int page = 1; page==1 || (page-1)*rowsPerPage < total; page++)
            {
                try
                {
                    var pagedResults = await _campanelloApi.GetInstrumentInfosAsync(new InstrumentsPagedQuery(instrumentType, page, rowsPerPage));
                    var rows = pagedResults.Rows;

                    foreach(var row in rows)
                    {
                        row.InstrumentType = instrumentType.ToString();
                    }

                    instruments.AddRange(rows);

                    total = total ?? pagedResults.Total;
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
            }

            Console.WriteLine($"{instruments.Count} {instrumentType}s found");

            return instruments;
        }

        private static void SaveInstruments(List<Instrument> scrapedInstruments)
        {
            int count = 0;

            Parallel.ForEach(scrapedInstruments, scrapedInstrument =>
            {
                using var context = new PortfolioContext();
                var dbInstruments = context.Set<Instrument>();

                var existingInstrument
                    = dbInstruments.FirstOrDefault
                        (i =>
                            i.InstrumentType == scrapedInstrument.InstrumentType &&
                            i.ISIN == scrapedInstrument.ISIN &&
                            i.Sedol == scrapedInstrument.Sedol &&
                            i.ExchangeCode == scrapedInstrument.ExchangeCode &&
                            i.Currency == scrapedInstrument.Currency
                        );

                if (existingInstrument == null)
                {
                    dbInstruments.Add(scrapedInstrument);

                    Interlocked.Increment(ref count);
                    Console.WriteLine($"Adding [{count} of {scrapedInstruments.Count}] : {scrapedInstrument.Name}");
                    context.SaveChanges();
                }
                else
                if (!CompareInstruments(scrapedInstrument, existingInstrument))
                {
                    //existingInstrument.InstrumentType = scrapedInstrument.InstrumentType;
                    existingInstrument.Name = scrapedInstrument.Name;
                    //existingInstrument.Sedol = scrapedInstrument.Sedol;
                    //existingInstrument.ISIN = scrapedInstrument.ISIN;
                    //existingInstrument.ExchangeCode = scrapedInstrument.ExchangeCode;
                    //existingInstrument.Currency = scrapedInstrument.Currency;
                    existingInstrument.Symbol = scrapedInstrument.Symbol;
                    existingInstrument.Rating = scrapedInstrument.Rating;
                    existingInstrument.ReturnM0 = scrapedInstrument.ReturnM0;
                    existingInstrument.OngoingCharge = scrapedInstrument.OngoingCharge;
                    existingInstrument.StarRating = scrapedInstrument.StarRating;
                    existingInstrument.RatingM36 = scrapedInstrument.RatingM36;
                    existingInstrument.ReturnM1 = scrapedInstrument.ReturnM1;
                    existingInstrument.ReturnM3 = scrapedInstrument.ReturnM3;
                    existingInstrument.ReturnM6 = scrapedInstrument.ReturnM6;
                    existingInstrument.ReturnM12 = scrapedInstrument.ReturnM12;
                    existingInstrument.ReturnM36 = scrapedInstrument.ReturnM36;
                    existingInstrument.ClosePriceChange = scrapedInstrument.ClosePriceChange;
                    existingInstrument.Yield_M12 = scrapedInstrument.Yield_M12;
                    existingInstrument.Price = scrapedInstrument.Price;
                    existingInstrument.ClosePrice = scrapedInstrument.ClosePrice;
                    existingInstrument.Medalist_RatingNumber = scrapedInstrument.Medalist_RatingNumber;
                    existingInstrument.ReturnW1 = scrapedInstrument.ReturnW1;
                    existingInstrument.InvestmentTypeId = scrapedInstrument.InvestmentTypeId;
                    existingInstrument.ExchangeTradedShare = scrapedInstrument.ExchangeTradedShare;
                    existingInstrument.LegalStructureName = scrapedInstrument.LegalStructureName;
                    existingInstrument.LegalStructureId = scrapedInstrument.LegalStructureId;
                    existingInstrument.YR_ReturnM12_1 = scrapedInstrument.YR_ReturnM12_1;
                    existingInstrument.YR_ReturnM12_2 = scrapedInstrument.YR_ReturnM12_2;
                    existingInstrument.YR_ReturnM12_3 = scrapedInstrument.YR_ReturnM12_3;
                    existingInstrument.Distribution = scrapedInstrument.Distribution;
                    existingInstrument.PERatio = scrapedInstrument.PERatio;
                    existingInstrument.PriceCurrencyId = scrapedInstrument.PriceCurrencyId;
                    existingInstrument.SustainabilityRating = scrapedInstrument.SustainabilityRating;
                    existingInstrument.CustomBuyFee = scrapedInstrument.CustomBuyFee;
                    existingInstrument.ReturnM60 = scrapedInstrument.ReturnM60;
                    existingInstrument.IntradayBid = scrapedInstrument.IntradayBid;
                    existingInstrument.IntradayAsk = scrapedInstrument.IntradayAsk;
                    existingInstrument.YR_ReturnM12_4 = scrapedInstrument.YR_ReturnM12_4;
                    existingInstrument.YR_ReturnM12_5 = scrapedInstrument.YR_ReturnM12_5;
                    existingInstrument.ReturnM120 = scrapedInstrument.ReturnM120;
                    existingInstrument.IMASectorId = scrapedInstrument.IMASectorId;
                    existingInstrument.IMASectorName = scrapedInstrument.IMASectorName;
                    context.SaveChanges();

                    Interlocked.Increment(ref count);
                    Console.WriteLine($"Updating [{count} of {scrapedInstruments.Count}] : {scrapedInstrument.Name}");
                }
            }); 
        }

        public async Task CleanInstruments()
        {
            using var context = new PortfolioContext();
            var dbInstruments = context.Set<Instrument>();

            await RemoveClosedEndedFundsWithDuplicateFunds(context, dbInstruments);
            await RemoveETFsWithDuplicateFunds(context, dbInstruments);
        }

        private static async Task RemoveClosedEndedFundsWithDuplicateFunds(PortfolioContext context, DbSet<Instrument> dbInstruments)
        {
            var closedFunds = dbInstruments
                .Where(i => i.LegalStructureName == "Closed Ended Investment Company" && i.InstrumentType == "Fund")
                .ToList();

            var closedCefs = dbInstruments
                .Where(i => i.LegalStructureName == "Closed Ended Investment Company" && i.InstrumentType == "CEF")
                .ToList();

            var duplicateIdsFunds = (from fund in closedFunds
                                     join cef in closedCefs
                                     on new { fund.ISIN, fund.Sedol, fund.ExchangeCode } equals new { cef.ISIN, cef.Sedol, cef.ExchangeCode }
                                     select fund.Id).ToList();

            var rowsToDelete = dbInstruments
                .Where(i => duplicateIdsFunds.Contains(i.Id))
                .ToList();

            dbInstruments.RemoveRange(rowsToDelete);
            await context.SaveChangesAsync();
        }

        private static async Task RemoveETFsWithDuplicateFunds(PortfolioContext context, DbSet<Instrument> dbInstruments)
        {
            var funds = dbInstruments
                .Where(i => i.InstrumentType == "Fund")
                .ToList();

            var etfs = dbInstruments
                .Where(i => i.InstrumentType == "ETF")
                .ToList();

            var duplicateIdsFunds = (from fund in funds
                                     join etf in etfs
                                     on new { fund.ISIN, fund.Sedol, fund.ExchangeCode } equals new { etf.ISIN, etf.Sedol, etf.ExchangeCode }
                                     select fund.Id).ToList();

            var rowsToDelete = dbInstruments
                .Where(i => duplicateIdsFunds.Contains(i.Id))
                .ToList();

            dbInstruments.RemoveRange(rowsToDelete);
            await context.SaveChangesAsync();
        }

        private static bool CompareInstruments(Instrument scrapedInstrument, Instrument existingInstrument)
        {
            var type = typeof(Instrument);
            var scrapeProperties
                = type
                    .GetProperties()
                    .Where(p => p.Name == "MarketCode")
                    .ToList();

            foreach (var prop in scrapeProperties)
            {
                // Skip key or calculated properties
                if (prop.Name == "Id" || prop.Name=="Sedol" || prop.Name=="ISIN" || prop.Name=="Currency" || prop.Name == "MarketCode")
                    continue;

                var scrapedValue = prop.GetValue(scrapedInstrument);
                var existingValue = prop.GetValue(existingInstrument);

                // Handle nulls and value comparison
                if (!object.Equals(scrapedValue, existingValue))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task ScrapeMarketCodes()
        {
            using var context = new PortfolioContext();

            var instrumentKeys
                = await context.Instrument
                            .Where(i => i.ISIN != null && i.MarketCode == null)
                            //.Where(i => i.ISIN == "IE00BJK9HD13")
                            .Select(i => new { i.ISIN, i.Sedol })
                            .Distinct()
                            .ToListAsync();

            // Setting too high triggers errors
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = 1
            };

            int count = 0;
            await Parallel.ForEachAsync(instrumentKeys, parallelOptions, async (key, cancellationToken) =>
            {
                using var context = new PortfolioContext();

                var isin = key.ISIN;
                var sedol = key.Sedol;
                var searchText = sedol != null ? $"{isin}&{sedol}" : $"{isin}";

                SearchResult searchResult = null;
                try
                {
                    searchResult = await _campanelloApi.Search(searchText);
                    Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error searching for {searchText}: {ex.Message}");
                    return;
                }

                foreach (var result in searchResult?.data?.results)
                {
                    var marketCode = result.marketCode;

                    bool existsMarketCode = await context.Instrument.AnyAsync(i => i.MarketCode == marketCode);

                    if (existsMarketCode)
                    {
                        Interlocked.Increment(ref count);
                        Console.WriteLine($"[{count} of {instrumentKeys.Count}] - Skipping {marketCode} as already exists");
                        continue;
                    }

                    var exchangeCode = result.market;
                    var currency = result.currencyCode;

                    var instrument = context.Instrument
                        .FirstOrDefault(i =>
                                            i.ISIN == isin &&
                                            i.Sedol == sedol &&
                                            ((i.ExchangeCode == exchangeCode) || (exchangeCode == "FUND")) &&
                                            ((i.Currency == currency) || (i.Currency.StartsWith("GB") && currency.StartsWith("GB")))
                                        );

                    if (instrument != null)
                    {
                        instrument.MarketCode = marketCode;

                        context.SaveChanges();

                        Interlocked.Increment(ref count);
                        Console.WriteLine($"[{count} of {instrumentKeys.Count}] - Saving market Code for {instrument.Name}, ISIN={instrument.ISIN} => {instrument.MarketCode}");
                    }
                }
            });
        }

        public async Task ScrapeHoldings()
        {
            using (var context = new PortfolioContext())
            {
                int count = 0;
                ParallelOptions parallelOptions = new()
                {
                    // Fails if set too high (2 too high!)
                    MaxDegreeOfParallelism = 1
                };

                for(int i=0; i < 3; i++)
                {
                    var instruments
                        = context.Instrument
                                 .Where(i => i.MarketCode != null &&
                                                !i.BondHoldings.HasValue && !i.EquityHoldings.HasValue &&
                                                !i.BondLong.HasValue && !i.BondShort.HasValue &&
                                                !i.CashLong.HasValue && !i.CashShort.HasValue &&
                                                !i.StockLong.HasValue && !i.StockShort.HasValue &&
                                                !i.OtherLong.HasValue && !i.OtherShort.HasValue &&
                                                !i.RatioTechnology.HasValue && !i.RatioUnitedStates.HasValue
                                        )
                                 .ToList();

                    await Parallel.ForEachAsync(instruments.Select(i => i.Id), parallelOptions, async (instrumentId, cancellationToken) =>
                    {
                        using var context = new PortfolioContext();
                        var instrument = await context.Instrument
                            .Include(i => i.Prices)
                            .FirstOrDefaultAsync(i => i.Id == instrumentId);

                        if (await ScrapeHoldings(instrument))
                        {
                            await context.SaveChangesAsync(cancellationToken);
                            Interlocked.Increment(ref count);
                            Console.WriteLine($"[{count} of {instruments.Count}] - Saving Portfolio {instrument.Name}");
                        }
                    });
                }
            }
        }

        private async Task<bool> ScrapeHoldings(Instrument instrument)
        {
            string marketCode = instrument.MarketCode;

            try
            {
                var html = await _campanelloApi.GetPortfolioAsync(marketCode);
                var document = new HtmlDocument();
                document.LoadHtml(html);
                var documentNode = document.DocumentNode;

                var data = documentNode.SelectSingleNode($"//script[@id='__NEXT_DATA__']");

                if (data != null)
                {
                    var json = data.InnerText;
                    var nextData = JsonConvert.DeserializeObject<Model.NextData.PortfolioData>(json);
                    var portfolio = nextData.props.pageProps.instrumentData.portfolio;

                    var asset = portfolio.asset;
                    var assetShort = portfolio.assetShort;
                    var region = portfolio.region;
                    var sector = portfolio.sector;

                    instrument.StockLong = (asset.Count > 0 ? asset[0].value : 0.0) / 100.0;
                    instrument.BondLong = (asset.Count > 1 ? asset[1].value : 0.0) / 100.0;
                    instrument.CashLong = (asset.Count > 2 ? asset[2].value : 0.0) / 100.0;
                    instrument.OtherLong = (asset.Count > 3 ? asset[3].value : 0.0) / 100.0;

                    instrument.StockShort = (assetShort.Count > 0 ? assetShort[0].value : 0.0) / 100.0;
                    instrument.BondShort = (assetShort.Count > 1 ? assetShort[1].value : 0.0) / 100.0;
                    instrument.CashShort = (assetShort.Count > 2 ? assetShort[2].value : 0.0) / 100.0;
                    instrument.OtherShort = (assetShort.Count > 3 ? assetShort[3].value : 0.0) / 100.0;

                    instrument.RatioUnitedStates = (region.Count > 0 ? region[0].value : 0.0) / 100.0;
                    instrument.RatioTechnology = (sector.Count > 10 ? sector[10].value : 0.0) / 100.0;

                    instrument.EquityHoldings = portfolio.numberOfEquities;
                    instrument.BondHoldings = portfolio.numberOfBonds;

                    return true;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("404"))
                {
                    Console.WriteLine($"{instrument.Name} ISIN={instrument.ISIN} => no portfolio {((Refit.ApiException)ex).RequestMessage.RequestUri.OriginalString}");
                }
                else
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            return false;
        }

        public async Task ScrapeYSymbols()
        {
            using var context = new PortfolioContext();

            var instruments
                = context.Instrument
                        .Where(i => i.MarketCode != null && i.Y_Symbol == null)
                        .ToList();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            int count = 0;
            await Parallel.ForEachAsync(instruments.Select(i => i.Id), parallelOptions, async (instrumentId, cancellationToken) =>
            {
                using var context = new PortfolioContext();
                var instrument = await context.Instrument.Where(i => i.Id == instrumentId).FirstOrDefaultAsync();
                var symbol = await GetYSymbolAsync(instrument);

                if (symbol != null)
                {
                    instrument.Y_Symbol = symbol;
                    context.SaveChanges();

                    Interlocked.Increment(ref count);
                    Console.WriteLine($"[{count} of {instruments.Count}] - Saving Symbol {instrument.Name} ({instrument.ISIN}) = {symbol}");
                }
            });
        }

        private async Task<string> GetYSymbolAsync(Instrument instrument)
        {
            try
            {
                var quoteResult = await _doofusLookupApi.LookupAsync(instrument.ISIN);

                return quoteResult.quotes?[0].symbol;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return null;
        }

        public async Task ScrapeRisk()
        {
            using var context = new PortfolioContext();

            var instruments
                = context.Instrument
                    //.Where(i => i.ISIN == "IE000AI6QNJ5")                    
                    .Where(i => i.MarketCode != null && i.RiskRating == null)
                    .ToList();

            //var parallelOptions = new ParallelOptions
            //{
            //    MaxDegreeOfParallelism = 1
            //};

            int count = 0;
            //await Parallel.ForEachAsync(instruments.Select(x => x.Id), parallelOptions, async (instrumentId, cancellationToken) =>
            foreach(var instrument in instruments)
            {
                //using var context = new PortfolioContext();
                //var instrument = context.Instrument.FirstOrDefault(i => i.Id == instrumentId);

                var updated = await UpdateRiskValues(instrument);

                if (updated)
                {
                    context.SaveChanges();

                    Interlocked.Increment(ref count);
                    Console.WriteLine($"[{count} of {instruments.Count}] - Saving Risk Rating {instrument.Name} ({instrument.ISIN}) = {instrument.RiskRating}");
                }
                else
                {
                    Interlocked.Increment(ref count);
                    Console.WriteLine($"[{count} of {instruments.Count}] - No Risk Rating found for {instrument.Name} ({instrument.ISIN})");
                }
            }
        }

        private async Task<bool> UpdateRiskValues(Instrument instrument)
        {
            try
            {
                var html = await _campanelloApi.GetRiskAndRatings(instrument.MarketCode);

                var document = new HtmlDocument();
                document.LoadHtml(html);
                var documentNode = document.DocumentNode;

                var data = documentNode.SelectSingleNode($"//script[@id='__NEXT_DATA__']");

                if (data != null)
                {
                    var json = data.InnerText;
                    var riskAndRatingsData = JsonConvert.DeserializeObject<Model.RiskAndRatings.RiskAndRatingsData>(json);
                    instrument.RiskRating = riskAndRatingsData.props.pageProps.instrumentData.ratings.SRRI;   

                    return instrument.RiskRating.HasValue;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                Console.WriteLine($"[No Risk Rating found for {instrument.Name} - {ex.Message}");
            }

            return false;
        }

        public async Task ScrapePrices()
        {
            using var context = new PortfolioContext();

            var instruments = context.Instrument
                .Where(i => i.Y_Symbol != null)
                .Include(i => i.Prices)
                .Where(i => i.Prices.Count == 0)
                .ToList();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            int count = 0;
            await Parallel.ForEachAsync(instruments, parallelOptions, async (instrument, cancellationToken) =>
            {
                var addedPrices = await GetPrices(instrument.Id);
                if (addedPrices > 0)
                {
                    Interlocked.Increment(ref count);
                    Console.WriteLine($"[{count} of {instruments.Count}] - Saving {addedPrices} Prices - {instrument.Name} ({instrument.ISIN})");
                }
            });
        }

        private async Task<int> GetPrices(int instrumentId)
        {
            using var context = new PortfolioContext();
            var instrument = await context.Instrument
                .Include(i => i.Prices)
                .FirstOrDefaultAsync(i => i.Id == instrumentId);

            int addedValues = 0;

            try
            {
                long period2 = new DateTimeOffset(DateTime.Today).ToUnixTimeSeconds();
                var json = await _doofusQueryApi.ChartAsync(instrument.Y_Symbol, period2);

                if (json != null)
                {
                    var chartResult = JsonConvert.DeserializeObject<ChartResult>(json);
                    var result0 = chartResult.chart.result[0];

                    var unixTimestamp = result0.timestamp;
                    var dateTimes = unixTimestamp.Select(ts => DateTimeOffset.FromUnixTimeSeconds(ts).DateTime).ToList();
                    var adjustedCloseValues = result0.indicators.adjclose[0].adjclose;

                    int count = int.Max(dateTimes.Count, adjustedCloseValues.Count);

                    var nonNullValues = Enumerable
                        .Range(0, count)
                        .Where(i => adjustedCloseValues[i] != null && adjustedCloseValues[i] != 0)
                        .Select(i => new Price 
                                        { 
                                            InstrumentId = instrument.Id, 
                                            Date = System.DateOnly.FromDateTime(dateTimes[i]), 
                                            Value = adjustedCloseValues[i].Value 
                                        }
                                )
                        .ToArray();

                    for (int i=0; i < nonNullValues.Length; i++)
                    {
                        var dateTime = nonNullValues[i].Value;
                        var adjustedClose = nonNullValues[i].Value;

                        if(i > 0)
                        {
                            var delta = nonNullValues[i].Value / nonNullValues[i-1].Value;

                            // Fix prices that jump around 100% or 0.01%
                            if (delta > 80 && delta < 120)
                            {
                                nonNullValues[i].Value /= 100;
                            }
                            else
                            if (delta > 0.008 && delta < 0.012)
                            {
                                nonNullValues[i].Value *= 100;
                            }
                        }

                        // Check if the price already exists
                        var existingPrice = instrument.Prices.FirstOrDefault(p => p.Date == nonNullValues[i].Date);
                        if (existingPrice != null)
                        {
                            // Update the existing price
                            existingPrice.Value = nonNullValues[i].Value;
                        }
                        else
                        {
                            // Add new price
                            instrument.Prices.Add(nonNullValues[i]);
                        }

                        addedValues = instrument.Prices.Count();
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return addedValues;
        }

        public async Task AddMissingInstruments()
        {
            using var context = new PortfolioContext();
            var dbInstruments = context.Set<Instrument>();

            Instrument[] manualInstruments =
            [
                new() { ISIN = "LU1001748638", Name="JPM Europe Equity Absolute Alpha C (perf) (dist) - GBP (hedged)", Currency="GBP", Sedol="BH4GX47", Y_Symbol="0P00011M7T.L", InstrumentType="Fund", Distribution="acc" }
            ];

            foreach(Instrument manualInstrument in manualInstruments)
            {
                var existingInstrument = await context.Instrument
                    .FirstOrDefaultAsync(i => i.ISIN == manualInstrument.ISIN);

                if (existingInstrument == null)
                {
                    dbInstruments.Add(manualInstrument);
                }
                else
                {
                    existingInstrument.Name = manualInstrument.Name;
                    existingInstrument.Currency = manualInstrument.Currency;
                    existingInstrument.Sedol = manualInstrument.Sedol;
                    existingInstrument.Y_Symbol = manualInstrument.Y_Symbol;
                    existingInstrument.InstrumentType = manualInstrument.InstrumentType;
                    existingInstrument.Distribution = manualInstrument.Distribution;
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
