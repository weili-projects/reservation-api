using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using ReservationApi.Data;
using ReservationApi.Models;
using ReservationApi.Services.Interfaces;
using ReservationApi.Utils;

namespace ReservationApi.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentService> _logger;

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
            _logger.LogDebug("MakeReservation starts: availabilityId: {availabilityId}, clientId: {clientId}", availabilityId, clientId);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var availability = await _context.Availabilities.FindAsync(availabilityId);
                    var client = await _context.Clients.FindAsync(clientId);
                    if (availability == null || client == null)
                    {
                        string msg = "Slot or client not found.";
                        _logger.LogWarning("{msg}", msg);
                        throw new ReservationException(msg);
                    }

                    DateTime currentTime = DateTime.Now;

                    if (availability.StartTime < currentTime.AddHours(24))
                    {
                        string msg = "reservations must be mde at least 24 hours in advance.";
                        _logger.LogWarning("{msg}", msg);
                        throw new ReservationException(msg);
                    }

                    var existingAppointment = await _context.Appointments
                        .Where(a => a.AvailabilityId == availabilityId && a.ClientId == clientId && (a.IsConfirmed || a.ExpirationTime > currentTime))
                        .FirstOrDefaultAsync();
                    if (existingAppointment != null)
                    {
                        string msg = "The slot is unavailable due to that is confirmed or pending for confirmation.";
                        _logger.LogWarning("{msg}", msg);
                        throw new ReservationException(msg);
                    }

                    var appointment = new Appointment
                    {
                        AvailabilityId = availabilityId,
                        ClientId = clientId,
                        ReservationTime = currentTime,
                        ExpirationTime = currentTime.AddMinutes(30),
                        IsConfirmed = false,
                        Availability = availability,
                        Client = client
                    };

                    await _context.Appointments.AddAsync(appointment);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    
                    // after the commit, try retriving the new appointment
                    var newAppointment = await _context.Appointments
                        .Include(a => a.Availability)
                        .Include(a => a.Client)
                        .SingleOrDefaultAsync(a => a.Id == appointment.Id);

                    if (newAppointment == null)
                    {
                        string msg = "Appointment not saved.";
                        _logger.LogWarning("{msg}", msg);
                        throw new ReservationException(msg);
                    }

                    _logger.LogDebug("MakeReservation result: appointment: {appointment}", appointment.Id);

                    return newAppointment;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception in creating appointment: {ErrorMsg}", ex.Message);
                    await transaction.RollbackAsync();
                    throw;
                }
            }

        }

        /// <summary>
        ///     Implementing ConfirmReservation to confirm the appointment
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        public async Task<Appointment> ConfirmReservation(int appointmentId)
        {
            _logger.LogDebug("ConfirmReservation starts: appointmentId: {appointmentId}", appointmentId);

            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                {
                    string msg = "Appointment not found.";
                    _logger.LogWarning("{msg}", msg);
                    throw new ReservationException(msg);
                }

                if (appointment.IsConfirmed)
                {
                    string msg = "Already confirmed.";
                    _logger.LogWarning("{msg}", msg);
                    throw new ReservationException(msg);
                }

                if (appointment.ExpirationTime < DateTime.Now)
                {
                    string msg = "Reservation has expired. Make a new application.";
                    _logger.LogWarning("{msg}", msg);
                    throw new ReservationException(msg);
                }

                appointment.IsConfirmed = true;
                await _context.SaveChangesAsync();


                // default is lazy loading, need eager loading here so that Availability and Client are not null, otherwise object reference exception 
                //var updatedAppointment = await _context.Appointments.FindAsync(appointmentId);
                var updatedAppointment = await _context.Appointments
                    .Include(a => a.Availability)
                    .Include(a => a.Client)
                    .SingleOrDefaultAsync(a => a.Id == appointmentId);


                if (updatedAppointment == null || !updatedAppointment.IsConfirmed)
                {
                    string msg = "Confirmation failed.";
                    _logger.LogWarning("{msg}", msg);
                    throw new ReservationException(msg);
                }

                _logger.LogDebug("ConfirmReservation result: confirmed");
                return updatedAppointment;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in confirming appointment: {ErrorMsg}", ex.Message);
                throw;
            }
        }
    }
}