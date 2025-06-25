using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalAppointment.Models.ViewModels
{
	public class ShiftScheduleVM
	{

        public string? UserId { get; set; }

        public string? TimeSlot { get; set; }

        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }
    }
}

