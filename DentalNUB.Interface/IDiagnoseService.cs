

using DentalNUB.Entities;
using DentalNUB.Entities.Models;

namespace DentalNUB.Interface
{
    public interface IDiagnoseService
    {
        Task<bool> CreateDiagnoseAsync(CreateDiagnoseRequest request);
        Task<IEnumerable<Diagnose>> GetAllDiagnosesAsync();
        Task<Diagnose?> GetDiagnoseByIdAsync(int id);
        Task<bool> UpdateDiagnoseAsync(int id, CreateDiagnoseRequest request);
        Task<bool> DeleteDiagnoseAsync(int id);
    }
}