
using DentalNUB.Entities.Models;

namespace DentalNUB.Interface
{

    public interface ICaseDistributionService
    {
        Task<Doctor?> DistributeCaseToDoctorAsync(string assignedClinicName);
    }
}