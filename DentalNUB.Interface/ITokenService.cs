using DentalNUB.Entities;
using DentalNUB.Entities.Models;

namespace DentalNUB.Interface
{

    public interface ITokenService
    {
        Task<string> CreateToken(User user);
    }
}