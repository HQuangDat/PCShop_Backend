using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;

namespace PCShop_Backend.Repositories
{
    public class SupportRepository : ISupportRepository
    {
        private readonly ApplicationDbContext _context;

        public SupportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Ticket> QueryTickets()
        {
            return _context.Tickets.Include(t => t.TicketComments);
        }

        public async Task<Ticket?> GetTicketByIdAsync(int ticketId)
        {
            return await _context.Tickets.FindAsync(ticketId);
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
        }

        public void RemoveTicket(Ticket ticket)
        {
            _context.Tickets.Remove(ticket);
        }

        public void UpdateTicket(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
        }

        public IQueryable<TicketComment> QueryTicketComments()
        {
            return _context.TicketComments;
        }

        public async Task<TicketComment?> GetCommentAsync(int ticketId, int commentId)
        {
            return await _context.TicketComments
                .FirstOrDefaultAsync(tc => tc.TicketId == ticketId && tc.CommentId == commentId);
        }

        public async Task AddCommentAsync(TicketComment comment)
        {
            await _context.TicketComments.AddAsync(comment);
        }

        public void RemoveComment(TicketComment comment)
        {
            _context.TicketComments.Remove(comment);
        }

        public void UpdateComment(TicketComment comment)
        {
            _context.TicketComments.Update(comment);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
