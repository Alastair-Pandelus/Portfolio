using Portfolio.Calculation;
using Portfolio.Scrape;

public class Application
{
    //private readonly IConfiguration _configuration;
    private readonly IScraper _scraper;
    private readonly ICalc _calc;

    public Application(/*IConfiguration configuration,*/ IScraper scraper, ICalc calc)
    {
        //_configuration = configuration;
        _scraper = scraper;
        _calc = calc;
    }

    public async Task RunAsync()
    {
        //await _scraper.ScrapeInstruments();
        //await _scraper.CleanInstruments();
        //await _scraper.ScrapeMarketCodes();
        //await _scraper.ScrapePortfolio();
        //await _scraper.ScrapeYSymbols();
        //await _scraper.ScrapeRisk();
        //await _scraper.ScrapePrices();

        //await _calc.SynthesiseMissingPrices();
        await _calc.SetAdjustedReturns();
        //await _calc.SetMaxDrawdown();
        //await _calc.SetMarketCorrelations();
        //await _calc.GetCorrelations();

        //Console.WriteLine(await response.Content.ReadAsStringAsync());


        Console.ReadLine();
    }
}