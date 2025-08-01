using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class MoreProxyFunds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* World Ex US -> Europe */
            migrationBuilder.Sql(@"
declare @xtrackersMSCIWorldexUSA1CGBP int
select @xtrackersMSCIWorldexUSA1CGBP = id from dbo.Instrument where [Name]='Xtrackers MSCI World ex USA 1C GBP'

declare @vanguardEurope int
select @vanguardEurope = id from dbo.Instrument where ISIN='IE00BK5BQX27'

INSERT INTO [ProxyInstrument] ([InstrumentId],[ProxyId] ,[Weight])
     VALUES (@xtrackersMSCIWorldexUSA1CGBP, @vanguardEurope, 1.0)
");

            /* Fix WidomTree Global Efficient Core to use longer priced bond fund */
            migrationBuilder.Sql(@"update dbo.ProxyInstrument 
    set ProxyId = (select Id from dbo.Instrument where ISIN='GB00B7Q0Q826')
where InstrumentId=3252 and ProxyId=5035");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
