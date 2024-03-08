using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationApi.Models
{
    [Table("appointments")]
    public class Appointment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("availability_id")]
        public int AvailabilityId { get; set; }

        [Required]
        [Column("client_id")]
        public int ClientId { get; set; }

        [Required]
        [Column("is_confirmed")]
        public bool IsConfirmed { get; set; }

        [Required]
        [Column("reservation_time")]
        public DateTime ReservationTime { get; set; }

        // automatically expired after 30 minutes from the ReservationTime
        [NotMapped]
        public DateTime ExpirationTime => ReservationTime.AddMinutes(30);


        [ForeignKey("AvailabilityId")]
        public required Availability Availability { get; set; }

        [ForeignKey("ClientId")]
        public required Client Client { get; set; }
    }
}