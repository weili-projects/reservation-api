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
                var result = await _availabilityService.CreateAvailability(request.ProviderId, request.AvailabilityRanges);
                
                var formattedResult = result.Select(a => new SlotDTO { AvailabilityId = a.Id, StartTime = a.StartTime, EndTime = a.EndTime }).ToList();

                return result.Count > 0 ? StatusCode(201, formattedResult) : BadRequest("No available slot created.");
            }
            catch (ReservationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in getting creating: {ErrorMsg}", ex.Message);
                return StatusCode(500, "Exception in creating availability.");
            }
        }

        // GET: api/availability/1
        [HttpGet("{id}")]
        public async Task<ActionResult<List<SlotDTO>>> GetAvailability(int id)
        {
            try
            {
                var result = await _availabilityService.GetAvailability(id);
                
                var formattedResult = result.Select(a => new SlotDTO { AvailabilityId = a.Id, StartTime = a.StartTime, EndTime = a.EndTime }).ToList();

                return result.Count > 0 ? Ok(formattedResult) : NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in getting availability: {ErrorMsg}", ex.Message);
                return StatusCode(500, "Exception in getting availability.");
            }
        }


    }
}