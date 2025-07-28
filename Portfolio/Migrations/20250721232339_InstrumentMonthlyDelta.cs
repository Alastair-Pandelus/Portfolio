using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection.Emit;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class InstrumentMonthlyDelta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"create view InstrumentMonthlyDelta as

WITH first_price_per_month AS (
    SELECT
        InstrumentId,
        - (YEAR(GETDATE())*12+MONTH(GETDATE()) - (YEAR(date)*12+MONTH(date))) as Month, 
        MIN(date) AS first_date
    FROM Price
    GROUP BY InstrumentId, - (YEAR(GETDATE())*12+MONTH(GETDATE()) - (YEAR(date)*12+MONTH(date)))
),
monthly_prices AS (
    SELECT
        fpm.InstrumentId,
        fpm.month,
        s.Value AS first_price
    FROM first_price_per_month fpm
    JOIN Price s
      ON s.InstrumentId = fpm.InstrumentId
     AND s.date = fpm.first_date
),
price_with_lag AS (
    SELECT
        InstrumentId,
        month,
        first_price,
        LAG(first_price) OVER (PARTITION BY InstrumentId ORDER BY month) AS prev_month_first_price
    FROM monthly_prices
)
SELECT
    InstrumentId,
    month,
    (first_price - prev_month_first_price) / prev_month_first_price as Delta
FROM price_with_lag
where prev_month_first_price is not null
--ORDER BY InstrumentId, month;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP view InstrumentMonthlyDelta");
        }
    }
}
