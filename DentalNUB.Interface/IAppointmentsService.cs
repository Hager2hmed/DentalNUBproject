using DentalNUB.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DentalNUB.Interface;
public interface IAppointmentsService
{
    Task<ActionResult<IEnumerable<GetAppointmentResponse>>> GetAppointmentsAsync(ClaimsPrincipal user);
    Task<ActionResult<GetAppointByIDResponse>> GetPatientDetailsAsync(int appointId, ClaimsPrincipal user);
    Task<ActionResult<AppointmentResponse>> CreateAppointAsync(CreateAppointmentRequest request, ClaimsPrincipal user);
}
