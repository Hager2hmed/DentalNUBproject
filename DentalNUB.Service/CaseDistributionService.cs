using DentalNUB.Entities;
using DentalNUB.Entities.Models;
using DentalNUB.Interface;
using Microsoft.EntityFrameworkCore;

namespace DentalNUB.Service
{
    public class CaseDistributionService : ICaseDistributionService
    {
        private readonly DBContext _context;

        public CaseDistributionService(DBContext context)
        {
            _context = context;
        }

        public async Task<Doctor?> DistributeCaseToDoctorAsync(string assignedClinicName)
        {
            // 1. جلب العيادة بالاسم
            var clinic = await _context.Clinics
                .Include(c => c.ClinicSections) // جلب الأقسام التابعة للعيادة
                .ThenInclude(cs => cs.Doctors)  // ثم جلب الدكاترة المرتبطين بكل قسم
                .FirstOrDefaultAsync(c => c.ClinicName == assignedClinicName);

            if (clinic == null || clinic.ClinicSections == null || !clinic.ClinicSections.Any())
                return null;

            // 2. جلب الدكاترة في العيادة مرتبين بالسنة ثم الترتيب في السيكشن
            var sortedDoctors = clinic.ClinicSections
                .SelectMany(cs => cs.Doctors)   // الحصول على كل الدكاترة في كل الأقسام
                .OrderBy(d => d.DoctorYear)
                .ThenBy(d => d.SectionOrder)    // ترتيب الدكاترة حسب ترتيبهم داخل القسم
                .ToList();

            // 3. نبحث عن أول دكتور مشغّل عدد قليل من الحالات (مثلاً أقل من 5 حالات)
            foreach (var doctor in sortedDoctors)
            {
                var doctorCasesCount = await _context.PatientCases
                    .CountAsync(pc => pc.DoctorID == doctor.DoctorID && pc.CaseStatus != "Completed");

                if (doctorCasesCount < 5) // العدد المسموح لكل دكتور في نفس الوقت
                    return doctor;
            }

            // مفيش دكتور متاح
            return null;
        }

    }

}