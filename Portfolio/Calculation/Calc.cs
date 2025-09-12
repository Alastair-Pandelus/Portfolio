using Microsoft.EntityFrameworkCore;
using Portfolio.EntityModel;
using MathNet.Numerics.Statistics;
using Microsoft.Identity.Client;

namespace Portfolio.Calculation
{
    public interface ICalc
    {
        //public Task SynthesiseMissingPrices();
        public Task SetMaxDrawdown();
        public Task SetMarketCorrelations();
        //public Task SetProxyInstruments();
        public Task SetAdjustedReturns();
        //public Task GetWatchlistCorrelations();
        public Task SetSharpeRatio();
    }

    public class Calc : ICalc
    {
        //public async Task SynthesiseMissingPrices()
        //{
        //    using var context = new PortfolioContext();

        //    var dateRange = await context.Set<EntityModel.PriceDate>().FromSqlRaw
        //        (@"select distinct([Date]) from dbo.Price order by [Date] desc").ToListAsync();

        //    var instruments = await context.Set<Instrument>()
        //        .ToListAsync();

        //    int i = 0;
        //    foreach(var instrument in instruments)
        //    {
        //        int count = 0;

        //        var instrumentPrices = await context.Set<Price>()
        //            .Where(p => p.InstrumentId == instrument.Id)
        //            .ToListAsync();

        //        if(instrumentPrices.Count < 2)
        //        {
        //            Console.WriteLine($"[{++i} of {instruments.Count}] - Instrument {instrument.Name} has less than 2 prices, skipping.");
        //            continue;
        //        }

        //        var earliestPriceDate = instrumentPrices
        //            .Select(p => p.Date)
        //            .Min();

        //        var latestPriceDate = instrumentPrices
        //            .Select(p => p.Date)
        //            .Max();

        //        var instrumentPriceDates = instrumentPrices
        //            .Select(p => p.Date)
        //            .ToList();

        //        var missingDates = dateRange.Select(d => d.Value)
        //            .Where(d => d >= earliestPriceDate && d <= latestPriceDate && !instrumentPriceDates.Contains(d))
        //            .ToList();

        //        foreach(var missingDate in missingDates)
        //        {
        //            var nearestPrice = instrumentPrices
        //                .Where(p => p.Date < missingDate)
        //                .OrderByDescending(p => p.Date)
        //                .FirstOrDefault();

        //            var newPrice = new Price
        //            {
        //                InstrumentId = instrument.Id,
        //                Date = missingDate,
        //                Value = nearestPrice.Value  
        //            };
        //            context.Set<Price>().Add(newPrice);
        //            count++;
        //        }

        //        if (count > 0)
        //        {
        //            context.SaveChanges();
        //            Console.WriteLine($"[{i} of {instruments.Count}] - Synthesised {count} missing prices for instrument: {instrument.Name}");
        //        }
        //        i++;
        //    }
        //}

        public async Task SetMarketCorrelations()
        {
            using var context = new PortfolioContext();
            var dbInstruments = context.Set<Instrument>();

            var instruments = await dbInstruments.FromSqlRaw
                (
                    @"select * from Instrument where id in 
                    (
                        select InstrumentId from 
                        (
                            select  InstrumentId, 
                                    count(distinct(YEAR([date])*12+MONTH([date]))) as Months, 
                                    max(YEAR([date])*12+MONTH([date])) as MaxMonth
                                from Price
                                    where Correlation is null
                                group by InstrumentId
                        ) InstrumentMonths
                        where InstrumentMonths.Months > 36 and MaxMonth = YEAR(GETDATE())*12+MONTH(GETDATE())
                    )
                    union 
                    select * from Instrument where Symbol = 'ACWI'"
               )
               //.Where(i => i.Symbol == "ACWI" || i.ISIN== "GB00B0CNH056")
               .ToListAsync();

            var acwi = instruments.FirstOrDefault(i => i.Symbol == "ACWI");
            if (acwi is null)
            {
                throw new Exception("ACWI benchmark not found");
            }

            var acwiMonthlyPrices = await GetMonthlyPrices(acwi.Id);

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            int count = 0;
            await Parallel.ForEachAsync(instruments, parallelOptions, async (instrument, cancellationToken) =>
            {
                var instrumentPrices = await GetMonthlyPrices(instrument.Id);

                var correlation = await GetCorrelation(acwiMonthlyPrices, instrumentPrices, instrument.Id);

                if (correlation != null)
                {
                    Interlocked.Increment(ref count);
                    Console.WriteLine($"[{count} of {instruments.Count}] Instrument: {instrument.Name}, Correlation with ACWI: {correlation:F4}");
                }
            });
        }

        //public async Task SetProxyInstruments()
        //{
        //    using var context = new PortfolioContext();
        //    var dbInstruments = context.Set<Instrument>();

        //    var ranmoreGlobalEquityInstitutionalGBP = await dbInstruments
        //        .FirstOrDefaultAsync(i => i.Name == "Ranmore Global Equity Institutional GBP");

        //    var ranmoreGlobalEquityInvestorGBP = await dbInstruments
        //        .FirstOrDefaultAsync(i => i.Name == "Ranmore Global Equity Investor GBP");

        //    //var dbProxies = context.Set<ProxyInstrument>();
        //}

        public async Task SetAdjustedReturns()
        {
            using var context = new PortfolioContext();
            var dbInstruments = context.Set<Instrument>();

            var instrumentIds = dbInstruments.Select(i => i.Id).ToList();

            int count = 0;
            foreach (var instrumentId in instrumentIds)
            {
                var instrument = await SetInstrumentAdjustedReturns(dbInstruments, context, instrumentId);
                Console.WriteLine($"[{++count} of {instrumentIds.Count}] - Set Adjusted Returns for Instrument: {instrument.Name} (ISIN: {instrument.ISIN})");
                context.Entry(instrument).State = EntityState.Detached;
            }
        }

        public static async Task<Instrument> SetInstrumentAdjustedReturns(DbSet<Instrument> dbInstruments, PortfolioContext context, int instrumentId)
        {
            var instrument = dbInstruments.Where(i => i.Id == instrumentId).FirstOrDefault();
            var instrumentEntry = context.Entry(instrument);

            Dictionary<int, List<
                Price>> prices = [];

            await instrumentEntry
                .Collection(i => i.Prices)
                .LoadAsync();

            prices[instrument.Id] = [.. instrument.Prices.OrderByDescending(p => p.Date)];

            await instrumentEntry
                    .Collection(i => i.AdjustedReturns)
                    .LoadAsync();

            var adjustedReturns = instrument.AdjustedReturns;

            DateOnly? currentDate = null;    
            for (int i = 0; i < prices[instrument.Id].Count - 1; i++)
            {
                var currentPrice = prices[instrument.Id][i];
                var previousPrice = prices[instrument.Id][i + 1];
                currentDate = currentPrice.Date;

                // Use natural log (LN in Excel)
                var adjustedReturn = new AdjustedReturn
                {
                    InstrumentId = instrument.Id,
                    Date = currentPrice.Date,
                    LogValue = Math.Log(1+((currentPrice.Value - previousPrice.Value)/previousPrice.Value))
                };

                var existingReturn = adjustedReturns.FirstOrDefault(ar => ar.Date == adjustedReturn.Date);

                if(existingReturn != null)
                {
                    // If an adjusted return already exists for this date, update it
                    existingReturn.LogValue = adjustedReturn.LogValue;
                }
                else
                {
                    // Otherwise, add the new adjusted return
                    adjustedReturns.Add(adjustedReturn);
                }
            }
            await context.SaveChangesAsync();

            return instrument;
        }

        //private double GetWeightedProxyPriceDelta(ICollection<ProxyInstrument> proxies, Dictionary<int, List<Price>> prices, int priceOffset)
        //{
        //    var weightedValue = 0.0;

        //    foreach (var proxy in proxies)
        //    {
        //        weightedValue += (proxy.Weight * prices[proxy.ProxyId][priceOffset].Value / prices[proxy.ProxyId][priceOffset+1].Value);
        //    }
        //    return weightedValue;
        //}

        //private static async Task<DateOnly?> GetProxyPrices(DbSet<Instrument> dbInstruments, Dictionary<int, List<Price>> prices, DateOnly? minSharedProxyInstrumentPriceDate, ProxyInstrument proxy)
        //{
        //    var proxyInstrument = await dbInstruments
        //        .Include(i => i.Prices)
        //        .FirstOrDefaultAsync(i => i.Id == proxy.ProxyId);

        //    var minProxyInstrumentDate = proxyInstrument.Prices.Select(p => p.Date).Min().ToDateTime(TimeOnly.MinValue);

        //    if (minSharedProxyInstrumentPriceDate == null)
        //    {
        //        minSharedProxyInstrumentPriceDate = DateOnly.FromDateTime(minProxyInstrumentDate);
        //    }
        //    else
        //    {
        //        if(minProxyInstrumentDate.Ticks > minSharedProxyInstrumentPriceDate.Value.ToDateTime(TimeOnly.MinValue).Ticks)
        //        {
        //            // If the proxy instrument date starts later than the shared date, update the shared date
        //            minSharedProxyInstrumentPriceDate = DateOnly.FromDateTime(minProxyInstrumentDate);
        //        }
        //    }

        //    prices[proxyInstrument.Id] = [.. proxyInstrument.Prices.OrderByDescending(p => p.Date)];
        //    return minSharedProxyInstrumentPriceDate;
        //}

        //public async Task GetWatchlistCorrelations()
        //{
        //    using var context = new PortfolioContext();
        //    var dbInstruments = context.Set<Instrument>();
        //    var dbCorrelations = context.Set<EntityModel.Correlation>();

        //    var instruments = await dbInstruments.Where(i => i.Watchlist).ToListAsync();
        //    var instrumentIds = instruments.Select(i => i.Id).ToList();

        //    var instrument5YearDeltas =
        //        (await context.InstrumentMonthlyDelta
        //            .Where(d => d.Month > -60)
        //            .ToListAsync())
        //            .ToList()
        //            .Where(i => instrumentIds.Contains(i.InstrumentId))
        //            .ToList();

        //    //var existingCorrelations = await dbCorrelations
        //    //    .Where(c => instrumentIds.Contains(c.Instrument1Id) && instrumentIds.Contains(c.Instrument2Id))
        //    //    .ToDictionaryAsync(c => c.Instrument1Id, c => c.Instrument2Id);

        //    Dictionary<int, double[]> instrumentDeltas = [];

        //    foreach (var instrumentId in instrumentIds)
        //    {
        //        var deltas = instrument5YearDeltas
        //            .Where(i => i.InstrumentId == instrumentId)
        //            // 0 is current month, -1 is previous month, etc.
        //            .OrderByDescending(i => i.Month)
        //            .Select(i => i.Delta)
        //            .ToArray();

        //        instrumentDeltas[instrumentId] = deltas;
        //    }

        //    int count = 0;
        //    var maxCount = ((instrumentDeltas.Count * instrumentDeltas.Count) / 2) - 1;
        //    for (int i = 0; i < instrumentDeltas.Count; i++)
        //    {
        //        var id1 = instrumentIds[i];
        //        var name1 = instruments.FirstOrDefault(i => i.Id == id1).Name;
        //        var deltas1 = instrumentDeltas[id1];

        //        if (deltas1.Length < 36)
        //        {
        //            Console.WriteLine($"[{++count} of ({maxCount}] - Skipping {name1} due to insufficient data (less than 3 years)");
        //            continue; // Skip instruments with less than 3 years of data
        //        }

        //        for (int j = i + 1; j < instrumentDeltas.Count; j++)
        //        {
        //            var id2 = instrumentIds[j];

        //            //if (existingCorrelations.Any(c => (c.Instrument1Id == id1 && c.Instrument2Id == id2)))
        //            //{
        //            //    Console.WriteLine($"[{++count} of ({maxCount}] - Skipping existing correlation between {name1} and {instruments.FirstOrDefault(i => i.Id == id2).Name}");
        //            //    continue; // Skip if correlation already exists
        //            //}

        //            var name2 = instruments.FirstOrDefault(j => j.Id == id2).Name;
        //            var deltas2 = instrumentDeltas[id2];

        //            if (deltas2.Length < 36)
        //            {
        //                continue; // Skip instruments with less than 3 years of data
        //            }

        //            var correlation = double.NaN;
        //            if (deltas1.Length == deltas2.Length)
        //            {
        //                // Both 5 Year worth of data
        //                correlation = MathNet.Numerics.Statistics.Correlation.Pearson(deltas1, deltas2);
        //            }
        //            else
        //            {
        //                // If not equal, try correlating down to 3 years, below that ignore
        //                var minLength = Math.Min(deltas1.Length, deltas2.Length);

        //                double[] instrument1Deltas = [.. deltas1.Take(minLength)];
        //                double[] instrument2Deltas = [.. deltas2.Take(minLength)];
        //                correlation = MathNet.Numerics.Statistics.Correlation.Pearson(instrument1Deltas, instrument2Deltas);
        //            }

        //            string msg;
        //            if (double.IsNaN(correlation))
        //            {
        //                msg = $"[{++count} of ({maxCount}] - Unable to calculate Correlation between {name1} and {name2}: {correlation:F4}";
        //            }
        //            else
        //            {
        //                var existingCorrelation = await dbCorrelations.Where(c => c.Instrument1Id==id1 && c.Instrument2Id==id2).FirstOrDefaultAsync();

        //                if (existingCorrelation == null)
        //                {
        //                    dbCorrelations.Add(new EntityModel.Correlation
        //                    {
        //                        Instrument1Id = id1,
        //                        Instrument2Id = id2,
        //                        Value = correlation
        //                    });
        //                }
        //                else
        //                {
        //                    existingCorrelation.Value = correlation;
        //                }
        //                msg = $"[{++count} of ({maxCount}] - Correlation between {name1} and {name2}: {correlation:F4}";
        //            }
        //            Console.WriteLine(msg);
        //        }

        //        // Save changes periodically per Instrument
        //        context.SaveChanges();
        //    }
        //}

        public async Task SetMaxDrawdown()
        {
            using var context = new PortfolioContext();
            var dbInstruments = context.Set<Instrument>();

            var instruments = await dbInstruments
                .Where(i => i.Prices.Any() && i.MaxDrawdown == null)
                .ToListAsync();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            int count = 0;
            await Parallel.ForEachAsync(instruments.Select(i => i.Id), parallelOptions, async (instrumentId, cancellationToken) =>
            {
                using var context = new PortfolioContext();
                var instrument = await GetInstrumentByIdAsync(instrumentId, true, context);

                try
                {
                    if (instrument.Prices == null || instrument.Prices.Count < 2)
                    {
                        return; // Skip instruments with insufficient price data
                    }

                    var maxDrawdown = CalculateMaxDrawdown(instrument.Prices);
                    if (maxDrawdown != null)
                    {
                        instrument.MaxDrawdown = maxDrawdown;
                        context.Update(instrument);
                        await context.SaveChangesAsync(cancellationToken);

                        Interlocked.Increment(ref count);
                        Console.WriteLine($"[{count} of {instruments.Count}] Instrument: {instrument.Name}, Max Drawdown: {maxDrawdown:F4}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calculating max drawdown for instrument {instrument.Name}: {ex.Message}");
                }
            });
        }

        public async Task SetSharpeRatio()
        {
            using var context = new PortfolioContext();
            var dbInstruments = context.Set<Instrument>();

            var instruments = await dbInstruments
                .Where(i => i.Prices.Any())
                //.Where(i => i.ISIN == "GB00B56B1J72")
                .ToListAsync();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 1 // Environment.ProcessorCount
            };

            //int count = 0;
            await Parallel.ForEachAsync(instruments.Select(i => i.Id), parallelOptions, async (instrumentId, cancellationToken) =>
            {
                using var context = new PortfolioContext();
                var instrument = await GetInstrumentByIdAsync(instrumentId, true, context);

                try
                {
                    var prices = instrument.Prices
                        .Where(p => p.Date.ToDateTime(TimeOnly.MinValue) >= DateTime.Now.AddYears(-6))
                        .OrderByDescending(p => p.Date)
                        .ToArray();

                    var growth = (1 + (prices.First().Value - prices.Last().Value) / prices.Last().Value);
                    var years = (prices.First().Date.ToDateTime(TimeOnly.MinValue) - prices.Last().Date.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25;

                    if(years > 3.0)
                    {
                        var annualReturn = Math.Pow(growth, 1 / years) - 1.0;

                        var priceValues = prices.Select(p => p.Value).ToArray();
                        var standardDeviation = ArrayStatistics.StandardDeviation(priceValues);

                        var sharpeRatio = (annualReturn - Constants.RiskFreeRate) / standardDeviation;

                        instrument.SharpeRatio = sharpeRatio;

                        await context.SaveChangesAsync();
                        Console.WriteLine($"Saving Sharpe ratio for {instrument.Name} = {sharpeRatio}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calculating max drawdown for instrument {instrument.Name}: {ex.Message}");
                }
            });
        }


        private static double? CalculateMaxDrawdown(ICollection<Price> prices)
        {
            var pricesList 
                = prices.Where(p => p.Date >= DateOnly.FromDateTime(DateTime.Now.AddYears(-6)))
                        .OrderBy(p => p.Date)
                        .ToList();

            if (pricesList?.Count() < 2)
            {
                return null;
            }
            double maxDrawdown = 0.0;
            double peak = pricesList[0].Value;
            foreach (var price in pricesList)
            {
                if (price.Value > peak)
                {
                    peak = price.Value;
                }
                else
                {
                    double drawdown = (peak - price.Value) / peak;
                    if (drawdown > maxDrawdown)
                    {
                        maxDrawdown = drawdown;
                    }
                }
            }
            return maxDrawdown;
        }

        private static async Task<List<Price>> GetMonthlyPrices(int instrumentId)
        {
            using var context = new PortfolioContext();

            Instrument instrument = await GetInstrumentByIdAsync(instrumentId, true, context);

            var firstPriceofEachMonth = instrument.Prices
                .Where(p => p.Date >= System.DateOnly.FromDateTime(DateTime.Now.AddYears(-5)))
                .GroupBy(p => new { p.Date.Year, p.Date.Month })
                .Select(g => g.FirstOrDefault())
                .OrderBy(p => p.Date)
                .ToList();

            return firstPriceofEachMonth;
        }

        private async Task<double?> GetCorrelation(List<Price> acwiPrices, List<Price> instrumentPrices, int instrumentId)
        {
            using var context = new PortfolioContext();

            var acwiMonthlyPrices = acwiPrices
                .Select(p => new { p.Date.Year, p.Date.Month, p.Value })
                .ToList();

            var instrumentMonthlyPrices = instrumentPrices
                .Select(p => new { p.Date.Year, p.Date.Month, p.Value })
                .Where(i => i.Value != 0.0)
                .ToList();

            var joinedPrices = acwiMonthlyPrices.Join(
                instrumentMonthlyPrices,
                acwi => new { acwi.Year, acwi.Month },
                instrument => new { instrument.Year, instrument.Month },
                (acwi, instrument) => new { Acwi = acwi.Value, Instrument = instrument.Value }
            ).ToArray();

            var deltas = Enumerable
                .Range(0, joinedPrices.Length - 1)
                .Select(i => new
                {
                    AcwiDelta = (joinedPrices[i + 1].Acwi - joinedPrices[i].Acwi) / joinedPrices[i].Acwi,
                    InstrumentDelta = (joinedPrices[i + 1].Instrument - joinedPrices[i].Instrument) / joinedPrices[i].Instrument
                })
                .ToList();

            var acwiDeltas = deltas.Select(d => d.AcwiDelta).ToArray();
            var instrumentDeltas = deltas.Select(d => d.InstrumentDelta).ToArray();

            var correlation = MathNet.Numerics.Statistics.Correlation.Pearson(acwiDeltas, instrumentDeltas);

            if (!double.IsNaN(correlation))
            {
                try
                {
                    Instrument instrument = await GetInstrumentByIdAsync(instrumentId, false, context);

                    instrument.Correlation = correlation;
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving correlation for instrument {instrumentId}: {ex.Message}");
                }
                return correlation;
            }

            return null;
        }

        private static async Task<Instrument> GetInstrumentByIdAsync(int instrumentId, bool withPrices, PortfolioContext context)
        {
            if (withPrices)
            {
                return await (context.Instrument
                    .Include(i => i.Prices)
                    .FirstOrDefaultAsync(i => i.Id == instrumentId));
            }
            else
            {
                return await context.Instrument
                    .FirstOrDefaultAsync(i => i.Id == instrumentId);
            }
        }
    }
}
