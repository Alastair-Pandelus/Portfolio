namespace Portfolio.EntityModel
{
    public class Price
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public System.DateOnly Date { get; set; }
        public int InstrumentId { get; set; }
        public Instrument Instrument { get; set; }
    }
}
