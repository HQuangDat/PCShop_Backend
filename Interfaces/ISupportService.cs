using Gridify;
using PCShop_Backend.Dtos.SupportDtos;
using PCShop_Backend.Dtos.SupportDtos.CreateDtos;
using PCShop_Backend.Dtos.SupportDtos.UpdateDtos;

namespace PCShop_Backend.Interfaces
{
    public interface ISupportService
    {
        Task<Paging<SupportTicketDto>> getTickets(GridifyQuery gridifyQuery);
        Task<Paging<SupportTicketDto>> getTicketsForUser(GridifyQuery gridifyQuery);
        Task<SupportTicketDto> getTicketById(int ticketId);
        Task CreateSupportTicket(CreateSupportTicketDto dto);
        Task UpdateSupportTicket(int ticketId, UpdateSupportTicketDto dto);
        Task DeleteSupportTicket(int ticketId);

        Task<Paging<SupportTicketCommentDto>> getTicketComments(int ticketId, GridifyQuery gridifyQuery);
        Task AddTicketComment(int ticketId, AddSupportTicketCommentDto dto);
        Task UpdateTicketComment(int ticketId, int commentId, UpdateSupportTicketCommentDto dto);
        Task DeleteTicketComment(int ticketId, int commentId);
    }
}
