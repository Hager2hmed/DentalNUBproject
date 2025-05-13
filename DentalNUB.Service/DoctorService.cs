using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DentalNUB.Entities;
using DentalNUB.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DentalNUB.Interface;

namespace DentalNUB.Service;
public class DoctorService : IDoctorService
{
    private readonly DBContext _context;

    public DoctorService(DBContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> CompleteDoctorProfileAsync(CompleteDoctorProfileRequest request, ClaimsPrincipal user)
    {
        var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return new UnauthorizedResult();

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return new UnauthorizedResult();

        var currentUser = await _context.Users.FindAsync(userId);
        if (currentUser == null || currentUser.Role != "Doctor")
            return new BadRequestObjectResult("Invalid user or role");

        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.ClinicName == request.ClinicName);
        if (clinic == null)
            return new BadRequestObjectResult("Clinic not found");

        if (clinic.AllowedYear.HasValue && clinic.AllowedYear != request.DoctorYear)
            return new BadRequestObjectResult($"This clinic is only available for year {clinic.AllowedYear}");

        var existingSections = await _context.ClinicSections
            .Where(s => s.ClinicID == clinic.ClinicID && s.DoctorYear == request.DoctorYear)
            .OrderBy(s => s.SectionName)
            .ToListAsync();

        string sectionName;
        int orderInSection;

        if (!existingSections.Any())
        {
            sectionName = $"{clinic.ClinicName} A";
            orderInSection = 1;
        }
        else
        {
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
                if (existingSections.Count >= 3)
                    return new BadRequestObjectResult("Maximum number of sections (A, B, C) reached for this clinic and year");

                var lastSection = existingSections.Last();
                var lastLetter = lastSection.SectionName.Split(' ').Last();
                if (!Regex.IsMatch(lastLetter, @"^[A-C]$"))
                    return new BadRequestObjectResult("Invalid section letter detected");

                var newLetter = (char)(lastLetter[0] + 1);
                if (newLetter > 'C')
                    return new BadRequestObjectResult("Cannot create more sections beyond C");

                sectionName = $"{clinic.ClinicName} {newLetter}";
                orderInSection = 1;
            }
        }

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

        var doctor = new Doctor
        {
            DoctorName = currentUser.FullName,
            DoctorEmail = currentUser.Email,
            DoctorPhone = request.DoctorPhone,
            DoctorYear = request.DoctorYear,
            UniversityID = request.UniversityID,
            ClinicID = clinic.ClinicID,
            SectionID = clinicSection.SectionID,
            SectionOrder = orderInSection,
            UserId = currentUser.Id
        };

        _context.Doctors.Add(doctor);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new ObjectResult($"Error saving doctor profile: {ex.Message}")
            {
                StatusCode = 500
            };
        }

        return new OkObjectResult(new
        {
            Message = "Doctor profile completed successfully!",
            SectionName = sectionName,
            SectionOrder = orderInSection
        });
    }

    public async Task<IActionResult> GetDoctorCasesAsync(ClaimsPrincipal user)
    {
        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdString, out int userId))
            return new UnauthorizedObjectResult("User ID is not valid.");

            var doctor = await _context.Doctors
        .Include(d => d.Patientcases)!
            .ThenInclude(c => c.Patient)
        .FirstOrDefaultAsync(d => d.UserId == userId);

        if (doctor == null)
            return new NotFoundObjectResult("Doctor not found.");

        if (doctor.Patientcases == null || !doctor.Patientcases.Any())
            return new OkObjectResult(new List<CasesDetailsResponse>()); // إرجاع قايمة فاضية لو مفيش Cases

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

        return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetCaseDetailsAsync(int caseId, ClaimsPrincipal user)
    {
        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdString, out int userId))
            return new UnauthorizedObjectResult("User ID is not valid.");

        var doctor = await _context.Doctors
            .Include(d => d.Patientcases)!
                .ThenInclude(c => c.Patient)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (doctor == null)
            return new NotFoundObjectResult("Doctor not found.");

        var patientCase = doctor.Patientcases
            .FirstOrDefault(c => c.CaseID == caseId);

        if (patientCase == null)
            return new NotFoundObjectResult("Case not found or not assigned to this doctor.");

        var response = new CasesDetailsResponse
        {
            CaseID = patientCase.CaseID,
            PatientName = patientCase.Patient.PatientName,
            Age = patientCase.Patient.Age,
            PatPhone = patientCase.Patient.PatPhone,
            ChronicalDiseases = patientCase.Patient.ChronicalDiseases
        };

        return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetDiagnosisForCaseAsync(int caseId, ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return new UnauthorizedObjectResult("Invalid user ID.");
        }

        var patientCase = await _context.PatientCases
            .Include(pc => pc.Doctor)
            .FirstOrDefaultAsync(pc => pc.CaseID == caseId && pc.Doctor.UserId == userId);

        if (patientCase == null)
        {
            return new NotFoundObjectResult("Case not found or not assigned to this doctor.");
        }

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
            return new NotFoundObjectResult("No diagnosis found for this case.");
        }

        return new OkObjectResult(new
        {
            AssignedClinic = diagnose.AssignedClinic,
            FinalDiagnose = diagnose.FinalDiagnose
        });
    }
}

