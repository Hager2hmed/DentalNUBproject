//using DentalNUB.Api.Controllers;
//using DentalNUB.Api.Data;
//using DentalNUB.Api.Entities;

//namespace DentalNUB.Api.Services;

//public interface UserService : IUserService
//{
//    private readonly DBContext _context;

//    public UserService(DBContext context)
//    {
//        _context = context;
//    }
//    public async Task<Patient> GetPatientProfileAsync(int patientId)
//    {
//        return await _context.Patients.FindAsync(patientId);
//    }

//    // Method to get Doctor account details
//    public async Task<Doctor> GetDoctorProfileAsync(int doctorId)
//    {
//        return await _context.Doctors.FindAsync(doctorId);
//    }

//    // Method to get Consultant account details
//    public async Task<Consultant> GetConsultantProfileAsync(int consultantId)
//    {
//        return await _context.Consultants.FindAsync(consultantId);
//    }

//    // تحديث بيانات المريض
//    public async Task<bool> UpdatePatientProfileAsync(UpdatePatientProfileRequest request)
//    {
//        var patient = await _context.Patients.FindAsync(request.PatientID);
//        if (patient == null) return false;

//        patient.PatientName = request.PatientName;
//        patient.PatPhone = request.PatPhone;
//        patient.NationalID = request.NationalID;
//        patient.Gender = request.Gender;
//        patient.Age = request.Age;
//        patient.ChronicalDiseases = request.ChronicalDiseases;

//        _context.Patients.Update(patient);
//        await _context.SaveChangesAsync();
//        return true;
//    }

//    // تحديث بيانات الطبيب
//    public async Task<bool> UpdateDoctorProfileAsync(UpdateDoctorProfileRequest request)
//    {
//        var doctor = await _context.Doctors.FindAsync(request.DoctorID);
//        if (doctor == null) return false;

//        doctor.DoctorName = request.DoctorName;
//        doctor.DoctorEmail = request.DoctorEmail;
//        doctor.DoctorPhone = request.DoctorPhone;
//        doctor.DoctorYear = request.DoctorYear;
//        doctor.UniversityID = request.UniversityID;

//        _context.Doctors.Update(doctor);
//        await _context.SaveChangesAsync();
//        return true;
//    }

//    // تحديث بيانات الاستشاري
//    public async Task<bool> UpdateConsultantProfileAsync(UpdateConsultantProfileRequest request)
//    {
//        var consultant = await _context.Consultants.FindAsync(request.ConsID);
//        if (consultant == null) return false;

//        consultant.ConsName = request.ConsName;
//        consultant.ConsEmail = request.ConsEmail;
//        consultant.ConsPhone = request.ConsPhone;
//        consultant.Specialty = request.Specialty;
//        consultant.ClinicType = request.ClinicType;

//        _context.Consultants.Update(consultant);
//        await _context.SaveChangesAsync();
//        return true;
//    }
//}

