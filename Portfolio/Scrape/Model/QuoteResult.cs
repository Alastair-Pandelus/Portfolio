namespace Portfolio.Scrape.Model
{
    public class Quote
    {
        public string exchange { get; set; }
        public string shortname { get; set; }
        public string quoteType { get; set; }
        public string symbol { get; set; }
        public string index { get; set; }
        public double score { get; set; }
        public string typeDisp { get; set; }
        public string longname { get; set; }
        public string exchDisp { get; set; }
        public bool isYahooFinance { get; set; }
    }

    public class QuoteResult
    {
        public List<object> explains { get; set; }
        public int count { get; set; }
        public List<Quote> quotes { get; set; }
        public List<object> news { get; set; }
        public List<object> nav { get; set; }
        public List<object> lists { get; set; }
        public List<object> researchReports { get; set; }
        public List<object> screenerFieldResults { get; set; }
        public int totalTime { get; set; }
        public int timeTakenForQuotes { get; set; }
        public int timeTakenForNews { get; set; }
        public int timeTakenForAlgowatchlist { get; set; }
        public int timeTakenForPredefinedScreener { get; set; }
        public int timeTakenForCrunchbase { get; set; }
        public int timeTakenForNav { get; set; }
        public int timeTakenForResearchReports { get; set; }
        public int timeTakenForScreenerField { get; set; }
        public int timeTakenForCulturalAssets { get; set; }
        public int timeTakenForSearchLists { get; set; }
    }
}
