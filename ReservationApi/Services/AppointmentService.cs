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
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentService> _logger;

        private const string TypeName = "AppointmentService";

        public AppointmentService(ApplicationDbContext context, ILogger<AppointmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        ///     Implementing MakeReservation to make appointment
        /// </summary>
        /// <param name="availabilityId"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<Appointment> MakeReservation(int availabilityId, int clientId)
        {
            _logger.LogDebug("[{TypeName}] MakeReservation starts: availabilityId: {availabilityId}, clientId: {clientId}", TypeName, availabilityId, clientId);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var availability = await _context.Availabilities.FindAsync(availabilityId);
                    var client = await _context.Clients.FindAsync(clientId);
                    if (availability == null || client == null)
                    {
                        string msg = "null availability or client";
                        _logger.LogWarning("[{TypeName}] {msg}", TypeName, msg);
                        throw new ApplicationException(msg);
                    }

                    if (availability.StartTime < DateTime.Now.AddHours(24))
                    {
                        string msg = "reservations must be mde at least 24 hours in advance.";
                        _logger.LogWarning("[{TypeName}] {msg}", TypeName, msg);
                        throw new ApplicationException(msg);
                    }

                    var appointment = new Appointment
                    {  
                        AvailabilityId = availabilityId,
                        ClientId = clientId,
                        ReservationTime = DateTime.Now,
                        IsConfirmed = false,
                        Availability = availability,
                        Client = client
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    
                    _logger.LogDebug("[{TypeName}] MakeReservation result: appointment: {appointment}", TypeName, String.Join(",", appointment));

                    return appointment;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{TypeName}] Exception in creating appointment: {ErrorMsg}", TypeName, ex.Message);
                    throw;
                }
            }
            
        }

        /// <summary>
        ///     Implementing ConfirmReservation to confirm the appointment
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        public async Task ConfirmReservation(int appointmentId)
        {
            _logger.LogDebug("[{TypeName}] ConfirmReservation starts: appointmentId: {appointmentId}", TypeName, appointmentId);

            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                {
                    string msg = "Appointment not found.";
                    _logger.LogWarning("{TypeName} - {msg}", TypeName, msg);
                    throw new ApplicationException(msg);
                }

                if (appointment.ExpirationTime < DateTime.Now)
                {
                    string msg = "Reservation has expired.";
                    _logger.LogWarning("{TypeName} - {msg}", TypeName, msg);
                    throw new ApplicationException(msg);
                }

                appointment.IsConfirmed = true;
                await _context.SaveChangesAsync();

                _logger.LogDebug("[{TypeName}] ConfirmReservation result: confirmed", TypeName);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TypeName}] Exception in confirming appointment: {ErrorMsg}", TypeName, ex.Message);
                throw;
            }
        }
    }
}