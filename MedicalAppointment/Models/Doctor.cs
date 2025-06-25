using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.Models
{
    [PrimaryKey(nameof(UserId), nameof(SpecializationId))]
    public class Doctor
	{
        public string ? UserId { get; set; }
        public string ? SpecializationId { get; set; }

       
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("SpecializationId")]
        public virtual Specialization? Specialization { get; set; }
    }
}

