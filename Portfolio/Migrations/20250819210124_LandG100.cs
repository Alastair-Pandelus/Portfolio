using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class LandG100 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* L&G 100 = 100% Nasdaq */
            migrationBuilder.Sql(@"
declare @landG100 int  
select @landG100 = id from dbo.Instrument where name = 'L&G Global 100 Index I Acc'
declare @nasdaq int  
select @nasdaq = id from dbo.Instrument where name = 'Invesco EQQQ NASDAQ-100 ETF GBP'
insert into [ProxyInstrument] ([InstrumentId],[ProxyId] ,[Weight])
	values(@landG100, @nasdaq, 1.0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
