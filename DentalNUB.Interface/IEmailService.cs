namespace DentalNUB.Interface
{
    public interface IEmailService
    {
        Task SendVerificationEmail(string email, string code);
    }
}