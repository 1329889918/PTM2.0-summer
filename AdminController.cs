using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PTM2._0.Data;
using PTM2._0.Models;

namespace PTM2._0.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly PTM2_0Context _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(PTM2_0Context context, ILogger<AdminController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // 并行执行多个统计查询提高性能
                var totalUsersTask = _context.User.CountAsync();
                var totalPerformancesTask = _context.Performance.CountAsync();
                var totalOrdersTask = _context.Order.CountAsync();
                var totalRevenueTask = _context.Order.SumAsync(o => o.TotalAmount);
                var recentOrdersTask = GetRecentOrdersAsync(5);

                await Task.WhenAll(
                    totalUsersTask,
                    totalPerformancesTask,
                    totalOrdersTask,
                    totalRevenueTask,
                    recentOrdersTask
                );

                var viewModel = new AdminDashboardViewModel
                {
                    TotalUsers = await totalUsersTask,
                    TotalPerformances = await totalPerformancesTask,
                    TotalOrders = await totalOrdersTask,
                    TotalRevenue = await totalRevenueTask,
                    RecentOrders = await recentOrdersTask,
                    PerformanceStats = await GetPerformanceStatsAsync(),
                    UserActivity = await GetRecentUserActivityAsync(5)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View("Error", new ErrorViewModel
                {
                    Message = "加载仪表盘数据时出错",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        private async Task<List<Order>> GetRecentOrdersAsync(int count)
        {
            return await _context.Order
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Ticket)
                    .ThenInclude(t => t.Performance)
                .OrderByDescending(o => o.OrderTime)
                .Take(count)
                .ToListAsync();
        }

        private async Task<List<PerformanceStat>> GetPerformanceStatsAsync()
        {
            return await _context.Performance
                .AsNoTracking()
                .Select(p => new PerformanceStat
                {
                    PerformanceId = p.PerformID,
                    PerformanceName = p.PerformName,
                    TicketSales = p.Tickets.Sum(t => t.Orders.Sum(o => o.OrderQuantity)),
                    TotalRevenue = p.Tickets.Sum(t => t.Orders.Sum(o => o.TotalAmount))
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<UserActivity>> GetRecentUserActivityAsync(int count)
        {
            return await _context.User
                .AsNoTracking()
                .OrderByDescending(u => u.Orders.Count)
                .Take(count)
                .Select(u => new UserActivity
                {
                    UserId = u.UserID,
                    UserName = u.Name,
                    OrderCount = u.Orders.Count,
                    LastOrderDate = u.Orders.Max(o => o.OrderTime)
                })
                .ToListAsync();
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPerformances { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<PerformanceStat> PerformanceStats { get; set; }
        public List<UserActivity> UserActivity { get; set; }
    }

    public class PerformanceStat
    {
        public int PerformanceId { get; set; }
        public string PerformanceName { get; set; }
        public int TicketSales { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class UserActivity
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int OrderCount { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }

    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public string Message { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

}
