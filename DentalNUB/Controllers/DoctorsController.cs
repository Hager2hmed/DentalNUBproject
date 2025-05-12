using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.RegularExpressions;
using DentalNUB.Entities.Models;
using DentalNUB.Entities;

namespace DentalNUB.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class DoctorsController : ControllerBase
    {
        private readonly DBContext _context;

        public DoctorsController(DBContext context)
        {
            _context = context;
        }

        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteDoctorProfile([FromBody] CompleteDoctorProfileRequest request)
        {
            // 1. التحقق من التوكن
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid User ID in token");

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != "Doctor")
                return BadRequest("Invalid user or role");

            // 2. التحقق من الـ Clinic
            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.ClinicName == request.ClinicName);
            if (clinic == null)
                return BadRequest("Clinic not found");

            // 3. التحقق من توافق DoctorYear مع AllowedYear
            if (clinic.AllowedYear.HasValue && clinic.AllowedYear != request.DoctorYear)
                return BadRequest($"This clinic is only available for year {clinic.AllowedYear}");

            // 4. إيجاد أو إنشاء ClinicSection
            var existingSections = await _context.ClinicSections
                .Where(s => s.ClinicID == clinic.ClinicID && s.DoctorYear == request.DoctorYear)
                .OrderBy(s => s.SectionName)
                .ToListAsync();

            string sectionName;
            int orderInSection;

            if (!existingSections.Any())
            {
                // أول Section للـ Clinic والسنة دي
                sectionName = $"{clinic.ClinicName} A";
                orderInSection = 1;
            }
            else
            {
                // ابحث عن Section فيها مكان (أقل من 30 طالب)
                var availableSection = existingSections
                    .Select(s => new
                    {
                        Section = s,
                        DoctorCount = _context.Doctors.Count(d => d.SectionID == s.SectionID)
                    })
                    .FirstOrDefault(s => s.DoctorCount < 30);

                if (availableSection != null)
                {
                    sectionName = availableSection.Section.SectionName;
                    orderInSection = availableSection.DoctorCount + 1;
                }
                else
                {
                    // تحقق من عدد الـ Sections
                    if (existingSections.Count >= 3)
                        return BadRequest("Maximum number of sections (A, B, C) reached for this clinic and year");

                    // إنشاء Section جديدة
                    var lastSection = existingSections.Last();
                    var lastLetter = lastSection.SectionName.Split(' ').Last(); // e.g., "A", "B"
                    if (!Regex.IsMatch(lastLetter, @"^[A-C]$"))
                        return BadRequest("Invalid section letter detected");

                    var newLetter = (char)(lastLetter[0] + 1); // A -> B, B -> C
                    if (newLetter > 'C')
                        return BadRequest("Cannot create more sections beyond C");

                    sectionName = $"{clinic.ClinicName} {newLetter}";
                    orderInSection = 1;
                }
            }

            // 5. إيجاد أو إنشاء ClinicSection
            var clinicSection = await _context.ClinicSections
                .FirstOrDefaultAsync(s => s.ClinicID == clinic.ClinicID && s.SectionName == sectionName && s.DoctorYear == request.DoctorYear);

            if (clinicSection == null)
            {
                clinicSection = new ClinicSection
                {
                    ClinicID = clinic.ClinicID,
                    SectionName = sectionName,
                    DoctorYear = request.DoctorYear,
                    MaxStudents = 30
                };
                _context.ClinicSections.Add(clinicSection);
                await _context.SaveChangesAsync();
            }

            // 6. إنشاء الدكتور
            var doctor = new Doctor
            {
                DoctorName = user.FullName,
                DoctorEmail = user.Email,
                DoctorPassword = user.PasswordHash, // Hashed بـ BCrypt من جدول Users
                DoctorPhone = request.DoctorPhone,
                DoctorYear = request.DoctorYear,
                UniversityID = request.UniversityID,
                ClinicID = clinic.ClinicID,
                SectionID = clinicSection.SectionID,
                SectionOrder = orderInSection,
                UserId = user.Id
            };

            _context.Doctors.Add(doctor);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving doctor profile: {ex.Message}");
            }

            // 7. إرجاع الرد
            return Ok(new
            {
                Message = "Doctor profile completed successfully!",
                SectionName = sectionName,
                SectionOrder = orderInSection
            });
        }


        //[Authorize(Roles = "Doctor")]
        [HttpGet("cases")]
        public async Task<IActionResult> GetDoctorCases()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized("User ID is not valid.");

            var doctor = await _context.Doctors
                .Include(d => d.Patientcases)
                    .ThenInclude(c => c.Patient)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor not found.");

            if (doctor.Patientcases == null || !doctor.Patientcases.Any())
                return Ok(new List<CasesDetailsResponse>()); // إرجاع قايمة فاضية لو مفيش Cases

            var response = doctor.Patientcases
                .Select(c => new CasesDetailsResponse
                {
                    CaseID = c.CaseID,
                    PatientName = c.Patient.PatientName,
                    Age = c.Patient.Age,
                    PatPhone = c.Patient.PatPhone,
                    ChronicalDiseases = c.Patient.ChronicalDiseases
                })
                .ToList();

            return Ok(response);
        }

        //[Authorize(Roles = "Doctor")]
        [HttpGet("cases/{caseId}")]
        public async Task<IActionResult> GetCaseDetails(int caseId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized("User ID is not valid.");

            var doctor = await _context.Doctors
                .Include(d => d.Patientcases)
                    .ThenInclude(c => c.Patient)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor not found.");

            var patientCase = doctor.Patientcases
                .FirstOrDefault(c => c.CaseID == caseId);

            if (patientCase == null)
                return NotFound("Case not found or not assigned to this doctor.");

            var response = new CasesDetailsResponse
            {
                CaseID = patientCase.CaseID,
                PatientName = patientCase.Patient.PatientName,
                Age = patientCase.Patient.Age,
                PatPhone = patientCase.Patient.PatPhone,
                ChronicalDiseases = patientCase.Patient.ChronicalDiseases
            };

            return Ok(response);
        }

        [HttpGet("case/{caseId}/diagnosis")]
        public async Task<IActionResult> GetDiagnosisForCase(int caseId)
        {
            // جيب الـ UserId من الـ JWT token عشان نتأكد إن الدكتور هو اللي له صلاحية على الـ PatientCase
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            // تحقق إن الـ PatientCase موجود ومرتبط بالدكتور
            var patientCase = await _context.PatientCases
                .Include(pc => pc.Doctor)
                .FirstOrDefaultAsync(pc => pc.CaseID == caseId && pc.Doctor.UserId == userId);

            if (patientCase == null)
            {
                return NotFound("Case not found or not assigned to this doctor.");
            }

            // جيب الـ Diagnose المرتبط بالـ PatientCase
            var diagnose = await _context.Diagnoses
                .Where(d => d.DiagnoseID == patientCase.DiagnoseID)
                .Select(d => new
                {
                    d.AssignedClinic,
                    d.FinalDiagnose
                })
                .FirstOrDefaultAsync();

            if (diagnose == null)
            {
                return NotFound("No diagnosis found for this case.");
            }

            // ارجع الـ response
            return Ok(new
            {
                AssignedClinic = diagnose.AssignedClinic,
                FinalDiagnose = diagnose.FinalDiagnose
            });
        }

    }
}