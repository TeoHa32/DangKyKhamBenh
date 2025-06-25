// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MedicalAppointment.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MedicalAppointment.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        //private readonly IWebHostEnvironment _webHost;
        private IHostEnvironment _hostingEnvironment;
        private IWebHostEnvironment _webHost;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHostEnvironment hostingEnvironment,
            IWebHostEnvironment webHost
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHost = webHost;
            _hostingEnvironment = hostingEnvironment;

        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            //phần đã được custom 
            [DataType(DataType.Text)]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            public string Address { get; set; }

            public IFormFile Image { get; set; }

            public string Gender { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            Username = userName;
            ViewData["UserId"] = user.Id;
            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FullName = user.FullName,
                Gender = user.Gender,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
            };
            ViewData["image"] = user.Image;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }
        public async Task<IActionResult> Upload()
        {
            // Logic tải ảnh lên
            var user = await _userManager.GetUserAsync(User);

            if (Input.Image != null && Input.Image.Length > 0)
            {
                string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "images", "profile");
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.Image.FileName);
                string filePath = Path.Combine(uploads, uniqueFileName);

                // Xóa ảnh cũ (nếu có)
                if(user.Image != null)
                {
                    string oldImagePath = Path.Combine(uploads, user.Image); // Giả sử 'user.Image' lưu trữ tên tệp ảnh hiện tại
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.Image.CopyToAsync(fileStream);
                }

                // Cập nhật thuộc tính ảnh đại diện của người dùng
                user.Image = uniqueFileName; // Thay thế bằng tên thuộc tính của bạn
                await _userManager.UpdateAsync(user); // Cập nhật đối tượng người dùng trong cơ sở dữ liệu
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            //Image Upload funtion
            await Upload();
            //
            user.FullName = Input.FullName;
            user.DateOfBirth = Input.DateOfBirth;
            user.Gender = Input.Gender;
            user.Address = Input.Address;


            // Cập nhật thông tin người dùng
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            try
            {
                await _userManager.UpdateAsync(user);
                StatusMessage = "Your profile has been updated";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error updating profile: " + ex.Message;
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }

    }
}
