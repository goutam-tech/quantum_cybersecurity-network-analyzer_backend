using Microsoft.EntityFrameworkCore;
using QuantumCyberAnalyzer.Models;

namespace QuantumCyberAnalyzer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<NetworkLog>      NetworkLogs       { get; set; }
    public DbSet<Node>            Nodes             { get; set; }
    public DbSet<Edge>            Edges             { get; set; }
    public DbSet<QuantumWalkResult> QuantumWalkResults { get; set; }
    public DbSet<QftResult>       QftResults        { get; set; }
    public DbSet<DetectionResult> DetectionResults  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // NetworkLog
        modelBuilder.Entity<NetworkLog>(e =>
        {
            e.HasKey(x => x.LogId);
            e.Property(x => x.SourceIp).HasMaxLength(50);
            e.Property(x => x.DestIp).HasMaxLength(50);
            e.Property(x => x.Protocol).HasMaxLength(20);
        });

        // Node – unique IP
        modelBuilder.Entity<Node>(e =>
        {
            e.HasKey(x => x.NodeId);
            e.HasIndex(x => x.IpAddress).IsUnique();
            e.Property(x => x.IpAddress).HasMaxLength(50);
        });

        // Edge
        modelBuilder.Entity<Edge>(e =>
        {
            e.HasKey(x => x.EdgeId);
            e.HasIndex(x => new { x.SourceIp, x.DestIp }).IsUnique();
        });

        // QuantumWalkResult → Node
        modelBuilder.Entity<QuantumWalkResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Node)
             .WithMany(n => n.QuantumWalkResults)
             .HasForeignKey(x => x.NodeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // QftResult → Node
        modelBuilder.Entity<QftResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Node)
             .WithMany(n => n.QftResults)
             .HasForeignKey(x => x.NodeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // DetectionResult → Node
        modelBuilder.Entity<DetectionResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ThreatLevel).HasMaxLength(20);
            e.HasOne(x => x.Node)
             .WithMany(n => n.DetectionResults)
             .HasForeignKey(x => x.NodeId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
