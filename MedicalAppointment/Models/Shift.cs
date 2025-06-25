using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalAppointment.Models
{
	public class Shift
	{
        [Key]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [ForeignKey("Schedule")]
        public string? ScheduleId { get; set; }

        public Schedule? Schedule { get; set; }

        public string? TimeSlot { get; set; }
    }
}

