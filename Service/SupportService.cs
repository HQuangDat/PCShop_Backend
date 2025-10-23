using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.SupportDtos;
using PCShop_Backend.Dtos.SupportDtos.CreateDtos;
using PCShop_Backend.Dtos.SupportDtos.UpdateDtos;
using PCShop_Backend.Models;
using Serilog;
using System.ComponentModel.Design;

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
            var query = _context.Tickets
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

            var result = await query.GridifyAsync(gridifyQuery);
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
                            .FirstOrDefaultAsync();
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
        public async Task UpdateSupportTicket(int ticketId, UpdateSupportTicketDto dto)
        {
            var existingTicket =  await _context.Tickets.FindAsync(ticketId);
            if (existingTicket == null)
            {
                Log.Information("Ticket with ID {TicketId} not found.", ticketId);
                throw new Exception("Ticket not found");
            }
            existingTicket.Title = dto.Title;
            existingTicket.Description = dto.Description;
            existingTicket.Status = dto.Status;
            existingTicket.Priority = dto.Priority;
            _context.Tickets.Update(existingTicket);
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
        public async Task<Paging<SupportTicketCommentDto>> getTicketComments(int ticketId, GridifyQuery gridifyQuery)
        {
            var CommentsQuery = _context.TicketComments
                                .Where(td => td.TicketId == ticketId)
                                .Select(tc => new SupportTicketCommentDto
                                {
                                    CommentId = tc.CommentId,
                                    TicketId = tc.TicketId,
                                    UserId = tc.UserId,
                                    CommentText = tc.CommentText,
                                    CreatedAt = tc.CreatedAt
                                });
            var result = await CommentsQuery.GridifyAsync(gridifyQuery);
            return result;
        }

        public async Task AddTicketComment(int ticketId,AddSupportTicketCommentDto dto)
        {
            var addComment = new TicketComment
            {
                TicketId = ticketId,
                UserId = 1, // Placeholder for UserId, should be replaced with actual user context
                CommentText = dto.CommentText,
                CreatedAt = DateTime.UtcNow
            };
            await _context.TicketComments.AddAsync(addComment);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateTicketComment(int ticketId, int commentId, UpdateSupportTicketCommentDto dto)
        {
            var existingComment = await _context.TicketComments
                                       .FirstOrDefaultAsync(tc => tc.TicketId == ticketId && tc.CommentId == commentId);
            if(existingComment == null)
            {
                Log.Error("Comment with ID {CommentId} for Ticket ID {TicketId} not found.", commentId, ticketId);
                throw new Exception("Comment not found");
            }

            existingComment.CommentText = dto.CommentText;
            _context.TicketComments.Update(existingComment);
            await _context.SaveChangesAsync();
        }

        public Task DeleteTicketComment(int ticketId, int commentId)
        {
            var existingComment =  _context.TicketComments
                                       .FirstOrDefault(tc => tc.TicketId == ticketId && tc.CommentId == commentId);
            if (existingComment == null)
            {
                Log.Error("Comment with ID {CommentId} for Ticket ID {TicketId} not found.", commentId, ticketId);
                throw new Exception("Comment not found");
            }

            _context.TicketComments.Remove(existingComment);
            return _context.SaveChangesAsync();
        }


    }
}
