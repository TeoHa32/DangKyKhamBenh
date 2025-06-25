using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MedicalAppointment.Data;
using MedicalAppointment.Models;
using MedicalAppointment.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.Controllers
{

    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationController(ApplicationDbContext context , UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? "");
            if (user == null)
            {
                return NotFound();
            }
            var appointment = await _context.Appointments.Include(m =>m.Specialization)
                .OrderByDescending(m => m.RegistrationDate)
                .FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (appointment == null)
            {
                return NotFound();
            }
            //ViewData["Price"] = _context.Specializations.Where(m => m.Id == appointment.SpecializationId).ToList();
            return View(appointment);
        }
        public IActionResult Payment()
        {
            return View();
        }
        
        public async Task<IActionResult> Create()
        {
            if (User.Identity?.Name != null)
            {
                ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Name");
                var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? "");
                if (user == null)
                {
                    return NotFound();
                }
                var AppointmentVM = new AppointmentVM
                {
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender
                };
                return View(AppointmentVM);
            }
            return Redirect("/Identity/Account/Register");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientID,FullName,DateOfBirth,Gender,PhoneNumber,Address,AppointmentDate,AppointmentTime,Symptom,SpecializationId")] AppointmentVM appointmentVM)
        {
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Name");
            if (ModelState.IsValid)
            {
                //ApplicationUser appUser = new ApplicationUser();
                //appUser.Id = appointmentVM.PatientID;
                //appUser.FullName = appointmentVM.FullName;
                //appUser.DateOfBirth = appointmentVM.DateOfBirth;
                //appUser.Gender = appointmentVM.Gender;
                //appUser.Address = appointmentVM.Address;
                //appUser.PhoneNumber = appointmentVM.PhoneNumber;
                //patient.Email = appointmentVM.Email;
                //await _userManager.UpdateAsync(appUser);
                //await _context.SaveChangesAsync();
                try
                {
                    Appointment appointment = new Appointment();
                    var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? "");
                    var price = await _context.Specializations.FindAsync(appointmentVM.SpecializationId);
                    if (user == null || price == null)
                    {
                        return NotFound();
                    }
                    appointment.UserId = user.Id;
                    appointment.RegistrationDate = DateTime.Now;
                    appointment.Symptom = appointmentVM.Symptom;
                    appointment.AppointmentDate = appointmentVM.AppointmentDate;
                    appointment.AppointmentTime = appointmentVM.AppointmentTime;
                    appointment.SpecializationId = appointmentVM.SpecializationId;
                    appointment.Price = price.Price;
                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ViewData["ErrorMessager"] = "Đã xảy ra lỗi! vui lòng kiểm tra thông tin đã nhập";
                    return View(appointmentVM);
                }
            }
            ViewData["ErrorMessager"] = "(2)Đã xảy ra lỗi! vui lòng kiểm tra thông tin đã nhập";
            return View(appointmentVM);
        }
    }
}