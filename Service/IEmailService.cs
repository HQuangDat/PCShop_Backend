namespace PCShop_Backend.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
