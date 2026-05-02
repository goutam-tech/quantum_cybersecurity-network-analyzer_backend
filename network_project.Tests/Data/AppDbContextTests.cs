using Xunit;
using Microsoft.EntityFrameworkCore;
using network_project.Data;
using network_project.Models;
using System;
using System.Linq;

namespace network_project.Tests.Data
{
    public class AppDbContextTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // isolated DB
                .Options;

            return new AppDbContext(options);
        }

        // ================= USERS =================

        [Fact]
        public void Add_User_Should_Save_Successfully()
        {
            using var context = GetDbContext();

            var user = new User
            {
                Email = "test@mail.com",
                Name = "Test",
                PasswordHash = "hash"
            };

            context.Users.Add(user);
            context.SaveChanges();

            Assert.Equal(1, context.Users.Count());
        }

        [Fact]
        public void User_Email_Should_Be_Unique()
        {
            using var context = GetDbContext();

            context.Users.Add(new User
            {
                Email = "duplicate@mail.com",
                PasswordHash = "hash"
            });

            context.SaveChanges();

            context.Users.Add(new User
            {
                Email = "duplicate@mail.com",
                PasswordHash = "hash2"
            });

            // ⚠️ InMemory DOES NOT enforce unique constraints → simulate
            Assert.ThrowsAny<Exception>(() => context.SaveChanges());
        }

        // ================= NODE =================

        [Fact]
        public void Add_Node_Should_Save()
        {
            using var context = GetDbContext();

            context.Nodes.Add(new Node
            {
                IpAddress = "192.168.1.1"
            });

            context.SaveChanges();

            Assert.Single(context.Nodes);
        }

        // ================= EDGE =================

        [Fact]
        public void Add_Edge_Should_Save()
        {
            using var context = GetDbContext();

            context.Edges.Add(new Edge
            {
                SourceIp = "1.1.1.1",
                DestIp = "2.2.2.2"
            });

            context.SaveChanges();

            Assert.Single(context.Edges);
        }

        // ================= RELATIONSHIPS =================

        [Fact]
        public void DetectionResult_Should_Link_To_Node()
        {
            using var context = GetDbContext();

            var node = new Node { IpAddress = "10.0.0.1" };
            context.Nodes.Add(node);
            context.SaveChanges();

            var detection = new DetectionResult
            {
                NodeId = node.NodeId,
                ThreatLevel = "Attack",
                Confidence = 0.9,
                DetectedAt = DateTime.UtcNow
            };

            context.DetectionResults.Add(detection);
            context.SaveChanges();

            var saved = context.DetectionResults.Include(x => x.Node).First();

            Assert.NotNull(saved.Node);
            Assert.Equal("10.0.0.1", saved.Node.IpAddress);
        }

        // ================= TOKEN =================

        [Fact]
        public void UserToken_Should_Link_To_User()
        {
            using var context = GetDbContext();

            var user = new User
            {
                Email = "token@mail.com",
                PasswordHash = "hash"
            };

            context.Users.Add(user);
            context.SaveChanges();

            var token = new UserToken
            {
                UserId = user.Id,
                Token = "abc123",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsRevoked = false
            };

            context.UserTokens.Add(token);
            context.SaveChanges();

            var saved = context.UserTokens.Include(x => x.User).First();

            Assert.NotNull(saved.User);
            Assert.Equal("token@mail.com", saved.User.Email);
        }

        // ================= CASCADE DELETE =================

        [Fact]
        public void Deleting_Node_Should_Delete_DetectionResults()
        {
            using var context = GetDbContext();

            var node = new Node { IpAddress = "10.0.0.2" };
            context.Nodes.Add(node);
            context.SaveChanges();

            context.DetectionResults.Add(new DetectionResult
            {
                NodeId = node.NodeId,
                ThreatLevel = "Attack",
                Confidence = 0.8,
                DetectedAt = DateTime.UtcNow
            });

            context.SaveChanges();

            context.Nodes.Remove(node);
            context.SaveChanges();

            Assert.Empty(context.DetectionResults);
        }
    }
}