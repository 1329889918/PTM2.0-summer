using Microsoft.AspNetCore.Mvc;
using PTM2._0.Models;  // 确保引用了您的模型命名空间
using PTM2._0.Data;    // 引用您的DbContext所在命名空间
using Microsoft.EntityFrameworkCore; // 添加这行

[Route("api/[controller]")]
[ApiController]
public class OrdersApiController : ControllerBase
{
    private readonly PTM2_0Context _context;

    public OrdersApiController(PTM2_0Context context)
    {
        _context = context;
    }

    // 获取最近30天每日销售额
    [HttpGet("DailySales")]
    public ActionResult<DailySalesData> GetDailySales()
    {
        try
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-30);

            // 直接按日期分组并汇总订单金额
            var sales = _context.Order
                .Where(o => o.OrderTime.Date >= startDate && o.OrderTime.Date <= endDate)
                .GroupBy(o => o.OrderTime.Date)
                .Select(g => new {
                    Date = g.Key,
                    TotalSales = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            // 补全日期范围
            var dates = new List<string>();
            var amounts = new List<decimal>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                dates.Add(date.ToString("MM-dd"));
                var sale = sales.FirstOrDefault(s => s.Date == date);
                amounts.Add(sale?.TotalSales ?? 0m);
            }

            return new DailySalesData
            {
                Dates = dates.ToArray(),
                Amounts = amounts.ToArray()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API异常: {ex.Message}");
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    // 修改 PerformanceSales 方法
    [HttpGet("PerformanceSales")]
    public ActionResult<PerformanceSalesData> GetPerformanceSales()
    {
        // 直接使用订单金额，避免关联查询
        var result = _context.Order
            .GroupBy(o => o.Ticket.Performance.PerformName)
            .Select(g => new {
                Name = g.Key,
                Value = g.Sum(o => o.TotalAmount)
            })
            .ToList();

        return new PerformanceSalesData
        {
            Names = result.Select(r => r.Name).ToArray(),
            Values = result.Select(r => r.Value).ToArray()
        };
    }
    // 获取用户消费占比
    [HttpGet("UserPurchases")]
    public ActionResult<UserPurchaseData> GetUserPurchases()
    {
        var result = _context.Order.Include(o => o.Ticket).ThenInclude(t => t.Performance)
            .GroupBy(o => o.User.Name)
            .Select(g => new {
                Name = g.Key,
                Value = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(g => g.Value)
            .Take(10) // 只取前10名用户
            .ToList();

        return new UserPurchaseData
        {
            Names = result.Select(r => r.Name).ToArray(),
            Values = result.Select(r => r.Value).ToArray()
        };
    }
}