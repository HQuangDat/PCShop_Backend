using Gridify;
using PCShop_Backend.Dtos.SupportDtos;
using PCShop_Backend.Dtos.SupportDtos.CreateDtos;
using PCShop_Backend.Dtos.SupportDtos.UpdateDtos;

namespace PCShop_Backend.Service
{
    public interface ISupportService
    {
        //--------Support Tickets--------//
        Task<Paging<SupportTicketDto>> getTickets(GridifyQuery gridifyQuery);
        Task<SupportTicketDto> getTicketById(int ticketId);
        Task CreateSupportTicket(CreateSupportTicketDto dto);
        Task UpdateSupportTicket(int ticketId, UpdateSupportTicketDto dto);
        Task DeleteSupportTicket(int ticketId);

        //--------Ticket Comments--------//
        Task<Paging<SupportTicketCommentDto>> getTicketComments(int ticketId, GridifyQuery gridifyQuery);
        Task AddTicketComment(int ticketId, AddSupportTicketCommentDto dto);
        Task UpdateTicketComment(int ticketId, int commentId, UpdateSupportTicketCommentDto dto);
        Task DeleteTicketComment(int ticketId, int commentId);

    }
}
