using DentalNUB.Entities.Models;
using DentalNUB.Entities;
using DentalNUB.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static DentalNUB.Entities.Models.Patient;
using Mapster;

namespace DentalNUB.Service;
public class AppointmentsService: IAppointmentsService
{
    private readonly DBContext _context;

    public AppointmentsService(DBContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<IEnumerable<GetAppointmentResponse>>> GetAppointmentsAsync(ClaimsPrincipal user)
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
                ChronicalDiseases = GetChronicDiseasesNamesList(a.Patient.ChronicalDiseases),
                CigarettesPerDay = a.Patient.CigarettesPerDay,
                TeethBrushingFrequency = a.Patient.TeethBrushingFrequency
            }
        }).ToList();

        return new OkObjectResult(result);
    }

    public async Task<ActionResult<GetAppointByIDResponse>> GetPatientDetailsAsync(int appointId, ClaimsPrincipal user)
    {
        var diagnose = await _context.Diagnoses
            .FirstOrDefaultAsync(d => d.AppointID == appointId);

        if (diagnose != null)
        {
            return new BadRequestObjectResult("This appointment is already diagnosed.");
        }

        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.AppointID == appointId);

        if (appointment == null)
        {
            return new NotFoundObjectResult("Appointment not found.");
        }

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

        return new OkObjectResult(result);
    }

    public async Task<ActionResult<AppointmentResponse>> CreateAppointAsync(CreateAppointmentRequest request, ClaimsPrincipal user)
    {
        var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return new UnauthorizedObjectResult("User ID not found in token.");

        int userId = int.Parse(userIdClaim.Value);

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return new BadRequestObjectResult("User not found.");

        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (patient == null)
        {
            patient = request.PatientData.Adapt<Patient>();
            patient.UserId = user.Id;
            _context.Patients.Add(patient);
        }
        else
        {
            patient.PatientName = request.PatientData.PatientName;
            patient.Gender = request.PatientData.Gender;
            patient.PatPhone = request.PatientData.PatPhone;
            patient.NationalID = request.PatientData.NationalID;
            patient.Age = request.PatientData.Age;
            patient.CigarettesPerDay = request.PatientData.CigarettesPerDay;
            patient.TeethBrushingFrequency = request.PatientData.TeethBrushingFrequency;

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

        string chronicDiseasesNames = string.Empty;
        if (!string.IsNullOrEmpty(patient.ChronicalDiseases))
        {
            chronicDiseasesNames = string.Join(", ",
                patient.ChronicalDiseases
                       .Split(',')
                       .Select(int.Parse)
                       .Select(GetChronicDiseaseName));
        }

        var appointment = request.Adapt<Appointment>();
        appointment.BookingType = (int)request.BookingType;
        appointment.PatientID = patient.PatientID;
        appointment.AppointDate = DateTime.Now;

        if (request.XRayImage != null)
        {
            appointment.XRayImage = await SaveXRayImageAsync(request.XRayImage);
        }

        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();

        var queueNumber = await _context.Appointments
            .CountAsync(a => a.AppointDate.Date == appointment.AppointDate.Date && a.AppointID < appointment.AppointID);

        queueNumber += 1;

        var estimatedTime = appointment.AppointDate.Date
            .AddHours(9)
            .AddMinutes((queueNumber - 1) * 15);

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

        return new CreatedAtActionResult("GetPatientDetails", "Appointments", new { appointId = appointment.AppointID }, response);
    }

    private async Task<string> SaveXRayImageAsync(IFormFile xrayImage)
    {
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "XRayImages");

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

