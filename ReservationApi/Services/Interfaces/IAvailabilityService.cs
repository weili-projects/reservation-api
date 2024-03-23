using System.Collections.Generic;
using System.Threading.Tasks;

using ReservationApi.Models;

namespace ReservationApi.Services.Interfaces
{
    public interface IAvailabilityService
    {
        Task<List<Availability>> CreateAvailability(int providerId, IEnumerable<(DateTime, DateTime)> availabilityRanges);

        Task<List<Availability>> GetAvailability(int providerId);

        // further consideration: Update, delete, to achieve more CRUD actions on availability.
    }
}