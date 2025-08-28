using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PTM2._0.Models;

namespace PTM2._0.Data
{
    public class PTM2_0Context : DbContext
    {
        public PTM2_0Context(DbContextOptions<PTM2_0Context> options)
            : base(options)
        {
        }

        public DbSet<PTM2._0.Models.Order> Order { get; set; } = default!;
        public DbSet<PTM2._0.Models.Performance> Performance { get; set; }
        public DbSet<PTM2._0.Models.Ticket> Ticket { get; set; }
        public DbSet<PTM2._0.Models.Venue> Venue { get; set; }
        public DbSet<PTM2._0.Models.User> User { get; set; }
        public DbSet<PTM2._0.Models.LoginModel> LoginModel { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 配置表名（确保与数据库表名一致）
            modelBuilder.Entity<Order>().ToTable("Order");
            modelBuilder.Entity<Performance>().ToTable("Performance");
            modelBuilder.Entity<Ticket>().ToTable("Ticket");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Venue>().ToTable("Venue");

            // 配置主键
            modelBuilder.Entity<Order>().HasKey(o => o.OrderID);
            modelBuilder.Entity<Performance>().HasKey(p => p.PerformID);
            modelBuilder.Entity<Ticket>().HasKey(t => t.TicketID);
            modelBuilder.Entity<User>().HasKey(u => u.UserID);
            modelBuilder.Entity<Venue>().HasKey(v => v.VenueID);
            // 配置关系
            modelBuilder.Entity<Performance>()
                .HasOne(p => p.Venue)
                .WithMany(v => v.Performances)
                .HasForeignKey(p => p.VenueID);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Performance)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.PerformID);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserID);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Ticket)
                .WithMany(t => t.Orders)
                .HasForeignKey(o => o.TicketID);

            // 初始数据种子
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Name = "admin",
                    Password = "admin123", // 实际应用中应使用哈希密码
                    IsAdmin = true,
                    Birthdate = new DateTime(1995, 8, 8),
                    Address = "管理员地址",
                    Gender = Gender.男,
                    Email = "admin@qq.com",
                    Phone = "11111111111"
                }
            );
        }
    }
}
