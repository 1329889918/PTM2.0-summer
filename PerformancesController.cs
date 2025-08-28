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
    public class PerformancesController : Controller
    {
        private readonly PTM2_0Context _context;

        public PerformancesController(PTM2_0Context context)
        {
            _context = context;
        }

        // GET: Performances
        // GET: Performances
        public async Task<IActionResult> Index(string searchTerm, string sortOrder, string typeFilter, string statusFilter)
        {
            // 排序状态维护
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParam = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            // 搜索条件处理
            ViewBag.CurrentFilter = searchTerm;

            // 类型筛选处理
            ViewBag.TypeFilter = typeFilter;
            ViewBag.TypeList = Enum.GetValues(typeof(PerformanceTypeEnum)).Cast<PerformanceTypeEnum>();

            // 状态筛选处理
            ViewBag.StatusFilter = statusFilter;
            ViewBag.StatusList = Enum.GetValues(typeof(PerformanceStatusEnum)).Cast<PerformanceStatusEnum>();

            // 基础查询
            var performances = _context.Performance
                .Include(p => p.Venue)
                .AsQueryable();

            // 应用搜索条件
            if (!string.IsNullOrEmpty(searchTerm))
            {
                performances = performances.Where(p =>
                    p.PerformName.Contains(searchTerm) ||
                    p.Venue.VenueName.Contains(searchTerm));
            }

            // 应用类型筛选
            if (!string.IsNullOrEmpty(typeFilter) && Enum.TryParse(typeFilter, out PerformanceTypeEnum type))
            {
                performances = performances.Where(p => p.PerformType == type);
            }

            // 应用状态筛选
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse(statusFilter, out PerformanceStatusEnum status))
            {
                performances = performances.Where(p => p.Status == status);
            }

            // 应用排序
            switch (sortOrder)
            {
                case "name_asc":
                    performances = performances.OrderBy(p => p.PerformName);
                    break;
                case "name_desc":
                    performances = performances.OrderByDescending(p => p.PerformName);
                    break;
                case "date_asc":
                    performances = performances.OrderBy(p => p.PerformDate);
                    break;
                case "date_desc":
                default:
                    performances = performances.OrderByDescending(p => p.PerformDate);
                    break;
            }

            // 直接返回全部结果（移除分页）
            return View(await performances.ToListAsync());
        }

        // GET: Performances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var performance = await _context.Performance
                .Include(p => p.Venue)
                .FirstOrDefaultAsync(m => m.PerformID == id);
            if (performance == null)
            {
                return NotFound();
            }

            return View(performance);
        }

        // GET: Performances/Create
        // 控制器

        public IActionResult Create()
        {
            // 统一使用 ViewBag.Venues 传递场馆列表
            ViewBag.Venues = _context.Venue.ToList();
            return View();
        }

        // POST: Performances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PerformID,PerformName,StartTime,EndTime,PerformDate,VenueID,PerformType,Status")] Performance performance)
        {
            var errors = new List<string>();

            // 前端验证
            if (ModelState.IsValid)
            {
                // 后端验证
                if (performance.EndTime <= performance.StartTime)
                {
                    errors.Add("结束时间必须晚于开始时间");
                }

                if (errors.Count == 0)
                {
                    try
                    {
                        _context.Add(performance);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "演出创建成功！" });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"保存失败: {ex.Message}");
                    }
                }
            }
            else
            {
                // 收集模型错误
                errors.AddRange(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
            }

            // 返回错误
            return Json(new
            {
                success = false,
                message = "请修正以下错误",
                errors = errors
            });
        }

        // GET: Performances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var performance = await _context.Performance.FindAsync(id);
            if (performance == null)
            {
                return NotFound();
            }
            ViewBag.Venues = await _context.Venue.ToListAsync();
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", performance.VenueID);
            ViewData["PerformType"] = new SelectList(Enum.GetValues(typeof(PerformanceTypeEnum)), "Value", "Name", performance.PerformType);
            return View(performance);
        }

        // POST: Performances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PerformID,PerformName,StartTime,EndTime,PerformDate,VenueID,PerformType,Status")] Performance performance)
        {
            if (id != performance.PerformID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(performance);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PerformanceExists(performance.PerformID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", performance.VenueID);
            ViewData["PerformType"] = new SelectList(Enum.GetValues(typeof(PerformanceTypeEnum)), "Value", "Name", performance.PerformType);
            ViewBag.Venues = await _context.Venue.ToListAsync();
            return View(performance);
        }

        // GET: Performances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var performance = await _context.Performance
                .Include(p => p.Venue)
                .FirstOrDefaultAsync(m => m.PerformID == id);
            if (performance == null)
            {
                return NotFound();
            }

            return View(performance);
        }

        // POST: Performances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var performance = await _context.Performance.FindAsync(id);
            if (performance != null)
            {
                _context.Performance.Remove(performance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PerformanceExists(int id)
        {
            return _context.Performance.Any(e => e.PerformID == id);
        }
        // 添加获取场馆列表的API
        [HttpGet]
        public IActionResult GetVenues()
        {
            try
            {
                // 查询所有场馆数据
                var venues = _context.Venue
                    .Select(v => new {
                        venueID = v.VenueID,
                        venueName = v.VenueName
                    })
                    .ToList();

                return Ok(venues);
            }
            catch (Exception ex)
            {
                // 记录异常日志
                Console.Error.WriteLine($"获取场馆列表失败: {ex.Message}");
                return StatusCode(500, "服务器内部错误");
            }
        }
    }
}