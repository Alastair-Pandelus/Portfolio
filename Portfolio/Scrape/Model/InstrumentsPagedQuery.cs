namespace Portfolio.Scrape.Model
{
    public class InstrumentsPagedQuery
    {
        public InstrumentsPagedQuery(InstrumentType fundType, int currentPage, int rowsPerPage)
        {
            this.rowsPerPage = rowsPerPage;
            this.currentPage = currentPage;
            screenerType = fundType.ToString();
        }

        public int currentPage { init; get; }

        public string screenerType { init; get; }

        public int rowsPerPage { init; get; }
    }
}
