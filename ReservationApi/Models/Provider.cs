using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationApi.Models
{
    [Table("providers")]
    public class Provider
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // for simplicity, just use name (lastname) for this project
        [Required]
        [Column("name")]
        public required string Name { get; set; }

        // public string? Email { get; set; }

        // zero- or one-to-many
        public List<Availability>? Availabilities { get; set; }

        // future consideration: add more properties such as speciality, ratings as needed (good for data retrival)

    }
}