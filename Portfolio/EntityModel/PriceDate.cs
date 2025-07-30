using Microsoft.EntityFrameworkCore;

namespace Portfolio.EntityModel
{
    // Psuedo entity to represent a DateOnly type in EF Core, used for date ranges of available prices
    // TODO - better solution?
    [Keyless]
    public class PriceDate
    {
        public DateOnly Value { get; set; }
    }
}
