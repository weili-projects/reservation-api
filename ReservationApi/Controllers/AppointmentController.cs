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
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(IAppointmentService appointmentService, ILogger<AppointmentController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        // POST: api/appointment
        [HttpPost]
        public async Task<ActionResult<ReservationDTO>> MakeReservation([FromBody] AppointmentRequestDTO request)
        {
            try
            {
                var app = await _appointmentService.MakeReservation(request.AvailabilityId, request.ClientId);
                
                var formattedResult = new ReservationDTO { AppointmentId = app.Id, AvailabilityId = app.AvailabilityId, AppointmentTime = app.Availability.StartTime, 
                    ClientId = app.ClientId, ClientName = app.Client.Name, IsConfirm = app.IsConfirmed, ReservationTime = app.ReservationTime, ExpirationTime = app.ExpirationTime };

                return app == null ?  BadRequest("No appointment created.") : StatusCode(201, formattedResult);
            }
            catch (ReservationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {   
                _logger.LogError(ex, "Exception in making reservation: {ErrorMsg}", ex.Message);
                return StatusCode(500, "Exception in making reservation.");
            }        
        }
        
        
        // PATCH: api/availability/2
        [HttpPatch("{appointmentId}")]
        public async Task<ActionResult<ReservationDTO>> ConfirmReservation(int appointmentId)
        {
            try
            {
                var app = await _appointmentService.ConfirmReservation(appointmentId);

                var formattedResult = new ReservationDTO { AppointmentId = app.Id, AvailabilityId = app.AvailabilityId, AppointmentTime = app.Availability.StartTime, 
                    ClientId = app.ClientId, ClientName = app.Client.Name, IsConfirm = app.IsConfirmed, ReservationTime = app.ReservationTime, ExpirationTime = app.ExpirationTime };
                
                _logger.LogDebug("ConfirmReservation ctrl 3");

                return Ok(formattedResult);
            }
            catch (ReservationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in confirming reservation: {ErrorMsg}", ex.Message);
                return StatusCode(500, "Exception in confirming reservation.");
            } 
        }
        
    }
}