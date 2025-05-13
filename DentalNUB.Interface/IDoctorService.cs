using DentalNUB.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DentalNUB.Interface;
public interface IDoctorService
{
    Task<IActionResult> CompleteDoctorProfileAsync(CompleteDoctorProfileRequest request, ClaimsPrincipal user);
    Task<IActionResult> GetDoctorCasesAsync(ClaimsPrincipal user);
    Task<IActionResult> GetCaseDetailsAsync(int caseId, ClaimsPrincipal user);
    Task<IActionResult> GetDiagnosisForCaseAsync(int caseId, ClaimsPrincipal user);
}
