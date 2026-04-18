//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

using network_project.Api;

// ════════════════════════════════════════════════════════════════════════════
//  Quantum-Inspired Cybersecurity Analysis System — Demo Entry Point
// ════════════════════════════════════════════════════════════════════════════

Console.WriteLine("══════════════════════════════════════════════════════");
Console.WriteLine("  Quantum-Inspired Cybersecurity Analysis System");
Console.WriteLine("══════════════════════════════════════════════════════\n");

var controller = new AnalysisController();

// ── Step 1: POST /upload ──────────────────────────────────────────────────
string csv = """
source_ip,dest_ip,protocol,packet_size,timestamp
192.168.1.1,10.0.0.5,TCP,512,2024-01-10 08:00:00
192.168.1.1,10.0.0.5,TCP,512,2024-01-10 08:05:00
192.168.1.1,10.0.0.5,TCP,512,2024-01-10 08:10:00
192.168.1.1,10.0.0.5,TCP,512,2024-01-10 08:15:00
192.168.1.1,10.0.0.5,TCP,512,2024-01-10 08:20:00
10.0.0.2,192.168.1.1,UDP,256,2024-01-10 08:01:00
10.0.0.3,192.168.1.1,TCP,1024,2024-01-10 08:03:00
172.16.0.10,10.0.0.5,ICMP,64,2024-01-10 08:07:00
172.16.0.10,192.168.1.1,TCP,2048,2024-01-10 08:08:00
172.16.0.10,10.0.0.2,UDP,128,2024-01-10 08:09:00
172.16.0.10,10.0.0.3,TCP,512,2024-01-10 08:11:00
10.0.0.5,172.16.0.10,TCP,512,2024-01-10 08:12:00
192.168.1.1,10.0.0.5,TCP,512,2024-01-10 08:05:00
invalid_ip,10.0.0.5,TCP,512,2024-01-10 08:06:00
192.168.1.2,10.0.0.5,TCP,-1,2024-01-10 08:13:00
""";

Console.WriteLine("▶ POST /upload");
var uploadResp = controller.Upload(csv);
Console.WriteLine($"  Status : {(uploadResp.Success ? "OK" : "FAIL")}");
Console.WriteLine($"  Message: {uploadResp.Message}");
if (uploadResp.Data is { } report)
{
    Console.WriteLine($"  Rows inserted     : {report.RowsInserted}");
    Console.WriteLine($"  Skipped (invalid) : {report.SkippedRows}");
    Console.WriteLine($"  Duplicates removed: {report.DuplicatesRemoved}");
    if (report.Errors.Any())
    {
        Console.WriteLine("  Validation notes:");
        foreach (var e in report.Errors) Console.WriteLine($"    • {e}");
    }
}

Console.WriteLine();

// ── Step 2: POST /analyze ─────────────────────────────────────────────────
Console.WriteLine("▶ POST /analyze");
var analyzeResp = controller.Analyze(walkSteps: 25, walkDt: 0.1, qftBucketMins: 5);
Console.WriteLine($"  Status : {(analyzeResp.Success ? "OK" : "FAIL")}");
Console.WriteLine($"  Message: {analyzeResp.Message}");
if (analyzeResp.Data is { } summary)
{
    Console.WriteLine($"  Nodes  : {summary.TotalNodes}   Edges: {summary.TotalEdges}");
    Console.WriteLine($"  HIGH   : {summary.HighRiskCount}");
    Console.WriteLine($"  MEDIUM : {summary.MedRiskCount}");
    Console.WriteLine($"  LOW    : {summary.LowRiskCount}");
    Console.WriteLine("\n  Top Threats:");
    foreach (var t in summary.TopThreats)
        Console.WriteLine($"    [{t.ThreatLevel,-6}] {t.IpAddress,-16} " +
                          $"score={t.TreadScore:F4}  confidence={t.Confidence:F4}  " +
                          $"walk={t.QuantumWalkScore:F4}  qft={t.QFTScore:F4}");
}

Console.WriteLine();

// ── Step 3: GET /results ──────────────────────────────────────────────────
Console.WriteLine("▶ GET /results");
var resultsResp = controller.GetResults();
Console.WriteLine($"  Status : {(resultsResp.Success ? "OK" : "FAIL")}");
Console.WriteLine($"  Message: {resultsResp.Message}");
if (resultsResp.Data is { } results)
{
    Console.WriteLine("\n  ┌──────────────────┬────────┬────────┬────────┬────────┐");
    Console.WriteLine("  │ IP Address       │ Level  │ Score  │ Walk   │ QFT    │");
    Console.WriteLine("  ├──────────────────┼────────┼────────┼────────┼────────┤");
    foreach (var r in results)
        Console.WriteLine($"  │ {r.IpAddress,-16} │ {r.ThreatLevel,-6} │ {r.TreadScore:F4} │ {r.QuantumWalkScore:F4} │ {r.QFTScore:F4} │");
    Console.WriteLine("  └──────────────────┴────────┴────────┴────────┴────────┘");
}

Console.WriteLine("\n══════════════════════════════════════════════════════");
Console.WriteLine("  Done.");
Console.WriteLine("══════════════════════════════════════════════════════");
