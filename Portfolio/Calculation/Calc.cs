using Microsoft.EntityFrameworkCore;
using Portfolio.EntityModel;

namespace Portfolio.Calculation
{
    public interface ICalc
    {
        public Task SynthesiseMissingPrices();
        public Task SetMaxDrawdown();
        public Task SetMarketCorrelations();
        //public Task GetCorrelations();
    }

    public class Calc : ICalc
    {
        public async Task SynthesiseMissingPrices()
        {
            using var context = new PortfolioContext();

            var dateRange = await context.Set<EntityModel.PriceDate>().FromSqlRaw
                (@"select distinct([Date]) from dbo.Price order by [Date] desc").ToListAsync();

            var instruments = await context.Set<Instrument>()
                .ToListAsync();

            int i = 0;
            foreach(var instrument in instruments)
            {
                int count = 0;

                var instrumentPrices = await context.Set<Price>()
                    .Where(p => p.InstrumentId == instrument.Id)
                    .ToListAsync();

                if(instrumentPrices.Count < 2)
                {
                    Console.WriteLine($"[{++i} of {instruments.Count}] - Instrument {instrument.Name} has less than 2 prices, skipping.");
                    continue;
                }

                var earliestPriceDate = instrumentPrices
                    .Select(p => p.Date)
                    .Min();

                var latestPriceDate = instrumentPrices
                    .Select(p => p.Date)
                    .Max();

                var instrumentPriceDates = instrumentPrices
                    .Select(p => p.Date)
                    .ToList();

                var missingDates = dateRange.Select(d => d.Value)
                    .Where(d => d >= earliestPriceDate && d <= latestPriceDate && !instrumentPriceDates.Contains(d))
                    .ToList();

                foreach(var missingDate in missingDates)
                {
                    var nearestPrice = instrumentPrices
                        .Where(p => p.Date < missingDate)
                        .OrderByDescending(p => p.Date)
                        .FirstOrDefault();

                    var newPrice = new Price
                    {
                        InstrumentId = instrument.Id,
                        Date = missingDate,
                        Value = nearestPrice.Value  
                    };
                    context.Set<Price>().Add(newPrice);
                    count++;
                }

                context.SaveChanges();
                Console.WriteLine($"[{++i} of {instruments.Count}] - Synthesised {count} missing prices for instrument: {instrument.Name}");
            }
        }

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
                                    where Symbol = 'ACWI' or Correlation is null
                                group by InstrumentId
                        ) InstrumentMonths
                        where InstrumentMonths.Months > 36 and MaxMonth = YEAR(GETDATE())*12+MONTH(GETDATE())
                    )"
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

        //public async Task GetCorrelations()
        //{
        //    using var context = new PortfolioContext();
        //    var dbInstruments = context.Set<Instrument>();
        //    var dbCorrelations = context.Set<Correlation>();

        //    var instruments = await dbInstruments.ToListAsync();
        //    var instrument5YearDeltas = 
        //        await context.InstrumentMonthlyDelta
        //            .Where(d => d.Month > -60)
        //            .ToListAsync();

        //    var instrumentIds = instrument5YearDeltas
        //        .Select(i => i.InstrumentId)
        //        .Distinct()
        //        .ToArray();

        //    var existingCorrelations = await dbCorrelations
        //        .Where(c => instrumentIds.Contains(c.Instrument1Id) && instrumentIds.Contains(c.Instrument2Id))
        //        .ToDictionaryAsync(c => c.Instrument1Id, c => c.Instrument2Id);

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
        //    var maxCount = ((instrumentDeltas.Count * instrumentDeltas.Count) / 2) - 1 - existingCorrelations.Count;
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

        //        for (int j=i+1; j < instrumentDeltas.Count; j++)
        //        {
        //            var id2 = instrumentIds[j];

        //            if (existingCorrelations.Any(c => (c.Instrument1Id == id1 && c.Instrument2Id == id2)))
        //            {
        //                Console.WriteLine($"[{++count} of ({maxCount}] - Skipping existing correlation between {name1} and {instruments.FirstOrDefault(i => i.Id == id2).Name}");
        //                continue; // Skip if correlation already exists
        //            }

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

        //                correlation = MathNet.Numerics.Statistics.Correlation.Pearson(
        //                    deltas1.Take(minLength).ToArray(),
        //                    deltas2.Take(minLength).ToArray());
        //            }

        //            string msg;
        //            if (double.IsNaN(correlation))
        //            {
        //                msg = $"[{++count} of ({maxCount}] - Unable to calculate Correlation between {name1} and {name2}: {correlation:F4}";
        //            }
        //            else
        //            {
        //                dbCorrelations.Add(new Correlation
        //                {
        //                    Instrument1Id = id1,
        //                    Instrument2Id = id2,
        //                    Value = correlation
        //                });
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

        private static double? CalculateMaxDrawdown(List<Price> prices)
        {
            prices = prices.Where(p => p.Date >= System.DateOnly.FromDateTime(DateTime.Now.AddYears(-5)))
                           .OrderBy(p => p.Date)
                           .ToList();

            if (prices == null || prices.Count < 2)
            {
                return null;
            }
            double maxDrawdown = 0.0;
            double peak = prices[0].Value;
            foreach (var price in prices)
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
