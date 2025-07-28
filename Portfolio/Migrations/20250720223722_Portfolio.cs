using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class Portfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Correlation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Instrument1Id = table.Column<int>(type: "int", nullable: false),
                    Instrument2Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Correlation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DateRange",
                columns: table => new
                {
                    Value = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Instrument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentType = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sedol = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ISIN = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExchangeCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MarketCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Y_Symbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    ReturnM0 = table.Column<double>(type: "float", nullable: false),
                    OngoingCharge = table.Column<double>(type: "float", nullable: false),
                    StarRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RatingM36 = table.Column<int>(type: "int", nullable: false),
                    ReturnM1 = table.Column<double>(type: "float", nullable: false),
                    ReturnM3 = table.Column<double>(type: "float", nullable: false),
                    ReturnM6 = table.Column<double>(type: "float", nullable: false),
                    ReturnM12 = table.Column<double>(type: "float", nullable: false),
                    ReturnM36 = table.Column<double>(type: "float", nullable: false),
                    ClosePriceChange = table.Column<double>(type: "float", nullable: false),
                    Yield_M12 = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    ClosePrice = table.Column<double>(type: "float", nullable: false),
                    Medalist_RatingNumber = table.Column<int>(type: "int", nullable: false),
                    ReturnW1 = table.Column<double>(type: "float", nullable: false),
                    InvestmentTypeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExchangeTradedShare = table.Column<bool>(type: "bit", nullable: false),
                    LegalStructureName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalStructureId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YR_ReturnM12_1 = table.Column<double>(type: "float", nullable: false),
                    YR_ReturnM12_2 = table.Column<double>(type: "float", nullable: false),
                    YR_ReturnM12_3 = table.Column<double>(type: "float", nullable: false),
                    Distribution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PERatio = table.Column<double>(type: "float", nullable: false),
                    PriceCurrencyId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SustainabilityRating = table.Column<int>(type: "int", nullable: true),
                    CustomBuyFee = table.Column<double>(type: "float", nullable: true),
                    ReturnM60 = table.Column<double>(type: "float", nullable: true),
                    IntradayBid = table.Column<double>(type: "float", nullable: true),
                    IntradayAsk = table.Column<double>(type: "float", nullable: true),
                    YR_ReturnM12_4 = table.Column<double>(type: "float", nullable: true),
                    YR_ReturnM12_5 = table.Column<double>(type: "float", nullable: true),
                    ReturnM120 = table.Column<double>(type: "float", nullable: true),
                    IMASectorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IMASectorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockLong = table.Column<double>(type: "float", nullable: true),
                    StockShort = table.Column<double>(type: "float", nullable: true),
                    BondLong = table.Column<double>(type: "float", nullable: true),
                    BondShort = table.Column<double>(type: "float", nullable: true),
                    CashLong = table.Column<double>(type: "float", nullable: true),
                    CashShort = table.Column<double>(type: "float", nullable: true),
                    OtherLong = table.Column<double>(type: "float", nullable: true),
                    OtherShort = table.Column<double>(type: "float", nullable: true),
                    RatioUnitedStates = table.Column<double>(type: "float", nullable: true),
                    RatioTechnology = table.Column<double>(type: "float", nullable: true),
                    EquityHoldings = table.Column<int>(type: "int", nullable: true),
                    BondHoldings = table.Column<int>(type: "int", nullable: true),
                    RiskRating = table.Column<int>(type: "int", nullable: true),
                    Correlation = table.Column<double>(type: "float", nullable: true),
                    MaxDrawdown = table.Column<double>(type: "float", nullable: true),
                    Watchlist = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Portfolio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Price",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Price", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Price_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioHolding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    PortfolioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioHolding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioHolding_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioHolding_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_InstrumentType_Sedol_ISIN_ExchangeCode_Currency",
                table: "Instrument",
                columns: new[] { "InstrumentType", "Sedol", "ISIN", "ExchangeCode", "Currency" },
                unique: true,
                filter: "[InstrumentType] IS NOT NULL AND [Sedol] IS NOT NULL AND [ISIN] IS NOT NULL AND [ExchangeCode] IS NOT NULL AND [Currency] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioHolding_InstrumentId",
                table: "PortfolioHolding",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioHolding_PortfolioId",
                table: "PortfolioHolding",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Price_InstrumentId",
                table: "Price",
                column: "InstrumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Correlation");

            migrationBuilder.DropTable(
                name: "DateRange");

            migrationBuilder.DropTable(
                name: "PortfolioHolding");

            migrationBuilder.DropTable(
                name: "Price");

            migrationBuilder.DropTable(
                name: "Portfolio");

            migrationBuilder.DropTable(
                name: "Instrument");
        }
    }
}
