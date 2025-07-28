using Microsoft.EntityFrameworkCore;

namespace Portfolio.EntityModel
{
    [Keyless]
    public class InstrumentMonthlyDelta
    {
        public int InstrumentId { get; set; }

        public int Month { get; set; }

        public double Delta { get; set; }
    }
}
