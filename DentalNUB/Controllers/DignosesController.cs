using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DentalNUB.Entities.Models;
using DentalNUB.Interface;
using DentalNUB.Entities;
using Mapster;

namespace DentalNUB.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosesController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ICaseDistributionService _caseDistributionService;

        public DiagnosesController(DBContext context, ICaseDistributionService caseDistributionService)
        {
            _context = context;
            _caseDistributionService = caseDistributionService;
        }

        [HttpGet("AllDiagnoses")]
        public async Task<IActionResult> GetAllDiagnoses()
        {
            var all = await _context.Diagnoses
                .Select(d => new { d.DiagnoseID, d.AppointID, d.FinalDiagnose })
                .ToListAsync();

            return Ok(all);
        }



        // GET: api/Diagnoses/5
        [HttpGet("GetDiagnose/{id}")]
        public async Task<ActionResult<DignoseResponse>> GetDiagnose(int id)
        {
            var diagnose = await _context.Diagnoses
                .Include(d => d.appointment)
                .Include(d => d.clinic)
                .Include(d => d.Consultant)
                .Include(d => d.Cases)
                .FirstOrDefaultAsync(d => d.DiagnoseID == id);

            if (diagnose == null)
                return NotFound();

            var response = new DignoseResponse
            {
                DiagnoseID = diagnose.DiagnoseID,
                FinalDiagnose = diagnose.FinalDiagnose,
                AssignedClinic = diagnose.AssignedClinic,
                ClinicName = diagnose.clinic?.ClinicName ?? "",
                ConsultantName = diagnose.Consultant?.User?.FullName ?? "",
                AppointmentDate = diagnose.appointment?.AppointDate ?? DateTime.MinValue,
                CaseIDs = diagnose.Cases.Select(c => c.CaseID).ToList()
            };

            return Ok(response);
        }



        // POST: api/Diagnoses
        [HttpPost("CreateDiagnose")]
        //[Authorize(Roles = "Consultant")]
        public async Task<IActionResult> CreateDiagnose([FromBody] DiagnoseRequest request)
        {
            // ✅ جلب الإيميل من التوكن
            var consEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (consEmail == null)
                return Unauthorized("البريد الإلكتروني غير موجود في التوكن.");

            // ✅ البحث عن الـ Consultant باستخدام الإيميل
            var consultant = await _context.Consultants
                .FirstOrDefaultAsync(c => c.ConsEmail == consEmail);

            if (consultant == null)
                return Unauthorized("الـ Consultant غير موجود في النظام.");

            int consID = consultant.ConsID;

            // ✅ التحقق من التشخيص السابق
            bool isDiagnosed = await _context.Diagnoses
                .AnyAsync(d => d.AppointID == request.AppointID);

            if (isDiagnosed)
                return BadRequest("This appointment has already been diagnosed.");

            // ✅ تحويل اسم العيادة لـ ClinicID
            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.ClinicName == request.AssignedClinic);

            if (clinic == null)
                return BadRequest("العيادة غير موجودة.");

            // ✅ عمل التشخيص
            var diagnose = request.Adapt<Diagnose>();
            diagnose.ConsID = consID;
            diagnose.ClinicID = clinic.ClinicID;

            _context.Diagnoses.Add(diagnose);
            await _context.SaveChangesAsync();

            // ✅ باقي الإجراءات
            var patientId = await _context.Appointments
                .Where(a => a.AppointID == request.AppointID)
                .Select(a => a.PatientID)
                .FirstOrDefaultAsync();

            var newCase = new PatientCase
            {
                PatientID = patientId,
                DiagnoseID = diagnose.DiagnoseID,
                ClinicID = diagnose.ClinicID,
                ConsID = consID,
                CaseStatus = "New",
                StartDate = DateTime.UtcNow
            };

            _context.PatientCases.Add(newCase);
            await _context.SaveChangesAsync();


            var assignedDoctor = await _caseDistributionService.DistributeCaseToDoctorAsync(request.AssignedClinic);

            if (assignedDoctor != null)
            {
                // ✅ ربط الحالة بالدكتور
                newCase.DoctorID = assignedDoctor.DoctorID;
                _context.PatientCases.Update(newCase);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "تم التشخيص وإنشاء الكيس بنجاح ✅",
                assignedDoctorName = assignedDoctor?.DoctorName // ممكن تستخدمها لو عايزة
            });



        }
    }
}

