using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel;
using System.Security.Claims;
using DentalNUB.Entities.Models;
using DentalNUB.Entities;
using static DentalNUB.Entities.Models.Patient;
using Mapster;
using DentalNUB.Interface;

namespace DentalNUB.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentsService _appointmentsService;

        public AppointmentsController(IAppointmentsService appointmentsService)
        {
            _appointmentsService = appointmentsService;

        }

        // GET: api/Appointments
        [HttpGet("GetAppointments")]
        [Authorize(Roles = "Consultant")]
        public async Task<ActionResult<IEnumerable<GetAppointmentResponse>>> GetAppointments()
        {
            return await _appointmentsService.GetAppointmentsAsync(User);
        }

        [HttpGet("GetPatientDetails/{appointId}")]
        [Authorize(Roles = "Consultant")]
        public async Task<ActionResult<GetAppointByIDResponse>> GetPatientDetails(int appointId)
        {
            return await _appointmentsService.GetPatientDetailsAsync(appointId, User);
        }

        [HttpPost("CreateAppoint")]
        public async Task<ActionResult<AppointmentResponse>> CreateAppoint([FromForm] CreateAppointmentRequest request)
        {
            return await _appointmentsService.CreateAppointAsync(request, User);
        }
    }
}