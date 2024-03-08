using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using ReservationApi.Data;
using ReservationApi.DTOs;
using ReservationApi.Models;
using ReservationApi.Services.Interfaces;
using ReservationApi.Utils;

namespace ReservationApi.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AvailabilityService> _logger;

        private const string TypeName = "AvailabilityService";

        public AvailabilityService(ApplicationDbContext context, ILogger<AvailabilityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        ///     Implementing CreateAvailability
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="availabilityRanges"></param>
        /// <returns>
        ///     A list of availabile slots that are just created
        /// </returns>
        /// <remarks>
        ///     Assumption: the ranges may have overlap of the existing slots, in this case, only create the non-overlap ones.
        ///     the creation rule depends on business needs
        /// </remarks>
        public async Task<List<Availability>> CreateAvailability(int providerId, IEnumerable<AvailabilityRangeDTO> availabilityRanges)
        {
            try
            {
                var provider = await _context.Providers.FindAsync(providerId);
                if (provider == null)
                {
                    string msg = "Provider not found";
                    _logger.LogWarning("{TypeName} - {msg}", TypeName, msg);
                    throw new ApplicationException(msg);
                }

                
                var validAvailabilities = new List<Availability>();

                foreach (var range in availabilityRanges)
                {
                    // Ideally the sanitize check should be checked on the client side 
                    // check overlap
                    if (!Helpers.IsValidTime(range.StartTime) || !Helpers.IsValidTime(range.EndTime) || range.EndTime < DateTime.Now)
                    {
                        // log it
                        continue;
                    }

                    DateTime slotStartTime = range.StartTime;

                    while ((slotStartTime - range.EndTime).TotalMinutes >= 15 && slotStartTime > DateTime.Now)
                    {
                        DateTime slotEndTime = slotStartTime.AddMinutes(15);
                        validAvailabilities.Add(new Availability
                        {
                            ProviderId = providerId,
                            StartTime = slotStartTime,
                            EndTime = slotEndTime,
                            Provider = provider
                        });
                        slotStartTime = slotEndTime;
                    }
                }
                if (validAvailabilities.Count > 0)
                {
                    await _context.AddRangeAsync(validAvailabilities);
                    await _context.SaveChangesAsync();
                }

                // can be zero or non-zero count
                return validAvailabilities;
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "[{TypeName}] Exception in creating availability: {ErrorMsg}", TypeName, ex.Message);
                throw;
            }

        }

        /// <summary>
        ///     Implementing GetAvailability to get availability by providerId
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Further consideration: get all availability by all providers, get avilability by date, etc
        /// </remarks>
        public async Task<List<Availability>> GetAvailability(int providerId)
        {
            try
            {
                // don't show the past availability (for an availability less than 24 hours ahead, still show it but can't reserve it)
                // don't show the availability that is in appointments which is confirmed or expiration date not reach
                DateTime currentTime = DateTime.Now;
                return await _context.Availabilities
                    .Where(a => a.ProviderId == providerId &&
                        a.StartTime > currentTime &&
                        !_context.Appointments
                            .Any(app => app.AvailabilityId == a.Id && (app.IsConfirmed || app.ExpirationTime > currentTime)))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TypeName}] Exception in getting availability: {ErrorMsg}", TypeName, ex.Message);
                throw;
            }
        }

    }
}