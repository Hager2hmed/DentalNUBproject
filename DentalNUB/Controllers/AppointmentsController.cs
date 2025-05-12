using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel;
using System.Security.Claims;
using DentalNUB.Entities.Models;
using DentalNUB.Entities;
using static DentalNUB.Entities.Models.Patient;
using Mapster;

namespace DentalNUB.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly DBContext _context;

        public AppointmentsController(DBContext context)
        {
            _context = context;
        }

        // GET: api/Appointments
        [HttpGet("GetAppointments")]
        //[Authorize(Roles = "Consultant")]
        public async Task<ActionResult<IEnumerable<GetAppointmentResponse>>> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => !_context.Diagnoses.Any(d => d.AppointID == a.AppointID))
                .ToListAsync();

            var result = appointments.Select(a => new GetAppointmentResponse
            {
                Complaint = a.Complaint,
                XRayImage = a.XRayImage,
                CreatePatientRequest = new CreatePatientRequest
                {
                    PatientName = a.Patient.PatientName,
                    PatPhone = a.Patient.PatPhone,
                    NationalID = a.Patient.NationalID,
                    Gender = a.Patient.Gender,
                    Age = a.Patient.Age,
                    // هنا بنحول الستركنج لأسماء مفهومة
                    ChronicalDiseases = GetChronicDiseasesNamesList(a.Patient.ChronicalDiseases),
                    CigarettesPerDay = a.Patient.CigarettesPerDay,
                    TeethBrushingFrequency = a.Patient.TeethBrushingFrequency
                }
            }).ToList();

            return Ok(result);
        }
        private List<string> GetChronicDiseasesNamesList(string? diseasesCsv)
        {
            if (string.IsNullOrWhiteSpace(diseasesCsv))
                return new List<string>();

            return diseasesCsv
                .Split(',')
                .Select(int.Parse)
                .Select(GetChronicDiseaseName)
                .ToList();
        }


        [HttpGet("GetPatientDetails/{appointId}")]
        //[Authorize(Roles = "Consultant")]
        public async Task<ActionResult<GetAppointByIDResponse>> GetPatientDetails(int appointId)
        {
            var diagnose = await _context.Diagnoses
                .FirstOrDefaultAsync(d => d.AppointID == appointId);

            if (diagnose != null)
            {
                return BadRequest("This appointment is already diagnosed.");
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointID == appointId);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // تحويل الأمراض المزمنة من CSV لأسماء باستخدام الدالة المساعدة
            var chronicDiseasesList = GetChronicDiseasesNamesList(appointment.Patient?.ChronicalDiseases);

            var result = new GetAppointByIDResponse
            {
                PatientDetails = new CreatePatientRequest
                {
                    PatientName = appointment.Patient?.PatientName,
                    PatPhone = appointment.Patient?.PatPhone,
                    NationalID = appointment.Patient?.NationalID,
                    Gender = appointment.Patient?.Gender,
                    Age = appointment.Patient?.Age ?? 0,
                    ChronicalDiseases = chronicDiseasesList,
                    CigarettesPerDay = appointment.Patient?.CigarettesPerDay,
                    TeethBrushingFrequency = appointment.Patient?.TeethBrushingFrequency
                },
                Complaint = appointment.Complaint,
                XRayImage = appointment.XRayImage
            };

            return Ok(result);
        }


        private async Task<string> SaveXRayImageAsync(IFormFile xrayImage)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "XRayImages");

            // لو المجلد مش موجود هننشئه
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }


            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(xrayImage.FileName);
            var filePath = Path.Combine(folderPath, fileName);


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await xrayImage.CopyToAsync(stream);
            }


            var xrayUrl = $"{Request.Scheme}://{Request.Host}/XRayImages/{fileName}";
            return xrayUrl;
        }



        // POST: api/Appointments
        [HttpPost("CreateAppoint")]
        public async Task<ActionResult<AppointmentResponse>> CreateAppoint([FromForm] CreateAppointmentRequest request)
        {
            // نجيب الـ user اللي سجل قبلي كده
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            int userId = int.Parse(userIdClaim.Value);

            // نجيب الـ user اللي سجل قبلي كده
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return BadRequest("User not found.");
            // نحاول نجيب المريض المرتبط بالـ UserID
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                // لو مش موجود، ننشئ واحد جديد
                patient = request.PatientData.Adapt<Patient>();
                patient.UserId = user.Id;
                _context.Patients.Add(patient);
            }
            else
            {
                // تحديث بيانات المريض الموجود
                patient.PatientName = request.PatientData.PatientName;
                patient.Gender = request.PatientData.Gender;
                patient.PatPhone = request.PatientData.PatPhone;
                patient.NationalID = request.PatientData.NationalID;
                patient.Age = request.PatientData.Age;
                patient.CigarettesPerDay = request.PatientData.CigarettesPerDay;
                patient.TeethBrushingFrequency = request.PatientData.TeethBrushingFrequency;

                // معالجة الأمراض المزمنة: تحويل List<string> لـ string "0,2,3" بعد ما نحول الاسم للـ enum value
                if (request.PatientData.ChronicalDiseases != null && request.PatientData.ChronicalDiseases.Any())
                {
                    var diseaseIds = request.PatientData.ChronicalDiseases
                        .Select(name => GetEnumValueFromName(name.Trim()))
                        .Where(val => val.HasValue)
                        .Select(val => val.Value.ToString());

                    patient.ChronicalDiseases = string.Join(",", diseaseIds);
                }
            }

            await _context.SaveChangesAsync();

            // تحويل الأمراض المزمنة إلى أسماء مفهومة باستخدام الديسكريبشن
            string chronicDiseasesNames = string.Empty;
            if (!string.IsNullOrEmpty(patient.ChronicalDiseases))
            {
                chronicDiseasesNames = string.Join(", ",
                    patient.ChronicalDiseases
                           .Split(',')
                           .Select(int.Parse)
                           .Select(GetChronicDiseaseName));
            }

            // إنشاء الحجز وربطه بالمريض
            var appointment = request.Adapt<Appointment>();
            appointment.BookingType = (int)request.BookingType;
            appointment.PatientID = patient.PatientID;

            appointment.AppointDate = DateTime.Now;

            // حفظ صورة الأشعة إن وُجدت
            if (request.XRayImage != null)
            {
                appointment.XRayImage = await SaveXRayImageAsync(request.XRayImage);
            }

            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            // حساب رقم الدور
            var queueNumber = await _context.Appointments
                .CountAsync(a => a.AppointDate.Date == appointment.AppointDate.Date && a.AppointID < appointment.AppointID);

            queueNumber += 1;

            // حساب الوقت التقديري للقدوم
            var estimatedTime = appointment.AppointDate.Date
                .AddHours(9)
                .AddMinutes((queueNumber - 1) * 15);

            // إعداد الريسبونس النهائي
            var response = new AppointmentResponse(appointment)
            {
                AppointmentId = appointment.AppointID,
                PatientName = patient.PatientName,
                AppointmentDate = appointment.AppointDate,
                QueueNumber = queueNumber,
                EstimatedTime = estimatedTime.ToString("hh:mm tt"),
                ChronicalDiseases = chronicDiseasesNames,
                Message = $"Your reservation has been completed successfully. Your Queue number is {queueNumber} at {estimatedTime:hh:mm tt}"
            };

            return CreatedAtAction("GetPatientDetails", new { appointId = appointment.AppointID }, response);
        }

        private int? GetEnumValueFromName(string diseaseName)
        {
            foreach (var value in Enum.GetValues(typeof(ChronicDisease)))
            {
                var field = typeof(ChronicDisease).GetField(value.ToString());
                var description = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                       .FirstOrDefault() as DescriptionAttribute;

                if (description != null && description.Description.Equals(diseaseName, StringComparison.OrdinalIgnoreCase))
                {
                    return (int)value;
                }
            }

            return null;
        }

        private string GetChronicDiseaseName(int chronicDiseaseValue)
        {
            var disease = (ChronicDisease)chronicDiseaseValue;
            var fieldInfo = disease.GetType().GetField(disease.ToString());
            var descriptionAttribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                                .FirstOrDefault() as DescriptionAttribute;
            return descriptionAttribute?.Description ?? disease.ToString();
        }

    }
}