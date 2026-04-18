using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace network_project.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Edges",
                columns: table => new
                {
                    EdgeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DestIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edges", x => x.EdgeId);
                });

            migrationBuilder.CreateTable(
                name: "NetworkLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DestIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Protocol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PacketSize = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkLogs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    NodeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TotalConnections = table.Column<int>(type: "integer", nullable: false),
                    AnomalyScore = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.NodeId);
                });

            migrationBuilder.CreateTable(
                name: "DetectionResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NodeId = table.Column<int>(type: "integer", nullable: false),
                    ThreatLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetectionResults_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QFTResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NodeId = table.Column<int>(type: "integer", nullable: false),
                    DominantFrequency = table.Column<double>(type: "double precision", nullable: false),
                    PeriodicityScore = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QFTResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QFTResults_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuantumWalkResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NodeId = table.Column<int>(type: "integer", nullable: false),
                    ProbabilityScore = table.Column<double>(type: "double precision", nullable: false),
                    AnomalyScore = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuantumWalkResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuantumWalkResults_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetectionResults_NodeId",
                table: "DetectionResults",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_SourceIp_DestIp",
                table: "Edges",
                columns: new[] { "SourceIp", "DestIp" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_IpAddress",
                table: "Nodes",
                column: "IpAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QFTResults_NodeId",
                table: "QFTResults",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_QuantumWalkResults_NodeId",
                table: "QuantumWalkResults",
                column: "NodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetectionResults");

            migrationBuilder.DropTable(
                name: "Edges");

            migrationBuilder.DropTable(
                name: "NetworkLogs");

            migrationBuilder.DropTable(
                name: "QFTResults");

            migrationBuilder.DropTable(
                name: "QuantumWalkResults");

            migrationBuilder.DropTable(
                name: "Nodes");
        }
    }
}
