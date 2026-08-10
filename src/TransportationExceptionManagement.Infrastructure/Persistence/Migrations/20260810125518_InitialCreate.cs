using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportationExceptionManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransportationExceptionCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CaseReference = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    MovementReference = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    OriginNode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DestinationNode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CarrierCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ExceptionType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Assignee = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    DueAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    ResolvedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    ResolutionSummary = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportationExceptionCases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransportationExceptionCaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseNotes_TransportationExceptionCases_TransportationExceptionCaseId",
                        column: x => x.TransportationExceptionCaseId,
                        principalTable: "TransportationExceptionCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseNotes_TransportationExceptionCaseId_CreatedAtUtc",
                table: "CaseNotes",
                columns: new[] { "TransportationExceptionCaseId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TransportationExceptionCases_Assignee",
                table: "TransportationExceptionCases",
                column: "Assignee");

            migrationBuilder.CreateIndex(
                name: "IX_TransportationExceptionCases_CaseReference",
                table: "TransportationExceptionCases",
                column: "CaseReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportationExceptionCases_CreatedAtUtc",
                table: "TransportationExceptionCases",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TransportationExceptionCases_DueAtUtc",
                table: "TransportationExceptionCases",
                column: "DueAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TransportationExceptionCases_ExceptionType",
                table: "TransportationExceptionCases",
                column: "ExceptionType");

            migrationBuilder.CreateIndex(
                name: "IX_TransportationExceptionCases_Severity",
                table: "TransportationExceptionCases",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_TransportationExceptionCases_Status",
                table: "TransportationExceptionCases",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseNotes");

            migrationBuilder.DropTable(
                name: "TransportationExceptionCases");
        }
    }
}
