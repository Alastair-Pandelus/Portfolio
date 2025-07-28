namespace Portfolio.EntityModel
{
    public class Portfolio
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<PortfolioHolding> Holdings { get; set; } = new List<PortfolioHolding>();
    }
}
