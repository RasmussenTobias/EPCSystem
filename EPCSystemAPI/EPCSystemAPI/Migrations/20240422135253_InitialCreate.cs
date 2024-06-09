using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPCSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Devices__Users_UserId",
                        column: x => x.UserId,
                        principalTable: "_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_ElectricityProduction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountWh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ElectricityProduction", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ElectricityProduction__Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "_Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProductionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId1 = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    UserId2 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Certificates__ElectricityProduction_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "_ElectricityProduction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__Certificates__ElectricityProduction_ProductionId",
                        column: x => x.ProductionId,
                        principalTable: "_ElectricityProduction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Certificates__Users_UserId",
                        column: x => x.UserId,
                        principalTable: "_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Certificates__Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_Ledger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificateId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ElectricityProductionId = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Volume = table.Column<int>(type: "int", nullable: true),
                    CertificateId1 = table.Column<int>(type: "int", nullable: false),
                    ElectricityProductionId1 = table.Column<int>(type: "int", nullable: false),
                    CertificateId2 = table.Column<int>(type: "int", nullable: false),
                    ElectricityProductionId2 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ledger", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Ledger__Certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "_Certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Ledger__Certificates_CertificateId1",
                        column: x => x.CertificateId1,
                        principalTable: "_Certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Ledger__ElectricityProduction_ElectricityProductionId",
                        column: x => x.ElectricityProductionId,
                        principalTable: "_ElectricityProduction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Ledger__ElectricityProduction_ElectricityProductionId1",
                        column: x => x.ElectricityProductionId1,
                        principalTable: "_ElectricityProduction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_ProduceEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    ElectricityProductionId = table.Column<int>(type: "int", nullable: false),
                    AmountWh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ProduceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ProduceEvents__Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "_Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__ProduceEvents__ElectricityProduction_ElectricityProductionId",
                        column: x => x.ElectricityProductionId,
                        principalTable: "_ElectricityProduction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__ProduceEvents__Ledger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "_Ledger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_TransferEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    CertificateId = table.Column<int>(type: "int", nullable: false),
                    FromUserId = table.Column<int>(type: "int", nullable: false),
                    ToUserId = table.Column<int>(type: "int", nullable: false),
                    Volume = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TransferEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK__TransferEvents__Ledger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "_Ledger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__TransferEvents__Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__TransferEvents__Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "_TransformEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    ElectricityProductionId = table.Column<int>(type: "int", nullable: false),
                    AmountWh = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TransformEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK__TransformEvents__Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "_Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__TransformEvents__ElectricityProduction_ElectricityProductionId",
                        column: x => x.ElectricityProductionId,
                        principalTable: "_ElectricityProduction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__TransformEvents__Ledger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "_Ledger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX__Certificates_CurrencyId",
                table: "_Certificates",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX__Certificates_ProductionId",
                table: "_Certificates",
                column: "ProductionId");

            migrationBuilder.CreateIndex(
                name: "IX__Certificates_UserId",
                table: "_Certificates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX__Certificates_UserId1",
                table: "_Certificates",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX__Devices_UserId",
                table: "_Devices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX__ElectricityProduction_DeviceId",
                table: "_ElectricityProduction",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX__Ledger_CertificateId",
                table: "_Ledger",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX__Ledger_CertificateId1",
                table: "_Ledger",
                column: "CertificateId1");

            migrationBuilder.CreateIndex(
                name: "IX__Ledger_ElectricityProductionId",
                table: "_Ledger",
                column: "ElectricityProductionId");

            migrationBuilder.CreateIndex(
                name: "IX__Ledger_ElectricityProductionId1",
                table: "_Ledger",
                column: "ElectricityProductionId1");

            migrationBuilder.CreateIndex(
                name: "IX__ProduceEvents_DeviceId",
                table: "_ProduceEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX__ProduceEvents_ElectricityProductionId",
                table: "_ProduceEvents",
                column: "ElectricityProductionId");

            migrationBuilder.CreateIndex(
                name: "IX__ProduceEvents_LedgerId",
                table: "_ProduceEvents",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX__TransferEvents_FromUserId",
                table: "_TransferEvents",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX__TransferEvents_LedgerId",
                table: "_TransferEvents",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX__TransferEvents_ToUserId",
                table: "_TransferEvents",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX__TransformEvents_DeviceId",
                table: "_TransformEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX__TransformEvents_ElectricityProductionId",
                table: "_TransformEvents",
                column: "ElectricityProductionId");

            migrationBuilder.CreateIndex(
                name: "IX__TransformEvents_LedgerId",
                table: "_TransformEvents",
                column: "LedgerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_ProduceEvents");

            migrationBuilder.DropTable(
                name: "_TransferEvents");

            migrationBuilder.DropTable(
                name: "_TransformEvents");

            migrationBuilder.DropTable(
                name: "_Ledger");

            migrationBuilder.DropTable(
                name: "_Certificates");

            migrationBuilder.DropTable(
                name: "_ElectricityProduction");

            migrationBuilder.DropTable(
                name: "_Devices");

            migrationBuilder.DropTable(
                name: "_Users");
        }
    }
}
