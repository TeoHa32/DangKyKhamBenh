using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalAppointment.Data;
using MedicalAppointment.Models;
using MedicalAppointment.Models.ViewModels;
using System.Numerics;
using Microsoft.SqlServer.Server;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace MedicalAppointment.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Doctor
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Doctors.Include(d => d.Specialization).Include(d => d.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/Doctor/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Doctors == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Specialization)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }

        // GET: Admin/Doctor/Create
        public IActionResult Create()
        {
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Name");

            ViewData["UserId"] = new SelectList(_context.Users.Include(m => m.UserRoles)
                                                                .Where(u => u.UserRoles != null && u.UserRoles
                                                                .Any(r => r.Role != null && r.Role.Name == "Doctor")), "Id", "FullName");
            return View();
        }

        // POST: Admin/Doctor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,SpecializationId")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Name", doctor.SpecializationId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", doctor.UserId);
            return View(doctor);
        }

        // GET: Admin/Doctor/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Doctors == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(m => m.UserId == id);
            if (doctor == null)
            {
                return NotFound();
            }
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Name", doctor.SpecializationId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", doctor.UserId);
            return View(doctor);
        }

        // POST: Admin/Doctor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserId,SpecializationId")] Doctor doctor)
        {
            if (id != doctor.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.UserId))
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
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Name", doctor.SpecializationId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", doctor.UserId);
            return View(doctor);
        }

        // GET: Admin/Doctor/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Doctors == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Specialization)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Admin/Doctor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Doctors == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Doctors'  is null.");
            }
            var doctor = await _context.Doctors.FirstOrDefaultAsync(m => m.UserId == id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Xếp lịch
        [HttpGet]
        public JsonResult GetSavedDates(string UserId)
        {
            var listDate = _context.Schedules
                .Where(m =>m.UserId == UserId)
                .Select(s => s.AppointmentDate.ToString("yyyy-MM-dd"))
                .ToList();
            return Json(listDate);
        }

        [HttpGet]
        public JsonResult GetSavedShirt(string userId, DateTime dateTime)
        {
            var grantedShift = _context.Shifts
                .Where(ur => ur.Schedule.UserId == userId)
                .Where(m => m.Schedule.AppointmentDate == dateTime) // Kiểm tra ngày thay vì chuỗi
                .Select(ur => ur.TimeSlot).ToList();
            return Json(grantedShift);
        }

        public IActionResult SetCalendar(string id)
        {
            ViewData["GrantedShift"] = _context.Shifts
                                            .Where(ur => ur.Schedule.UserId == id)
                                            //.Where(m => m.Schedule.AppointmentDate.Day == 21) // Kiểm tra ngày thay vì chuỗi
                                            .Select(ur => ur.TimeSlot).ToList();

            ViewData["UserId"] = new SelectList(_context.Users.Include(m => m.UserRoles)
                                                                .Where(m => m.Id == id)
                                                                .Where(u => u.UserRoles != null && u.UserRoles
                                                                .Any(r => r.Role != null && r.Role.Name == "Doctor")), "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCalendar([Bind("UserId,AppointmentDate")] ShiftScheduleVM scheduleVM, List<string> TimeSlot)
        {
            ViewData["UserId"] = new SelectList(_context.Users.Include(m => m.UserRoles)
                                                        .Where(u => u.UserRoles != null && u.UserRoles
                                                        .Any(r => r.Role != null && r.Role.Name == "Doctor")), "Id", "FullName", scheduleVM.UserId);
            if (ModelState.IsValid)
            {
                try
                {
                    if (TimeSlot == null || TimeSlot.Count == 0)
                    {
                        // Nếu List TimeSlot là null hoặc rỗng, xóa Schedule và Shifts liên quan
                        var existingSchedule = await _context.Schedules.FirstOrDefaultAsync(s => s.UserId == scheduleVM.UserId && s.AppointmentDate == scheduleVM.AppointmentDate);
                        if (existingSchedule != null)
                        {
                            var existingShifts = await _context.Shifts.Where(s => s.ScheduleId == existingSchedule.Id).ToListAsync();
                            _context.Shifts.RemoveRange(existingShifts);
                            _context.Schedules.Remove(existingSchedule);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        // Kiểm tra xem đã có Schedule nào có cùng AppointmentDate và UserId chưa
                        var existingSchedule = await _context.Schedules.FirstOrDefaultAsync(s => s.UserId == scheduleVM.UserId && s.AppointmentDate == scheduleVM.AppointmentDate);

                        if (existingSchedule != null)
                        {
                            // Nếu đã tồn tại Schedule, xóa các Shift của Schedule đó
                            var existingShifts = await _context.Shifts.Where(s => s.ScheduleId == existingSchedule.Id).ToListAsync();
                            _context.Shifts.RemoveRange(existingShifts);
                            // Xóa Schedule
                            _context.Schedules.Remove(existingSchedule);
                        }

                        // Tạo Schedule mới
                        Schedule schedule = new Schedule
                        {
                            UserId = scheduleVM.UserId,
                            AppointmentDate = scheduleVM.AppointmentDate
                        };

                        _context.Schedules.Add(schedule);
                        await _context.SaveChangesAsync();

                        // Thêm các ca trực mới vào cơ sở dữ liệu
                        foreach (var timeSlot in TimeSlot)
                        {
                            Shift shift = new Shift
                            {
                                ScheduleId = schedule.Id,
                                TimeSlot = timeSlot // Sử dụng chuỗi đã chuyển đổi
                            };

                            _context.Shifts.Add(shift);
                        }

                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ViewData["ErrorMessager"] = "Đã xảy ra lỗi! Vui lòng kiểm tra thông tin đã nhập.";
                    return View(scheduleVM);
                }
            }
            ViewData["ErrorMessager"] = "Đã xảy ra lỗi! Vui lòng kiểm tra thông tin đã nhập.";
            return View(scheduleVM);
        }

        private bool DoctorExists(string id)
        {
            return (_context.Doctors?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
