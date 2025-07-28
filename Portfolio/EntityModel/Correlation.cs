namespace Portfolio.EntityModel
{
    public class Correlation
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public int Instrument1Id { get; set; }
        public int Instrument2Id { get; set; }
    }
}