using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalAppointment.Models
{
	public class Appointment
	{
		[Key]
		public string? Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime RegistrationDate { get; set; }

        [ForeignKey("ApplicationUser")]

        public string? UserId { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }

        [MaxLength(200)]
        public string? Symptom { get; set; }


        [ForeignKey("Specialization")]
        public string? SpecializationId { get; set; }
        public virtual Specialization? Specialization { get; set; }

        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [MaxLength(10)]
        public string? AppointmentTime { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}

