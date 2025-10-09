using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APISegaAI.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestCases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Author = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Precondition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Postcondition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TestCode = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataPools",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TestCaseId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataPools_TestCases_TestCaseId",
                        column: x => x.TestCaseId,
                        principalTable: "TestCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HistoryEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    TestCaseId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    User = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryEntries_TestCases_TestCaseId",
                        column: x => x.TestCaseId,
                        principalTable: "TestCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InputData",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TestCaseId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputData_TestCases_TestCaseId",
                        column: x => x.TestCaseId,
                        principalTable: "TestCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TestSteps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    TestCaseId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ExpectedResult = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestSteps_TestCases_TestCaseId",
                        column: x => x.TestCaseId,
                        principalTable: "TestCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataPoolItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    DataPoolId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPoolItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataPoolItems_DataPools_DataPoolId",
                        column: x => x.DataPoolId,
                        principalTable: "DataPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataPoolItems_DataPoolId",
                table: "DataPoolItems",
                column: "DataPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_DataPools_TestCaseId",
                table: "DataPools",
                column: "TestCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEntries_TestCaseId",
                table: "HistoryEntries",
                column: "TestCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEntries_Timestamp",
                table: "HistoryEntries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_InputData_TestCaseId",
                table: "InputData",
                column: "TestCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_Number",
                table: "TestCases",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_Status",
                table: "TestCases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TestSteps_TestCaseId",
                table: "TestSteps",
                column: "TestCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataPoolItems");

            migrationBuilder.DropTable(
                name: "HistoryEntries");

            migrationBuilder.DropTable(
                name: "InputData");

            migrationBuilder.DropTable(
                name: "TestSteps");

            migrationBuilder.DropTable(
                name: "DataPools");

            migrationBuilder.DropTable(
                name: "TestCases");
        }
    }
}
