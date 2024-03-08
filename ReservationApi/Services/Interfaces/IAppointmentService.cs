using System.Collections.Generic;
using System.Threading.Tasks;

using ReservationApi.Models;

namespace ReservationApi.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<Appointment> MakeReservation(int availabilityId, int clientId);

        Task<Appointment> ConfirmReservation(int appointmentId);

        // further consideratin: cancel appointment
    }
}