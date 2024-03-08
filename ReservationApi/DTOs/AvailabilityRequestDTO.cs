namespace ReservationApi.DTOs
{
    public class AvailabilityRequestDTO
    {
        public int ProviderId { get; set; }
        public required List<AvailabilityRangeDTO> AvailabilityRanges { get; set; }
    }
}