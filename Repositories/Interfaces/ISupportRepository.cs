using PCShop_Backend.Models;

namespace PCShop_Backend.Repositories.Interfaces
{
    public interface ISupportRepository
    {
        // Tickets
        IQueryable<Ticket> QueryTickets();
        Task<Ticket?> GetTicketByIdAsync(int ticketId);
        Task AddTicketAsync(Ticket ticket);
        void RemoveTicket(Ticket ticket);
        void UpdateTicket(Ticket ticket);

        // Comments
        IQueryable<TicketComment> QueryTicketComments();
        Task<TicketComment?> GetCommentAsync(int ticketId, int commentId);
        Task AddCommentAsync(TicketComment comment);
        void RemoveComment(TicketComment comment);
        void UpdateComment(TicketComment comment);

        Task SaveChangesAsync();
    }
}
