using DentalNUB.Entities;
using DentalNUB.Entities.Models;
using DentalNUB.Interface;
using Microsoft.EntityFrameworkCore;

namespace DentalNUB.Services
{
    public class DiagnoseService : IDiagnoseService
    {
        private readonly DBContext _context;

        public DiagnoseService(DBContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateDiagnoseAsync(CreateDiagnoseRequest request)
        {
            var patientCase = await _context.PatientCases.FindAsync(request.CaseID);
            if (patientCase == null)
                return false;

            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.ClinicName.ToString() == request.AssignedClinic);
            if (clinic == null)
                return false;

            var diagnose = new Diagnose
            {
                ConsID = request.ConsID,
                AssignedClinic = request.AssignedClinic,
                FinalDiagnose = request.FinalDiagnose
            };

            _context.Diagnoses.Add(diagnose);
            await _context.SaveChangesAsync();

            patientCase.DiagnoseID = diagnose.DiagnoseID;
            patientCase.ClinicID = clinic.ClinicID;
            patientCase.ConsID = request.ConsID;
            patientCase.CaseStatus = "AssignedToClinic";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Diagnose>> GetAllDiagnosesAsync()
        {
            return await _context.Diagnoses.ToListAsync();
        }

        public async Task<Diagnose?> GetDiagnoseByIdAsync(int id)
        {
            return await _context.Diagnoses.FindAsync(id);
        }

        public async Task<bool> UpdateDiagnoseAsync(int id, CreateDiagnoseRequest request)
        {
            var diagnose = await _context.Diagnoses.FindAsync(id);
            if (diagnose == null)
                return false;

            diagnose.ConsID = request.ConsID;
            diagnose.AssignedClinic = request.AssignedClinic;
            diagnose.FinalDiagnose = request.FinalDiagnose;

            _context.Diagnoses.Update(diagnose);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDiagnoseAsync(int id)
        {
            var diagnose = await _context.Diagnoses.FindAsync(id);
            if (diagnose == null)
                return false;

            _context.Diagnoses.Remove(diagnose);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
