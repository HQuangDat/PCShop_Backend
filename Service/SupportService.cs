using Gridify;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.SupportDtos;
using PCShop_Backend.Dtos.SupportDtos.CreateDtos;
using PCShop_Backend.Dtos.SupportDtos.UpdateDtos;
using PCShop_Backend.Models;

namespace PCShop_Backend.Service
{
    public class SupportService : ISupportService
    {
        private readonly ApplicationDbContext _context;

        public SupportService(ApplicationDbContext context)
        {
            _context = context;
        }

        //--------Support Tickets--------//
        public async Task<Paging<SupportTicketDto>> getTickets(GridifyQuery gridifyQuery)
        {
            var query = await  _context.Tickets
                            .Include(cm => cm.TicketComments)
                            .Select(t => new SupportTicketDto
                            {
                                TicketId = t.TicketId,
                                UserId = t.UserId,
                                Title = t.Title,
                                Description = t.Description,
                                Status = t.Status,
                                Priority = t.Priority,
                                AssignedToUserId = t.AssignedToUserId,
                                UpdatedAt = t.UpdatedAt,
                                Comments = t.TicketComments.Select(tc => new SupportTicketCommentDto
                                {
                                    CommentId = tc.CommentId,
                                    CommentText = tc.CommentText,
                                    CreatedAt = tc.CreatedAt
                                }).ToList()
                            });

            var result = await query.AsQueryable().Gridify(gridifyQuery);
            return result;
        }

        public async Task<SupportTicketDto> getTicketById(int ticketId)
        {
            var ticket = await _context.Tickets
                            .Where(t => t.TicketId == ticketId)
                            .Include(cm => cm.TicketComments)
                            .Select(t => new SupportTicketDto
                            {
                                TicketId = t.TicketId,
                                UserId = t.UserId,
                                Title = t.Title,
                                Description = t.Description,
                                Status = t.Status,
                                Priority = t.Priority,
                                AssignedToUserId = t.AssignedToUserId,
                                UpdatedAt = t.UpdatedAt,
                                Comments = t.TicketComments.Select(tc => new SupportTicketCommentDto
                                {
                                    CommentId = tc.CommentId,
                                    CommentText = tc.CommentText,
                                    CreatedAt = tc.CreatedAt
                                }).ToList()
                            })
                            .FirstOrDefault();
            if (ticket == null)
            {
                throw new Exception("Ticket not found");
            }

            return ticket;
        }

        public async Task CreateSupportTicket(CreateSupportTicketDto dto)
        {
            var newTicket = new Ticket
            {
                Title = dto.Title,
                UserId = 1, // Placeholder for UserId, should be replaced with actual user context
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow,
                AssignedToUserId = null
            };
            
            await _context.Tickets.AddAsync(newTicket);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSupportTicket(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
            {
                throw new Exception("Ticket not found");
            }
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
        }

        //--------Ticket Comments--------//
        public Task<Paging<SupportTicketCommentDto>> getTicketComments(int ticketId, GridifyQuery gridifyQuery)
        {
            throw new NotImplementedException();
        }

        public Task AddTicketComment(int userId, AddSupportTicketCommentDto dto)
        {
            throw new NotImplementedException();
        }
        public Task UpdateTicketComment(int ticketId, int commentId, UpdateSupportTicketCommentDto dto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteTicketComment(int ticketId, int commentId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSupportTicket(int ticketId, UpdateSupportTicketDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
