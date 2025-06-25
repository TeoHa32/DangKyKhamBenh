using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.Models.ViewModels
{
	public class AppointmentVM
	{
        public string? PatientID { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên bệnh nhân")]
        public string? FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string? Address { get; set; }
        public string? AppointmentID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? AppointmentTime { get; set; }
        public string Status = "Đã đăng ký";
        [Required(ErrorMessage = "Vui lòng nhập tình trạng sức khoẻ")]
        public string? Symptom { get; set; }
        public string? SpecializationId { get; set; }
        
    }
}

