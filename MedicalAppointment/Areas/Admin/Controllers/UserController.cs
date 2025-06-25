using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedicalAppointment.Data;
using MedicalAppointment.Models;
using MedicalAppointment.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        //2.danh sách tài khoản
        public async Task<IActionResult> Index()
        {
            var listUser = await _context.Users.ToListAsync();
            return View(listUser);
        }

        public async Task<IActionResult> PatientsIndex()
        {
            var listUser = await _context.Users
                .Include(m => m.UserRoles)
                .Where(u => u.UserRoles != null && u.UserRoles.Any(r => r.Role != null && r.Role.Name == "Patients"))
                .ToListAsync();
            return View(listUser);
        }
        public async Task<IActionResult> DoctorIndex()
        {
            var listUser = await _context.Users
                .Include(m => m.UserRoles)
                .Where(u => u.UserRoles != null && u.UserRoles.Any(r => r.Role != null && r.Role.Name == "Doctor"))
                .ToListAsync();
            return View(listUser);
        }
        public async Task<IActionResult> AdminIndex()
        {
            var listUser = await _context.Users
                .Include(m => m.UserRoles)
                .Where(u => u.UserRoles != null && u.UserRoles.Any(r => r.Role != null && r.Role.Name == "Admin"))
                .ToListAsync();
            return View(listUser);
        }

        //2.chi tiết tài khoản
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var userRole = _context.UserRoles.FirstOrDefaultAsync(m => m.UserId == id);
            var role = _context.UserRoles.ToList();
                                
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name");
            ViewData["GrantedRoles"] = await _context.UserRoles.Where(ur => ur.UserId == id).Select(ur => ur.RoleId).ToListAsync();

            return View(user);
        }

        // GET: Admin/Specialization/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var specialization = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (specialization == null)
            {
                return NotFound();
            }

            return View(specialization);
        }

        // POST: Admin/Specialization/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users'  is null.");
            }
            var specialization = await _context.Users.FindAsync(id);
            if (specialization != null)
            {
                _context.Users.Remove(specialization);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool UserExists(string id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpPost]
        public JsonResult GrantPermissions(string UserId, string RoleId)
        {
            var grantPermissions = _context.UserRoles.SingleOrDefault(m => m.UserId == UserId && m.RoleId == RoleId);
            if (grantPermissions != null)
            {
                //Xóa
                _context.UserRoles.Remove(grantPermissions);
                _context.SaveChanges();
            }
            else
            {
                grantPermissions = new ApplicationUserRole();
                grantPermissions.UserId = UserId;
                grantPermissions.RoleId = RoleId;
                _context.Add(grantPermissions);
                _context.SaveChanges();
            }
            return Json(new
            {
                status = "Đã phân quyền"
            });
        }
    }
}