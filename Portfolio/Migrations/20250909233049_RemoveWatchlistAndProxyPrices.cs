using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWatchlistAndProxyPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProxyInstrument");

            migrationBuilder.DropColumn(
                name: "Watchlist",
                table: "Instrument");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Watchlist",
                table: "Instrument",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ProxyInstrument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    InstrumentId1 = table.Column<int>(type: "int", nullable: true),
                    ProxyId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyInstrument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProxyInstrument_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProxyInstrument_Instrument_InstrumentId1",
                        column: x => x.InstrumentId1,
                        principalTable: "Instrument",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProxyInstrument_InstrumentId",
                table: "ProxyInstrument",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProxyInstrument_InstrumentId1",
                table: "ProxyInstrument",
                column: "InstrumentId1");
        }
    }
}
