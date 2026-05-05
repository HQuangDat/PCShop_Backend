using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Dtos.SupportDtos;
using PCShop_Backend.Dtos.SupportDtos.CreateDtos;
using PCShop_Backend.Dtos.SupportDtos.UpdateDtos;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Interfaces;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;
using Serilog;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PCShop_Backend.Service
{
    public class SupportService : ISupportService
    {
        private readonly ISupportRepository _supportRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheService _cacheService;

        public SupportService(ISupportRepository supportRepository, IHttpContextAccessor httpContextAccessor, ICacheService cacheService)
        {
            _supportRepository = supportRepository;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
        }

        // ========== Support Tickets ==========
        public async Task<Paging<SupportTicketDto>> getTickets(GridifyQuery gridifyQuery)
        {
            var rawKey = $"Tickets_{gridifyQuery.Page}_{gridifyQuery.PageSize}_{gridifyQuery.Filter}_{gridifyQuery.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<SupportTicketDto>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _supportRepository.QueryTickets()
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
                }).GridifyAsync(gridifyQuery);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<Paging<SupportTicketDto>> getTicketsForUser(GridifyQuery gridifyQuery)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var rawKey = $"Tickets_{userId}_{gridifyQuery.Page}_{gridifyQuery.PageSize}_{gridifyQuery.Filter}_{gridifyQuery.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<SupportTicketDto>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _supportRepository.QueryTickets()
                .Where(t => t.UserId == userId)
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
                }).GridifyAsync(gridifyQuery);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<SupportTicketDto> getTicketById(int ticketId)
        {
            var rawKey = $"Ticket_{ticketId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<SupportTicketDto>(key);
            if (cachedData != null)
                return cachedData;

            var ticket = await _supportRepository.QueryTickets()
                .Where(t => t.TicketId == ticketId)
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
                throw new NotFoundException("Ticket not found");

            await _cacheService.SetAsync(key, ticket);
            return ticket;
        }

        public async Task CreateSupportTicket(CreateSupportTicketDto dto)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var newTicket = new Ticket
            {
                Title = dto.Title,
                UserId = userId,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow,
                AssignedToUserId = null
            };

            await _supportRepository.AddTicketAsync(newTicket);
            await _supportRepository.SaveChangesAsync();
        }

        public async Task UpdateSupportTicket(int ticketId, UpdateSupportTicketDto dto)
        {
            var existingTicket = await _supportRepository.GetTicketByIdAsync(ticketId);
            if (existingTicket == null)
            {
                Log.Information("Ticket with ID {TicketId} not found.", ticketId);
                throw new NotFoundException("Ticket not found");
            }

            existingTicket.Title = dto.Title;
            existingTicket.Description = dto.Description;
            existingTicket.Status = dto.Status;
            existingTicket.Priority = dto.Priority;
            _supportRepository.UpdateTicket(existingTicket);
            await _supportRepository.SaveChangesAsync();

            var rawKey = $"Ticket_{ticketId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        public async Task DeleteSupportTicket(int ticketId)
        {
            var ticket = await _supportRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new NotFoundException("Ticket not found");

            _supportRepository.RemoveTicket(ticket);
            await _supportRepository.SaveChangesAsync();

            var rawKey = $"Ticket_{ticketId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        // ========== Ticket Comments ==========
        public async Task<Paging<SupportTicketCommentDto>> getTicketComments(int ticketId, GridifyQuery gridifyQuery)
        {
            var rawKey = $"TicketComments_{ticketId}_{gridifyQuery.Page}_{gridifyQuery.PageSize}_{gridifyQuery.Filter}_{gridifyQuery.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<SupportTicketCommentDto>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _supportRepository.QueryTicketComments()
                .Where(tc => tc.TicketId == ticketId)
                .Select(tc => new SupportTicketCommentDto
                {
                    CommentId = tc.CommentId,
                    TicketId = tc.TicketId,
                    UserId = tc.UserId,
                    CommentText = tc.CommentText,
                    CreatedAt = tc.CreatedAt
                }).GridifyAsync(gridifyQuery);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task AddTicketComment(int ticketId, AddSupportTicketCommentDto dto)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var addComment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                CommentText = dto.CommentText,
                CreatedAt = DateTime.UtcNow
            };
            await _supportRepository.AddCommentAsync(addComment);
            await _supportRepository.SaveChangesAsync();

            var rawKey = $"Ticket_{ticketId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        public async Task UpdateTicketComment(int ticketId, int commentId, UpdateSupportTicketCommentDto dto)
        {
            var existingComment = await _supportRepository.GetCommentAsync(ticketId, commentId);
            if (existingComment == null)
            {
                Log.Error("Comment {CommentId} for ticket {TicketId} not found.", commentId, ticketId);
                throw new NotFoundException("Comment not found");
            }

            existingComment.CommentText = dto.CommentText;
            _supportRepository.UpdateComment(existingComment);
            await _supportRepository.SaveChangesAsync();

            var rawKey = $"Ticket_{ticketId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        public async Task DeleteTicketComment(int ticketId, int commentId)
        {
            var existingComment = await _supportRepository.GetCommentAsync(ticketId, commentId);
            if (existingComment == null)
            {
                Log.Error("Comment {CommentId} for ticket {TicketId} not found.", commentId, ticketId);
                throw new NotFoundException("Comment not found");
            }

            _supportRepository.RemoveComment(existingComment);

            var rawKey = $"Ticket_{ticketId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);

            await _supportRepository.SaveChangesAsync();
        }
    }
}
