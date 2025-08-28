using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PTM2._0.Data;
using PTM2._0.Models;

namespace PTM2._0.Controllers
{
    public class TicketsController : Controller
    {
        private readonly PTM2_0Context _context;

        public TicketsController(PTM2_0Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.PriceSortParam = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            ViewBag.QuantitySortParam = sortOrder == "quantity_asc" ? "quantity_desc" : "quantity_asc";
            ViewBag.PercentSortParam = sortOrder == "percent_asc" ? "percent_desc" : "percent_asc";

            ViewBag.CurrentFilter = searchTerm;

            var ticketsQuery = _context.Ticket
                .Include(t => t.Performance)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ticketsQuery = ticketsQuery.Where(t =>
                    t.Performance.PerformName.Contains(searchTerm) ||
                    t.Price.ToString().Contains(searchTerm));
            }

            var soldOrders = await _context.Order
                .Where(o => o.OrderStatus == OrderStatusEnum.已完成 ||
                           o.OrderStatus == OrderStatusEnum.进行中)
                .GroupBy(o => o.TicketID)
                .Select(g => new { TicketID = g.Key, SoldQuantity = g.Sum(o => o.OrderQuantity) })
                .ToListAsync();

            var tickets = await ticketsQuery.ToListAsync();

            var result = tickets.Select(t => new TicketViewModel
            {
                Ticket = t,
                SoldQuantity = soldOrders.FirstOrDefault(o => o.TicketID == t.TicketID)?.SoldQuantity ?? 0,
                SoldPercentage = t.InitialTicketQuantity > 0
                    ? Math.Round((double)(soldOrders.FirstOrDefault(o => o.TicketID == t.TicketID)?.SoldQuantity ?? 0) / t.InitialTicketQuantity * 100, 1)
                    : 0
            }).AsQueryable();

            switch (sortOrder)
            {
                case "price_asc":
                    result = result.OrderBy(t => t.Ticket.Price);
                    break;
                case "price_desc":
                    result = result.OrderByDescending(t => t.Ticket.Price);
                    break;
                case "quantity_asc":
                    result = result.OrderBy(t => t.Ticket.TicketQuantity);
                    break;
                case "quantity_desc":
                    result = result.OrderByDescending(t => t.Ticket.TicketQuantity);
                    break;
                case "percent_asc":
                    result = result.OrderBy(t => t.SoldPercentage);
                    break;
                case "percent_desc":
                    result = result.OrderByDescending(t => t.SoldPercentage);
                    break;
                default:
                    result = result.OrderByDescending(t => t.Ticket.TicketID);
                    break;
            }

            return View(result.ToList());
        }



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 1. 获取Ticket基础信息
            var ticket = await _context.Ticket
                .Include(t => t.Performance)
                .ThenInclude(p => p.Venue)
                .FirstOrDefaultAsync(m => m.TicketID == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // 2. 计算已售数量
            var soldQuantity = await _context.Order
                .Where(o => o.TicketID == id &&
                           (o.OrderStatus == OrderStatusEnum.已完成 ||
                            o.OrderStatus == OrderStatusEnum.进行中))
                .SumAsync(o => (int?)o.OrderQuantity) ?? 0;

            // 3. 构建ViewModel
            var viewModel = new TicketViewModel
            {
                Ticket = ticket,
                SoldQuantity = soldQuantity,
                SoldPercentage = ticket.InitialTicketQuantity > 0 ?
                    Math.Round((double)soldQuantity / ticket.InitialTicketQuantity * 100, 1) : 0
            };

            return View(viewModel); // 返回ViewModel而非原始Ticket
        }

        public IActionResult Create()
        {
            ViewBag.PerformID = new SelectList(_context.Performance, "PerformID", "PerformName");
            return PartialView("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketID,Price,PerformID,TicketQuantity")] Ticket ticket)
        {
            var errors = new List<string>();

            if (ModelState.IsValid)
            {
                // 仅保留场馆容量验证（如果需要）
                var performance = await _context.Performance
                    .Include(p => p.Venue)
                    .FirstOrDefaultAsync(p => p.PerformID == ticket.PerformID);

                if (performance == null)
                {
                    errors.Add("所选演出不存在");
                }
                else if (ticket.TicketQuantity > performance.Venue.Capacity)
                {
                    errors.Add($"门票数量不能超过场馆容量 ({performance.Venue.Capacity})");
                }
                else
                {
                    try
                    {
                        ticket.InitialTicketQuantity = ticket.TicketQuantity;
                        _context.Add(ticket);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "票务创建成功！" });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"保存失败: {ex.Message}");
                    }
                }
            }
            else
            {
                errors.AddRange(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
            }

            return Json(new
            {
                success = false,
                message = "请修正以下错误",
                errors = errors
            });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["PerformID"] = new SelectList(_context.Performance, "PerformID", "PerformName", ticket.PerformID);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketID,Price,PerformID,InitialTicketQuantity,TicketQuantity")] Ticket ticket)
        {
            if (id != ticket.TicketID)
            {
                return Json(new { success = false, message = "票务ID不匹配" });
            }

            var errors = new List<string>();

            if (ModelState.IsValid)
            {
                try
                {
                    // 获取原始门票数据
                    var existingTicket = await _context.Ticket.FindAsync(id);
                    if (existingTicket == null)
                    {
                        return Json(new { success = false, message = "票务不存在" });
                    }

                    // 检查场馆容量
                    var performance = await _context.Performance
                        .Include(p => p.Venue)
                        .FirstOrDefaultAsync(p => p.PerformID == ticket.PerformID);

                    if (performance == null)
                    {
                        errors.Add("所选演出不存在");
                    }
                    else if (ticket.InitialTicketQuantity > performance.Venue.Capacity)
                    {
                        errors.Add($"票数不能超过场馆容量 ({performance.Venue.Capacity})");
                    }
                    else
                    {
                        // 计算已售数量
                        var soldQuantity = await _context.Order
                            .Where(o => o.TicketID == id &&
                                      (o.OrderStatus == OrderStatusEnum.已完成 ||
                                       o.OrderStatus == OrderStatusEnum.进行中))
                            .SumAsync(o => (int?)o.OrderQuantity) ?? 0;

                        // 确保新设置的初始票数不小于已售数量
                        if (ticket.InitialTicketQuantity < soldQuantity)
                        {
                            errors.Add($"初始票数不能小于已售数量 ({soldQuantity})");
                        }
                        else
                        {
                            // 更新可售票数 = 初始票数 - 已售数量
                            ticket.TicketQuantity = ticket.InitialTicketQuantity - soldQuantity;

                            // 更新其他字段
                            existingTicket.Price = ticket.Price;
                            existingTicket.PerformID = ticket.PerformID;
                            existingTicket.InitialTicketQuantity = ticket.InitialTicketQuantity;
                            existingTicket.TicketQuantity = ticket.TicketQuantity;

                            _context.Update(existingTicket);
                            await _context.SaveChangesAsync();
                            return Json(new { success = true, message = "票务更新成功！" });
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketID))
                    {
                        return Json(new { success = false, message = "票务不存在" });
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // 收集所有验证错误
            errors.AddRange(ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return Json(new
            {
                success = false,
                message = "请修正以下错误",
                errors = errors
            });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.Performance)
                .FirstOrDefaultAsync(m => m.TicketID == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // 计算已售数量
            var soldQuantity = await _context.Order
                .Where(o => o.TicketID == id &&
                           (o.OrderStatus == OrderStatusEnum.已完成 ||
                            o.OrderStatus == OrderStatusEnum.进行中))
                .SumAsync(o => o.OrderQuantity);

            // 创建 ViewModel
            var viewModel = new TicketViewModel
            {
                Ticket = ticket,
                SoldQuantity = soldQuantity,
                SoldPercentage = ticket.InitialTicketQuantity > 0 ?
                    Math.Round((double)soldQuantity / ticket.InitialTicketQuantity * 100, 1) : 0
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket != null)
            {
                _context.Ticket.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.TicketID == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetVenueCapacity(int performId)
        {
            var performance = await _context.Performance
                .Include(p => p.Venue)
                .FirstOrDefaultAsync(p => p.PerformID == performId);

            if (performance == null)
                return NotFound();

            return Ok(performance.Venue.Capacity);
        }
    }
}