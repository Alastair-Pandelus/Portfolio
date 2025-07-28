using Microsoft.EntityFrameworkCore;

namespace Portfolio.EntityModel;


[Index(nameof(InstrumentId), nameof(Year), IsUnique = true, Name = "InstrumentAnnualPerformance_InstrumentIdYear")]
public partial class InstrumentAnnualPerformance
{
    public int Id { get; set; }

    public int InstrumentId { get; set; }

    public int Year { get; set; }

    public double Value { get; set; }
}
