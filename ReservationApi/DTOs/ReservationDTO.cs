namespace ReservationApi.DTOs
{
    public class ReservationDTO
    {
        public int AppointmentId { get; set; }
        public int AvailabilityId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public int ClientId { get; set; }
        public required string ClientName { get; set; }
        public bool IsConfirm { get; set; }
        public DateTime ReservationTime { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}