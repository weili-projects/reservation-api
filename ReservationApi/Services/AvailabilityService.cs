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
        public async Task<List<Availability>> CreateAvailability(int providerId, IEnumerable<(DateTime, DateTime)> availabilityRanges)
        {
            _logger.LogDebug("CreateAvailability starts: providerId: {providerId}", providerId);
            foreach(var range in availabilityRanges) 
            {
                _logger.LogDebug("range: {start} - {end}", range.Item1, range.Item2);
            }

            try
            {
                var provider = await _context.Providers.FindAsync(providerId);
                if (provider == null)
                {
                    string msg = "Provider not found";
                    _logger.LogWarning("{msg}", msg);
                    throw new ReservationException(msg);
                }

                
                HashSet<DateTime> existingSlots = new HashSet<DateTime>();
                List<Availability> existingAvailabilities = await GetAvailability(providerId);
                foreach(var slot in existingAvailabilities)
                {
                    existingSlots.Add(slot.StartTime);
                }


                var validAvailabilities = new List<Availability>();

                foreach (var range in availabilityRanges)
                {
                    DateTime rangeStartTime = range.Item1;
                    DateTime rangeEndTime = range.Item2;
                    // Ideally the sanitize check should be checked on the client side 
                    if (!Helpers.IsValidTime(rangeStartTime) || !Helpers.IsValidTime(rangeEndTime) || rangeEndTime < DateTime.Now)
                    {
                        _logger.LogDebug("CreateAvailability: skipping range {start} - {end}", rangeStartTime, rangeEndTime);
                        continue;
                    }

                    DateTime slotStartTime = rangeStartTime;
                    
                    for (;(rangeEndTime - slotStartTime).TotalMinutes >= 15 && slotStartTime > DateTime.Now; slotStartTime = slotStartTime.AddMinutes(15))
                    {
                        // if a slot has been added, skip it
                        if (existingSlots.Contains(slotStartTime))
                        {
                            _logger.LogDebug("CreateAvailability: slot with start time {start} already exists for provider id: {id}", slotStartTime, providerId);
                            continue;
                        }

                        DateTime slotEndTime = slotStartTime.AddMinutes(15);
                        validAvailabilities.Add(new Availability
                        {
                            ProviderId = providerId,
                            StartTime = slotStartTime,
                            EndTime = slotEndTime,
                            Provider = provider
                        });
                        existingSlots.Add(slotStartTime);
                    }
                }

                _logger.LogDebug("CreateAvailability: validAvailabilities count: {count}", validAvailabilities.Count);
            
                if (validAvailabilities.Count > 0)
                {
                    await _context.AddRangeAsync(validAvailabilities);
                    await _context.SaveChangesAsync();
                    
                    foreach(var a in validAvailabilities) 
                    {
                        _logger.LogDebug("CreateAvailability result: slot availability id: {id}, provider id: {pid}, start: {start}, end: {end}", a.Id, a.ProviderId, a.StartTime, a.EndTime);    
                    }
                }

                return validAvailabilities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in creating availability: {ErrorMsg}", ex.Message);
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
            _logger.LogDebug("GetAvailability starts: providerId: {providerId}", providerId);
            
            try
            {
                // don't show the past availability (for an availability less than 24 hours ahead, still show it but can't reserve it)
                // don't show the availability that is in appointments which is confirmed or expiration date not reach
                DateTime currentTime = DateTime.Now;
                var result = await _context.Availabilities
                    .Where(a => a.ProviderId == providerId &&
                        a.StartTime > currentTime &&
                        !_context.Appointments
                            .Any(app => app.AvailabilityId == a.Id && (app.IsConfirmed || app.ExpirationTime > currentTime)))
                    .ToListAsync();

                _logger.LogDebug("GetAvailability result: {result}", String.Join(",", result));
            
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in getting availability: {ErrorMsg}", ex.Message);
                throw;
            }
        }

    }
}