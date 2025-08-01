using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class ProxyInstrumentFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProxyInstrument_Instrument_InstrumentId",
                table: "ProxyInstrument");

            migrationBuilder.DropForeignKey(
                name: "FK_ProxyInstrument_Instrument_ProxyId",
                table: "ProxyInstrument");

            migrationBuilder.DropIndex(
                name: "IX_ProxyInstrument_ProxyId",
                table: "ProxyInstrument");

            migrationBuilder.AddColumn<int>(
                name: "InstrumentId1",
                table: "ProxyInstrument",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProxyInstrument_InstrumentId1",
                table: "ProxyInstrument",
                column: "InstrumentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProxyInstrument_Instrument_InstrumentId",
                table: "ProxyInstrument",
                column: "InstrumentId",
                principalTable: "Instrument",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProxyInstrument_Instrument_InstrumentId1",
                table: "ProxyInstrument",
                column: "InstrumentId1",
                principalTable: "Instrument",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProxyInstrument_Instrument_InstrumentId",
                table: "ProxyInstrument");

            migrationBuilder.DropForeignKey(
                name: "FK_ProxyInstrument_Instrument_InstrumentId1",
                table: "ProxyInstrument");

            migrationBuilder.DropIndex(
                name: "IX_ProxyInstrument_InstrumentId1",
                table: "ProxyInstrument");

            migrationBuilder.DropColumn(
                name: "InstrumentId1",
                table: "ProxyInstrument");

            migrationBuilder.CreateIndex(
                name: "IX_ProxyInstrument_ProxyId",
                table: "ProxyInstrument",
                column: "ProxyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProxyInstrument_Instrument_InstrumentId",
                table: "ProxyInstrument",
                column: "InstrumentId",
                principalTable: "Instrument",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProxyInstrument_Instrument_ProxyId",
                table: "ProxyInstrument",
                column: "ProxyId",
                principalTable: "Instrument",
                principalColumn: "Id");
        }
    }
}
