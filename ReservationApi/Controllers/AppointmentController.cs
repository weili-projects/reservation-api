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
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentController> _logger;
        
        private const string TypeName = "AppointmentController";

        public AppointmentController(IAppointmentService appointmentService, ILogger<AppointmentController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        // reserve
        // should lock
        // should trigger the scheduler
        // should send email
        // need to be 24 hour earlier
        [HttpPost]
        public async Task<ActionResult<Appointment>> MakeReservation([FromBody] ReservationDTO request)
        {
            try
            {
                var app = await _appointmentService.MakeReservation(request.AvailabilityId, request.ClientId);
                return app == null ?  BadRequest("No appointment created.") : StatusCode(201, app);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {   
                _logger.LogError(ex, "[{TypeName}] Exception in making reservation: {ErrorMsg}", TypeName, ex.Message);
                return StatusCode(500, "Exception in making reservation.");
            }        
        }
        
        // confirm
        // should check permission
        // should lock
        // should deactivate the scheduler
        // should send the email at the end
        [HttpPatch("{appointmentId}")]
        public async Task<IActionResult> ConfirmReservation(int appointmentId)
        {
            try
            {
                await _appointmentService.ConfirmReservation(appointmentId);
                return Ok();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TypeName}] Exception in confirming reservation: {ErrorMsg}", TypeName, ex.Message);
                return StatusCode(500, "Exception in confirming reservation.");
            } 
        }
        
    }
}