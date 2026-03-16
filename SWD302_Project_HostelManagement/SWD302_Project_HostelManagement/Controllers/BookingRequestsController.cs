using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;

namespace SWD302_Project_HostelManagement.Controllers
{
    [Authorize(Roles = "HostelOwner")]
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
            var ownerIdClaim = User.FindFirst("ProfileId")?.Value;
            if (!int.TryParse(ownerIdClaim, out int ownerId))
                return RedirectToAction("Login", "Auth");

            var appDbContext = _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                    .ThenInclude(t => t.Account)
                .Where(b => b.Room.OwnerId == ownerId);
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

        // POST: BookingRequests/Approve/5
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var booking = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                    .ThenInclude(t => t.Account)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                TempData["Error"] = "Booking request not found.";
                return RedirectToAction("Index");
            }

            if (booking.Status != "Pending")
            {
                TempData["Error"] = "Only pending bookings can be approved.";
                return RedirectToAction("Index");
            }

            if (booking.Room.Status != "Available")
            {
                TempData["Error"] = "Room is no longer available.";
                return RedirectToAction("Index");
            }

            booking.Status = "Approved";
            booking.UpdatedDate = DateTime.UtcNow;

            booking.Room.Status = "Occupied";
            booking.Room.UpdatedAt = DateTime.UtcNow;

            //var log = new RoomUpdateLog
            //{
            //    RoomId = booking.Room.RoomId,
            //    BookingId = booking.BookingId,
            //    ChangedByOwnerId = int.Parse(User.FindFirst("ProfileId")?.Value ?? "0"),
            //    StatusBefore = "Available",
            //    StatusAfter = "Occupied",
            //    ChangedAt = DateTime.UtcNow
            //};
            //await _context.RoomUpdateLogs.AddAsync(log);

            var tenantEmail = booking.Tenant.Account.Email;
            var notification = new Notification
            {
                BookingId = booking.BookingId,
                RecipientEmail = tenantEmail,
                Subject = $"Booking Approved - Room {booking.Room.RoomNumber}",
                MessageContent = $"Dear {booking.Tenant.Name}, your booking request for room {booking.Room.RoomNumber} has been approved.",
                Type = "BookingApproved",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Notifications.AddAsync(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Booking #{id} approved successfully.";
            return RedirectToAction("Index");
        }

        // GET: BookingRequests/Reject/5
        [HttpGet]
        public async Task<IActionResult> Reject(int id)
        {
            var booking = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != "Pending")
            {
                TempData["Error"] = "Only pending bookings can be rejected.";
                return RedirectToAction("Index");
            }

            return View(booking);
        }

        // POST: BookingRequests/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, [Bind("BookingId,RejectReason")] BookingRequest bookingRequest)
        {
            var booking = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                    .ThenInclude(t => t.Account)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                TempData["Error"] = "Booking request not found.";
                return RedirectToAction("Index");
            }

            if (booking.Status != "Pending")
            {
                TempData["Error"] = "Only pending bookings can be rejected.";
                return RedirectToAction("Index");
            }

            booking.Status = "Rejected";
            booking.RejectReason = bookingRequest.RejectReason;
            booking.UpdatedDate = DateTime.UtcNow;

            var tenantEmail = booking.Tenant.Account.Email;
            var notification = new Notification
            {
                BookingId = booking.BookingId,
                RecipientEmail = tenantEmail,
                Subject = $"Booking Rejected - Room {booking.Room.RoomNumber}",
                MessageContent = $"Dear {booking.Tenant.Name}, your booking request has been rejected. Reason: {bookingRequest.RejectReason}",
                Type = "BookingRejected",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Notifications.AddAsync(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Booking #{id} rejected.";
            return RedirectToAction("Index");
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
