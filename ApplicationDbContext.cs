using Microsoft.EntityFrameworkCore;
using PTM2._0.Models;

namespace PTM2._0.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 添加您的DbSet
        public DbSet<Performance> Performances { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Order> Orders { get; set; }

        // 可以根据需要添加其他模型

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 在这里配置模型关系和数据种子
            base.OnModelCreating(modelBuilder);
        }
    }
}