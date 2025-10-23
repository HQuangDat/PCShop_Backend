namespace PCShop_Backend.Dtos.SupportDtos.UpdateDtos
{
    public class UpdateSupportTicketDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public string? Priority { get; set; }
    }
}
