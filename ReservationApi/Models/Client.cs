using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationApi.Models
{
    [Table("clients")]
    public class Client
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // for simplicity, just use name (lastname) for this project
        [Required]
        [Column("name")]
        public required string Name { get; set; }

        //public string? Email { get; set; }

        // zero- or one-to-many
        public List<Appointment>? Appointments { get; set; }

    }
}