using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationApi.Models
{
    [Table("avaiabilities")]
    public class Availability
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("provider_id")]
        public int ProviderId { get; set; }
        
        [Required]
        [Column("start_time")]
        public DateTime StartTime { get; set; }
        
        [Required]
        [Column("end_time")]
        public DateTime EndTime { get; set; }

        [ForeignKey("ProviderId")]
        public required Provider Provider { get; set; }
        
        // zero- or one-to-many, whether the slot is available depends on Appointment table
        public List<Appointment>? Appointments { get; set; }
    }
}