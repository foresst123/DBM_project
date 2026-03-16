using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;

namespace SWD302_Project_HostelManagement.Controllers
{
    public class BookingRequestsController : Controller
    {
        private readonly AppDbContext _context;

        public BookingRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: BookingRequests
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.BookingRequests.Include(b => b.Room).Include(b => b.Tenant);
            return View(await appDbContext.ToListAsync());
        }

        // GET: BookingRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingRequest = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (bookingRequest == null)
            {
                return NotFound();
            }

            return View(bookingRequest);
        }

        // GET: BookingRequests/Create
        public IActionResult Create()
        {
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomId");
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "TenantId");
            return View();
        }

        // POST: BookingRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,RoomId,TenantId,RequestType,StartDate,EndDate,Status,RejectReason,CreatedDate,UpdatedDate")] BookingRequest bookingRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookingRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomId", bookingRequest.RoomId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "TenantId", bookingRequest.TenantId);
            return View(bookingRequest);
        }

        // GET: BookingRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingRequest = await _context.BookingRequests.FindAsync(id);
            if (bookingRequest == null)
            {
                return NotFound();
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomId", bookingRequest.RoomId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "TenantId", bookingRequest.TenantId);
            return View(bookingRequest);
        }

        // POST: BookingRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,RoomId,TenantId,RequestType,StartDate,EndDate,Status,RejectReason,CreatedDate,UpdatedDate")] BookingRequest bookingRequest)
        {
            if (id != bookingRequest.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookingRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingRequestExists(bookingRequest.BookingId))
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
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomId", bookingRequest.RoomId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "TenantId", bookingRequest.TenantId);
            return View(bookingRequest);
        }

        // GET: BookingRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingRequest = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (bookingRequest == null)
            {
                return NotFound();
            }

            return View(bookingRequest);
        }

        // POST: BookingRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookingRequest = await _context.BookingRequests.FindAsync(id);
            if (bookingRequest != null)
            {
                _context.BookingRequests.Remove(bookingRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingRequestExists(int id)
        {
            return _context.BookingRequests.Any(e => e.BookingId == id);
        }
    }
}
