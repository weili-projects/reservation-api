using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Timers;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ReservationApi.Data;
using ReservationApi.DTOs;
using ReservationApi.Models;
using ReservationApi.Services;
using ReservationApi.Services.Interfaces;
using ReservationApi.Utils;

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly ILogger<AvailabilityController> _logger;

        public AvailabilityController(IAvailabilityService availabilityService, ILogger<AvailabilityController> logger)
        {
            _availabilityService = availabilityService;
            _logger = logger;
        }

        // POST: api/availability
        // only Provider role can do it
        [HttpPost]
        public async Task<ActionResult<List<SlotDTO>>> CreateAvailability([FromBody] AvailabilityRequestDTO request)
        {
            try
            {
                List<(DateTime, DateTime)> datetimeRanges = request.AvailabilityRanges
                    .Select(dtoRange => (dtoRange.StartTime, dtoRange.EndTime))
                    .ToList();
                
                var result = await _availabilityService.CreateAvailability(request.ProviderId, datetimeRanges);
                
                var formattedResult = result.Select(a => new SlotDTO { AvailabilityId = a.Id, StartTime = a.StartTime, EndTime = a.EndTime }).ToList();

                return result.Count > 0 ? StatusCode(201, formattedResult) : NotFound("No available slot created.");
            }
            catch (ReservationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in getting creating: {ErrorMsg}", ex.Message);
                return NotFound("Exception in creating availability.");
            }
        }

        // GET: api/availability/1
        [HttpGet("{pid}")]
        public async Task<ActionResult<List<SlotDTO>>> GetAvailability(int pid)
        {
            try
            {
                var result = await _availabilityService.GetAvailability(pid);

                if (result.Count == 0)
                {
                    return NoContent();
                }
                
                var formattedResult = result.Select(a => new SlotDTO { AvailabilityId = a.Id, StartTime = a.StartTime, EndTime = a.EndTime }).ToList();

                return Ok(formattedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in getting availability: {ErrorMsg}", ex.Message);
                return NotFound("Exception in getting availability.");
            }
        }


    }
}