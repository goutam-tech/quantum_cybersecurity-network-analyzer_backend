using Microsoft.EntityFrameworkCore;
using network_project.Models;

namespace network_project.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<NetworkLog>      NetworkLogs       { get; set; }
    public DbSet<Node>            Nodes             { get; set; }
    public DbSet<Edge>            Edges             { get; set; }
    public DbSet<QuantumWalkResult> QuantumWalkResults { get; set; }
    public DbSet<QftResult>       QftResults        { get; set; }
    public DbSet<DetectionResult> DetectionResults  { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NetworkLog>(e =>
        {
            e.HasKey(x => x.LogId);
            e.Property(x => x.SourceIp).HasMaxLength(50);
            e.Property(x => x.DestIp).HasMaxLength(50);
            e.Property(x => x.Protocol).HasMaxLength(20);
        });

        modelBuilder.Entity<Node>(e =>
        {
            e.HasKey(x => x.NodeId);
            e.HasIndex(x => x.IpAddress).IsUnique();
            e.Property(x => x.IpAddress).HasMaxLength(50);
        });


        modelBuilder.Entity<Edge>(e =>
        {
            e.HasKey(x => x.EdgeId);
            e.HasIndex(x => new { x.SourceIp, x.DestIp }).IsUnique();
        });

        modelBuilder.Entity<QuantumWalkResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Node)
             .WithMany(n => n.QuantumWalkResults)
             .HasForeignKey(x => x.NodeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        
        modelBuilder.Entity<QftResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Node)
             .WithMany(n => n.QftResults)
             .HasForeignKey(x => x.NodeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DetectionResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ThreatLevel).HasMaxLength(20);
            e.HasOne(x => x.Node)
             .WithMany(n => n.DetectionResults)
             .HasForeignKey(x => x.NodeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<UserToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();
            e.Property(x => x.Token).IsRequired();
            e.HasOne(x => x.User)
             .WithMany(u => u.Tokens)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
