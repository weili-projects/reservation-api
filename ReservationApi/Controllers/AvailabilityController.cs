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

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly ILogger<AvailabilityController> _logger;
        private const string TypeName = "AvailabilityController";

        public AvailabilityController(IAvailabilityService availabilityService, ILogger<AvailabilityController> logger)
        {
            _availabilityService = availabilityService;
            _logger = logger;
        }

        // POST: api/availability
        // only Provider role can do it
        [HttpPost]
        public async Task<ActionResult<Availability>> CreateAvailability([FromBody] AvailabilityRequestDTO request)
        {
            try
            {
                var result = await _availabilityService.CreateAvailability(request.ProviderId, request.AvailabilityRanges);

                return result.Count > 0 ? StatusCode(201, result) : BadRequest("No available slot created.");
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TypeName}] Exception in getting creating: {ErrorMsg}", TypeName, ex.Message);
                return StatusCode(500, "Exception in creating availability.");
            }
        }

        // GET: api/availability/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Availability>> GetAvailability(int id)
        {
            try
            {
                var result = await _availabilityService.GetAvailability(id);
                return result.Count > 0 ? Ok(result) : NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TypeName}] Exception in getting availability: {ErrorMsg}", TypeName, ex.Message);
                return StatusCode(500, "Exception in getting availability.");
            }
        }


    }
}