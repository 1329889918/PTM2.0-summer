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

        // ��ȡͳ������API
        // ��ȡͳ������API���޸ĺ�
        [HttpGet]
        // ��ȡͳ������API���޸ĺ�
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                // ������������Ʊ������ʱ�䣩
                var soldTickets = await _context.Order
                    .Where(o => o.OrderStatus == OrderStatusEnum.�����)
                    .SumAsync(o => o.OrderQuantity);

                // �������е��ݳ�(δ��7����)
                var upcomingPerformances = await _context.Performance
                    .Where(p => p.Status == PerformanceStatusEnum.δ��ʼ &&
                                p.PerformDate >= DateTime.Now.Date &&
                                p.PerformDate <= DateTime.Now.Date.AddDays(7))
                    .CountAsync();

                // ���������루����ʱ�䣩
                var totalRevenue = await _context.Order
                    .Where(o => o.OrderStatus == OrderStatusEnum.�����)
                    .SumAsync(o => o.TotalAmount);

                var stats = new
                {
                    soldTickets = soldTickets,
                    upcomingPerformances = upcomingPerformances,
                    monthlyRevenue = totalRevenue // ��Ȼ��������Monthly����ʵ����������
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "��ȡͳ������ʱ����");
                return StatusCode(500, "����������");
            }
        }

        // ��ȡ�����ݳ�API
        // ��ȡ�����ݳ�API���޸ĺ�
        [HttpGet]
        public async Task<IActionResult> GetRecentPerformances()
        {
            try
            {
                // ��ȡδ��7���ڵ�"δ��ʼ"�ݳ��͵�ǰ"������"���ݳ�
                var performances = await _context.Performance
                    .Include(p => p.Venue)
                    .Include(p => p.Tickets)
                    .Where(p =>
                        // δ��ʼ����δ��7���ڣ���ǰ�����е��ݳ�
                        ((p.Status == PerformanceStatusEnum.δ��ʼ &&
                          p.PerformDate >= DateTime.Now.Date &&
                          p.PerformDate <= DateTime.Now.Date.AddDays(7)) ||
                         p.Status == PerformanceStatusEnum.������))
                    .OrderBy(p => p.PerformDate)
                    .ThenBy(p => p.StartTime)
                    .ToListAsync();

                var result = performances.Select(p => new
                {
                    PerformID = p.PerformID,
                    PerformName = p.PerformName,
                    Date = p.PerformDate.ToString("yyyy-MM-dd") + " " + p.StartTime.ToString(@"hh\:mm"),
                    Venue = p.Venue.VenueName,
                    // ������Ʊ�ٷֱȣ�������λС����
                    SoldPercentage = p.Tickets != null && p.Tickets.Any()
                        ? Math.Round((100 - (double)p.Tickets.Sum(t => t.TicketQuantity) / p.Tickets.Sum(t => t.InitialTicketQuantity) * 100), 2)
                        : 0,
                    // ״̬ת��
                    Status = p.Status switch
                    {
                        PerformanceStatusEnum.δ��ʼ => "δ��ʼ",
                        PerformanceStatusEnum.������ => "������",
                        PerformanceStatusEnum.�ѽ��� => "�����",
                        PerformanceStatusEnum.��ȡ�� => "��ȡ��",
                        _ => "δ֪"
                    },
                    StatusClass = p.Status switch
                    {
                        PerformanceStatusEnum.δ��ʼ => "active",
                        PerformanceStatusEnum.������ => "active",
                        PerformanceStatusEnum.�ѽ��� => "completed",
                        PerformanceStatusEnum.��ȡ�� => "cancelled",
                        _ => ""
                    }
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "��ȡ�����ݳ�ʱ����");
                return StatusCode(500, "����������");
            }
        }

        // �����������ֲ���...
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