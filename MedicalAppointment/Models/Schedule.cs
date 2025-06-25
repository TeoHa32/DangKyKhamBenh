using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalAppointment.Models
{
	public class Schedule
	{
        [Key]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }

        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }
    }
}

