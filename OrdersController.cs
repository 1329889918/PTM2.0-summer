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
    public class OrdersController : Controller
    {
        private readonly PTM2_0Context _context;

        public OrdersController(PTM2_0Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTicketPrice(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null) return NotFound();
            return Ok(ticket.Price);
        }

        public async Task<IActionResult> Index(string searchTerm, string sortOrder, string statusFilter)
        {
            var orders = _context.Order
                .Include(o => o.Ticket).ThenInclude(t => t.Performance)
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                orders = orders.Where(o =>
                    o.User.Name.Contains(searchTerm) ||
                    o.Ticket.Performance.PerformName.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse(statusFilter, out OrderStatusEnum status))
                {
                    orders = orders.Where(o => o.OrderStatus == status);
                }
            }

            switch (sortOrder?.ToLower())
            {
                case "asc":
                    orders = orders.OrderBy(o => o.OrderTime);
                    break;
                case "desc":
                default:
                    orders = orders.OrderByDescending(o => o.OrderTime);
                    break;
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortOrder = sortOrder;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.StatusList = Enum.GetValues(typeof(OrderStatusEnum)).Cast<OrderStatusEnum>();

            return View(await orders.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Order
                .Include(o => o.Ticket).ThenInclude(t => t.Performance).ThenInclude(p => p.Venue)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderID == id);

            return order == null ? NotFound() : View(order);
        }

        public async Task<IActionResult> Create()
        {
            // 实时查询可用门票（TicketQuantity > 0）
            var availableTickets = await _context.Ticket
                .Include(t => t.Performance)
                .Where(t => t.TicketQuantity > 0)
                .Select(t => new {
                    t.TicketID,
                    DisplayText = $"{t.Performance.PerformName} (¥{t.Price} 剩余:{t.TicketQuantity})"
                })
                .ToListAsync();

            ViewData["TicketID"] = new SelectList(availableTickets, "TicketID", "DisplayText");
            ViewData["UserID"] = new SelectList(_context.User, "UserID", "Name");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTickets()
        {
            var tickets = await _context.Ticket
                .Include(t => t.Performance)
                .Where(t => t.TicketQuantity > 0)
                .Select(t => new {
                    ticketID = t.TicketID,
                    displayText = $"{t.Performance.PerformName} (¥{t.Price} 剩余:{t.TicketQuantity})"
                })
                .ToListAsync();

            return Json(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserID,TicketID,OrderQuantity,OrderStatus")] Order order)
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateViewData(order);
                return View(order);
            }

            if (order.OrderQuantity > 5)
            {
                ModelState.AddModelError("OrderQuantity", "每张订单最多只能购买5张门票");
                await LoadCreateViewData(order);
                return View(order);
            }

            var ticket = await _context.Ticket
                .Include(t => t.Performance)
                .FirstOrDefaultAsync(t => t.TicketID == order.TicketID);

            if (ticket == null)
            {
                ModelState.AddModelError("", "所选门票不存在");
                await LoadCreateViewData(order);
                return View(order);
            }

            if (ticket.TicketQuantity < order.OrderQuantity)
            {
                ModelState.AddModelError("OrderQuantity", $"库存不足，剩余票数: {ticket.TicketQuantity}");
                await LoadCreateViewData(order);
                return View(order);
            }

            var freshTicket = await _context.Ticket
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TicketID == ticket.TicketID);

            if (freshTicket.TicketQuantity < order.OrderQuantity)
            {
                ModelState.AddModelError("OrderQuantity", "库存已变更，请重新提交");
                await LoadCreateViewData(order);
                return View(order);
            }

            order.OrderTime = DateTime.Now;
            order.OrderStatus = OrderStatusEnum.待支付;
            order.TotalAmount = ticket.Price * order.OrderQuantity;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Order.Add(order);
                    ticket.TicketQuantity -= order.OrderQuantity;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new
                    {
                        success = true,
                        message = "订单创建成功！",
                        redirectUrl = Url.Action(nameof(Index))
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    errors.Add("创建失败：" + ex.Message);

                    return Json(new
                    {
                        success = false,
                        message = "创建订单失败",
                        errors = errors
                    });
                }
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Order
                .Include(o => o.Ticket)
                .FirstOrDefaultAsync(m => m.OrderID == id);

            if (order == null) return NotFound();

            var tickets = await _context.Ticket
                .Include(t => t.Performance)
                .ToListAsync();

            ViewData["TicketID"] = new SelectList(tickets, "TicketID", "Performance.PerformName", order.TicketID);
            ViewData["UserID"] = new SelectList(_context.User, "UserID", "Name", order.UserID);

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,UserID,TicketID,OrderQuantity,OrderStatus")] Order order)
        {
            if (id != order.OrderID) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadEditViewData(order);
                return View(order);
            }

            var originalOrder = await _context.Order
                .Include(o => o.Ticket)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (originalOrder == null) return NotFound();

            var newTicket = await _context.Ticket.FindAsync(order.TicketID);
            if (newTicket == null)
            {
                ModelState.AddModelError("", "所选门票不存在");
                await LoadEditViewData(order);
                return View(order);
            }

            int quantityDiff = order.OrderQuantity - originalOrder.OrderQuantity;
            bool ticketChanged = originalOrder.TicketID != order.TicketID;

            if (ticketChanged || quantityDiff != 0)
            {
                if (ticketChanged)
                {
                    originalOrder.Ticket.TicketQuantity += originalOrder.OrderQuantity;
                    _context.Update(originalOrder.Ticket);

                    if (newTicket.TicketQuantity < order.OrderQuantity)
                    {
                        ModelState.AddModelError("OrderQuantity", $"新门票库存不足，剩余: {newTicket.TicketQuantity}");
                        await LoadEditViewData(order);
                        return View(order);
                    }
                    newTicket.TicketQuantity -= order.OrderQuantity;
                }
                else
                {
                    if (newTicket.TicketQuantity < quantityDiff)
                    {
                        ModelState.AddModelError("OrderQuantity", $"库存不足，剩余: {newTicket.TicketQuantity}");
                        await LoadEditViewData(order);
                        return View(order);
                    }
                    newTicket.TicketQuantity -= quantityDiff;
                }
                _context.Update(newTicket);
            }

            originalOrder.UserID = order.UserID;
            originalOrder.TicketID = order.TicketID;
            originalOrder.OrderQuantity = order.OrderQuantity;
            originalOrder.OrderStatus = order.OrderStatus;
            originalOrder.TotalAmount = newTicket.Price * order.OrderQuantity;

            try
            {
                _context.Update(originalOrder);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "订单更新成功！";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.OrderID)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "更新失败：" + ex.Message);
                await LoadEditViewData(order);
                return View(order);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Order
                .Include(o => o.Ticket).ThenInclude(t => t.Performance)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderID == id);

            return order == null ? NotFound() : View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order
                .Include(o => o.Ticket)
                .FirstOrDefaultAsync(m => m.OrderID == id);

            if (order == null) return NotFound();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (order.Ticket != null)
                    {
                        order.Ticket.TicketQuantity += order.OrderQuantity;
                        _context.Update(order.Ticket);
                    }

                    _context.Order.Remove(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "订单已取消，库存已恢复";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "取消失败：" + ex.Message;
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
        }



        [HttpPost("{id}/pay")]
        public async Task<IActionResult> PayOrder(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null) return NotFound();

            if (order.OrderStatus == OrderStatusEnum.待支付)
            {
                order.OrderStatus = OrderStatusEnum.进行中;
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "支付成功" });
            }
            return BadRequest(new { success = false, message = "状态异常" });
        }

        private async Task LoadCreateViewData(Order order)
        {
            var availableTickets = await _context.Ticket
                .Include(t => t.Performance)
                .Where(t => t.TicketQuantity > 0)
                .ToListAsync();

            ViewData["TicketID"] = new SelectList(availableTickets, "TicketID", "Performance.PerformName", order.TicketID);
            ViewData["UserID"] = new SelectList(_context.User, "UserID", "Name", order.UserID);
        }

        private async Task LoadEditViewData(Order order)
        {
            var tickets = await _context.Ticket
                .Include(t => t.Performance)
                .ToListAsync();

            ViewData["TicketID"] = new SelectList(tickets, "TicketID", "Performance.PerformName", order.TicketID);
            ViewData["UserID"] = new SelectList(_context.User, "UserID", "Name", order.UserID);
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderID == id);
        }

        public IActionResult Charts()
        {
            return View();
        }


    }
}