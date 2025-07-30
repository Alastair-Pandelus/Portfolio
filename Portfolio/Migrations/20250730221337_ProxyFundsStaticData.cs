using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class ProxyFundsStaticData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Ranmore Institutional = 100% Ranmore Investor */
            migrationBuilder.Sql(@"
declare @ranmoreGlobalEquityInstitutionalGBP int
select @ranmoreGlobalEquityInstitutionalGBP = id from dbo.Instrument where [Name]='Ranmore Global Equity Institutional GBP'

declare @ranmoreGlobalEquityInvestorGBP int
select @ranmoreGlobalEquityInvestorGBP = id from dbo.Instrument where [Name]='Ranmore Global Equity Investor GBP'

INSERT INTO [ProxyInstrument] ([InstrumentId],[ProxyId] ,[Weight])
     VALUES (@ranmoreGlobalEquityInstitutionalGBP, @ranmoreGlobalEquityInvestorGBP, 1.0)");

            /* Winton Trend Enhanced = 100% Winton Trend + 100% MSCI World */
            migrationBuilder.Sql(@"
declare @wintonTrendEnhGlblEqUCITSIGBPUnHAcc int
select @wintonTrendEnhGlblEqUCITSIGBPUnHAcc = id from dbo.Instrument where [Name]='Winton Trend Enh Glbl Eq UCITSIGBPUnHAcc'

declare @wintonTrendUCITSIGBPAcc int
select @wintonTrendUCITSIGBPAcc = id from dbo.Instrument where [Name]='Winton Trend UCITS I GBP Acc'

declare @msciWorld int
select @msciWorld = id from dbo.Instrument where [ISIN]='IE00BD4TXV59'

INSERT INTO [ProxyInstrument] ([InstrumentId],[ProxyId] ,[Weight])
     VALUES (@wintonTrendEnhGlblEqUCITSIGBPUnHAcc, @wintonTrendUCITSIGBPAcc, 1.0)
INSERT INTO [ProxyInstrument] ([InstrumentId],[ProxyId] ,[Weight])
     VALUES (@wintonTrendEnhGlblEqUCITSIGBPUnHAcc, @msciWorld, 1.0)");

            /* WisdomTree Global Efficient Core = 90% World Equity + 60% Government Bond */
            migrationBuilder.Sql(@"
declare @wisdomTreeGlbEfficientCorETFUSDAccGBP int
select @wisdomTreeGlbEfficientCorETFUSDAccGBP = id from dbo.Instrument where [Name]='WisdomTree Glb Efficient Cor ETFUSDAcc GBP'

declare @msciWorld int
select @msciWorld = id from dbo.Instrument where [ISIN]='IE00BD4TXV59'

declare @globalGovernmentBondFund int
select @globalGovernmentBondFund = id from dbo.Instrument where [ISIN]='GB00BR560P63'

INSERT INTO [ProxyInstrument] ([InstrumentId],[ProxyId] ,[Weight])
     VALUES (@wisdomTreeGlbEfficientCorETFUSDAccGBP, @msciWorld, 0.9)
INSERT INTO [ProxyInstrument] ([InstrumentId],[ProxyId] ,[Weight])
     VALUES (@wisdomTreeGlbEfficientCorETFUSDAccGBP, @globalGovernmentBondFund, 0.6)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from ProxyInstrument");
        }
    }
}
