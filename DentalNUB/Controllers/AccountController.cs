//using Microsoft.AspNetCore.Mvc;

//namespace DentalNUB.Api.Controllers;

//[Route("api/[controller]")]
//[ApiController]
//public class AccountController : ControllerBase
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    //public class AccountController : ControllerBase
//    //{
//    //    private readonly IUserService _userService;

//    //    public AccountController(IUserService userService)
//    //    {
//    //        _userService = userService;
//    //    }

//        // Endpoint to get patient profile
//        [HttpGet("getPatientProfile/{patientId}")]
//        public async Task<IActionResult> GetPatientProfile(int patientId)
//        {
//            var patient = await _userService.GetPatientProfileAsync(patientId);
//            if (patient == null) return NotFound("Patient not found");
//            return Ok(patient);
//        }

//        // Endpoint to get doctor profile
//        [HttpGet("getDoctorProfile/{doctorId}")]
//        public async Task<IActionResult> GetDoctorProfile(int doctorId)
//        {
//            var doctor = await _userService.GetDoctorProfileAsync(doctorId);
//            if (doctor == null) return NotFound("Doctor not found");
//            return Ok(doctor);
//        }

//        // Endpoint to get consultant profile
//        [HttpGet("getConsultantProfile/{consultantId}")]
//        public async Task<IActionResult> GetConsultantProfile(int consultantId)
//        {
//            var consultant = await _userService.GetConsultantProfileAsync(consultantId);
//            if (consultant == null) return NotFound("Consultant not found");
//            return Ok(consultant);
//        }

//        // Existing update endpoints
//        [HttpPut("updatePatientProfile")]
//        public async Task<IActionResult> UpdatePatientProfile([FromBody] UpdatePatientProfileRequest request)
//        {
//            var result = await _userService.UpdatePatientProfileAsync(request);
//            if (!result) return BadRequest("Failed to update patient profile");
//            return Ok("Patient profile updated successfully");
//        }

//        [HttpPut("updateDoctorProfile")]
//        public async Task<IActionResult> UpdateDoctorProfile([FromBody] UpdateDoctorProfileRequest request)
//        {
//            var result = await _userService.UpdateDoctorProfileAsync(request);
//            if (!result) return BadRequest("Failed to update doctor profile");
//            return Ok("Doctor profile updated successfully");
//        }

//        [HttpPut("updateConsultantProfile")]
//        public async Task<IActionResult> UpdateConsultantProfile([FromBody] UpdateConsultantProfileRequest request)
//        {
//            var result = await _userService.UpdateConsultantProfileAsync(request);
//            if (!result) return BadRequest("Failed to update consultant profile");
//            return Ok("Consultant profile updated successfully");
//        }
//    }