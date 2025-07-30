namespace Portfolio.EntityModel
{
    // https://pyportfolioopt.readthedocs.io/en/latest/ExpectedReturns.html
    public class AdjustedReturn
    {
        public int Id { get; set; }

        // The logarithmic difference between today's price and yesterday's price.
        // Storing as log has advantage of being additive
        public double LogValue { get; set; }

        public System.DateOnly Date { get; set; }
        public int InstrumentId { get; set; }
        public Instrument Instrument { get; set; }
    }
}
