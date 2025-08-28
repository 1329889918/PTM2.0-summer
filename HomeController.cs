using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PTM2._0.Data;
using PTM2._0.Models;

namespace PTM2._0.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PTM2_0Context _context;

        public HomeController(ILogger<HomeController> logger, PTM2_0Context context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // 获取统计数据API
        // 获取统计数据API（修改后）
        [HttpGet]
        // 获取统计数据API（修改后）
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                // 计算总已售门票（所有时间）
                var soldTickets = await _context.Order
                    .Where(o => o.OrderStatus == OrderStatusEnum.已完成)
                    .SumAsync(o => o.OrderQuantity);

                // 即将举行的演出(未来7天内)
                var upcomingPerformances = await _context.Performance
                    .Where(p => p.Status == PerformanceStatusEnum.未开始 &&
                                p.PerformDate >= DateTime.Now.Date &&
                                p.PerformDate <= DateTime.Now.Date.AddDays(7))
                    .CountAsync();

                // 计算总收入（所有时间）
                var totalRevenue = await _context.Order
                    .Where(o => o.OrderStatus == OrderStatusEnum.已完成)
                    .SumAsync(o => o.TotalAmount);

                var stats = new
                {
                    soldTickets = soldTickets,
                    upcomingPerformances = upcomingPerformances,
                    monthlyRevenue = totalRevenue // 虽然变量名是Monthly，但实际是总收入
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取统计数据时出错");
                return StatusCode(500, "服务器错误");
            }
        }

        // 获取近期演出API
        // 获取近期演出API（修改后）
        [HttpGet]
        public async Task<IActionResult> GetRecentPerformances()
        {
            try
            {
                // 获取未来7天内的"未开始"演出和当前"进行中"的演出
                var performances = await _context.Performance
                    .Include(p => p.Venue)
                    .Include(p => p.Tickets)
                    .Where(p =>
                        // 未开始且在未来7天内，或当前进行中的演出
                        ((p.Status == PerformanceStatusEnum.未开始 &&
                          p.PerformDate >= DateTime.Now.Date &&
                          p.PerformDate <= DateTime.Now.Date.AddDays(7)) ||
                         p.Status == PerformanceStatusEnum.进行中))
                    .OrderBy(p => p.PerformDate)
                    .ThenBy(p => p.StartTime)
                    .ToListAsync();

                var result = performances.Select(p => new
                {
                    PerformID = p.PerformID,
                    PerformName = p.PerformName,
                    Date = p.PerformDate.ToString("yyyy-MM-dd") + " " + p.StartTime.ToString(@"hh\:mm"),
                    Venue = p.Venue.VenueName,
                    // 计算售票百分比（保留两位小数）
                    SoldPercentage = p.Tickets != null && p.Tickets.Any()
                        ? Math.Round((100 - (double)p.Tickets.Sum(t => t.TicketQuantity) / p.Tickets.Sum(t => t.InitialTicketQuantity) * 100), 2)
                        : 0,
                    // 状态转换
                    Status = p.Status switch
                    {
                        PerformanceStatusEnum.未开始 => "未开始",
                        PerformanceStatusEnum.进行中 => "进行中",
                        PerformanceStatusEnum.已结束 => "已完成",
                        PerformanceStatusEnum.已取消 => "已取消",
                        _ => "未知"
                    },
                    StatusClass = p.Status switch
                    {
                        PerformanceStatusEnum.未开始 => "active",
                        PerformanceStatusEnum.进行中 => "active",
                        PerformanceStatusEnum.已结束 => "completed",
                        PerformanceStatusEnum.已取消 => "cancelled",
                        _ => ""
                    }
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取近期演出时出错");
                return StatusCode(500, "服务器错误");
            }
        }

        // 其他方法保持不变...
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}