using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DentalNUB.Entities;
using DentalNUB.Interface;

namespace DentalNUB.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Doctor")]
    public class DoctorsController : ControllerBase
    {
      
        private readonly IDoctorService _doctorService;

        public DoctorsController( IDoctorService doctorService)
        {
       
            _doctorService = doctorService;
        }


        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteDoctorProfile([FromBody] CompleteDoctorProfileRequest request)
        {
            return await _doctorService.CompleteDoctorProfileAsync(request, User);
        }

        //[Authorize(Roles = "Doctor")]
        [HttpGet("cases")]
        public async Task<IActionResult> GetDoctorCases()
        {
            return await _doctorService.GetDoctorCasesAsync(User);
        }


        //[Authorize(Roles = "Doctor")]
        [HttpGet("cases/{caseId}")]
        public async Task<IActionResult> GetCaseDetails(int caseId)
        {
            return await _doctorService.GetCaseDetailsAsync(caseId, User);
        }

        [HttpGet("case/{caseId}/diagnosis")]
        public async Task<IActionResult> GetDiagnosisForCase(int caseId)
        {
            return await _doctorService.GetDiagnosisForCaseAsync(caseId, User);
        }
    }
}