namespace Portfolio.EntityModel
{
    public class PortfolioHolding
    {
        public int Id { get; set; }
        public int InstrumentId { get; set; }
        public Instrument Instrument { get; set; }
        public double Weight { get; set; }
    }
}
