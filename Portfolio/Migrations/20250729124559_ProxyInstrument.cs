using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class ProxyInstrument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProxyInstrument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProxyInstrument_Instrument_ProxyId",
                        column: x => x.ProxyId,
                        principalTable: "Instrument",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProxyInstrument_InstrumentId",
                table: "ProxyInstrument",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProxyInstrument_ProxyId",
                table: "ProxyInstrument",
                column: "ProxyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProxyInstrument");
        }
    }
}
