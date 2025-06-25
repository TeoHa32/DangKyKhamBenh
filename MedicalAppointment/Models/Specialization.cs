using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalAppointment.Models
{
	public class Specialization
	{
		[Key]
		public string? Id { get; set; } = Guid.NewGuid().ToString();
        [MaxLength(200)]
        public string? Name { get; set; }
		public string? Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}

