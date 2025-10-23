namespace PCShop_Backend.Dtos.SupportDtos.CreateDtos
{
    public class CreateSupportTicketDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Priority { get; set; }
    }
}