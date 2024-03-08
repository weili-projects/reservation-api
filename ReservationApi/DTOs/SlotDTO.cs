namespace ReservationApi.DTOs
{
    public class SlotDTO
    {
        public int AvailabilityId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}