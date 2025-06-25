using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalAppointment.Data;
using MedicalAppointment.Models;
using Microsoft.AspNetCore.Authorization;

namespace MedicalAppointment.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]  
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Appointment
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Appointments
                .OrderByDescending(m => m.AppointmentDate)
                .Include(a => a.ApplicationUser)
                .Include(a => a.Specialization);
            return View(await applicationDbContext.ToListAsync());
        }
        //đếm số lượng để giới hạn lượt đăng kí trong ngày
        [HttpGet]
        public JsonResult GetDateByCount()
        {
            var getDateByCount = _context.Appointments
                .GroupBy(a => a.AppointmentDate)
                .Where(group => group.Count() >= 3)
                .Select(group => group.Key.Date.ToString("yyyy-MM-dd"))
                .ToList();
            return Json(getDateByCount);
        }
        // GET: Admin/Appointment/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.ApplicationUser)
                .Include(a => a.Specialization)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Admin/Appointment/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Id");
            return View();
        }

        // POST: Admin/Appointment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RegistrationDate,UserId,Symptom,SpecializationId,AppointmentDate,AppointmentTime,Price")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", appointment.UserId);
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Id", appointment.SpecializationId);
            return View(appointment);
        }

        // GET: Admin/Appointment/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", appointment.UserId);
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Id", appointment.SpecializationId);
            return View(appointment);
        }

        // POST: Admin/Appointment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,RegistrationDate,UserId,Symptom,SpecializationId,AppointmentDate,AppointmentTime,Price")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", appointment.UserId);
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Id", appointment.SpecializationId);
            return View(appointment);
        }

        // GET: Admin/Appointment/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.ApplicationUser)
                .Include(a => a.Specialization)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Admin/Appointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Appointments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointments'  is null.");
            }
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(string id)
        {
          return (_context.Appointments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
