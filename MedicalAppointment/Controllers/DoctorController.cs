using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MedicalAppointment.Data;
using MedicalAppointment.Models;
using MedicalAppointment.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Redirect()
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? "");
            if (user == null)
            {
                return NotFound();
            }
            return Redirect("/Doctor/SetCalendar?id=" + user.Id);
        }
        public async Task<IActionResult> Index(string inputsearch)
        {
            ViewData["Specialize"] = new SelectList(_context.Specializations, "Id", "Name");

            if (String.IsNullOrEmpty(inputsearch))
            {
                var doctor = await _context.Doctors
                .Include(m => m.User)
                .Include(m => m.Specialization)
                .Where(m => m.User != null && m.User.UserRoles != null && m.User.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "Doctor"))
                .ToListAsync();
                return View(doctor);
            }
            else
            {
                var doctor = await _context.Doctors
                .Include(m => m.User)
                .Include(m => m.Specialization)
                .Where(m => m.User != null && m.User.UserRoles != null && m.User.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "Doctor"))
                .Where(m => m.User.FullName.ToLower().Contains(inputsearch.ToLower())
                    || m.SpecializationId.ToLower().Contains(inputsearch.ToLower())
                    || string.IsNullOrEmpty(inputsearch) == true
                    || m.User.Gender.ToLower().Contains(inputsearch.ToLower()))
                .ToListAsync();
                return View(doctor);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetFilteredDoctor(string gender)
        {
            
            var doctor = await _context.Doctors
            .Include(m => m.User)
            .Include(m => m.Specialization)
            .Where(m => m.User != null && m.User.UserRoles != null && m.User.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "Doctor"))
            .Where(m => m.User.Gender.ToLower().Contains(gender.ToLower())
                    || string.IsNullOrEmpty(gender) == true)
            .ToListAsync();
            return PartialView("_DoctorList", doctor);
            
        }

        //sử lý dữ liệu ajax
        [HttpGet]
        public JsonResult GetSavedDates(string UserId)
        {
            var listDate = _context.Schedules
                .Where(m => m.UserId == UserId)
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
        //lập lịch bác sĩ
        public IActionResult SetCalendar(string id)
        {
            //var user = await _context.FindByNameAsync(User.Identity?.Name ?? "");
           
            ViewData["GrantedShift"] = _context.Shifts
                                            .Where(ur => ur.Schedule.UserId == id)
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

                    return View(scheduleVM);
                    //lập thành công
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


    }
}