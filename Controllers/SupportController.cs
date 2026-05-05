using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.SupportDtos.CreateDtos;
using PCShop_Backend.Dtos.SupportDtos.UpdateDtos;
using PCShop_Backend.Interfaces;
using Serilog;

namespace PCShop_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportController : ControllerBase
    {
        private readonly ISupportService _supportService;

        public SupportController(ISupportService supportService)
        {
            _supportService = supportService;
        }

        [HttpGet("tickets")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTickets([FromQuery] GridifyQuery query)
        {
            var result = await _supportService.getTickets(query);
            return Ok(result);
        }

        [HttpGet("user-tickets")]
        [Authorize]
        public async Task<IActionResult> GetUserTickets([FromQuery] GridifyQuery query)
        {
            var result = await _supportService.getTicketsForUser(query);
            return Ok(result);
        }

        [HttpGet("ticket/{id}")]
        [Authorize]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var result = await _supportService.getTicketById(id);
            Log.Information("Fetched ticket with ID: {TicketId}", id);
            return Ok(result);
        }

        [HttpPost("supportTicket-create")]
        [Authorize]
        public async Task<IActionResult> CreateSupportTicket([FromBody] CreateSupportTicketDto dto)
        {
            await _supportService.CreateSupportTicket(dto);
            Log.Information("Created new support ticket!");
            return Ok(new { message = "Created new support ticket success!" });
        }

        [HttpPut("supportTicket-update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSupportTicket(int id, [FromBody] UpdateSupportTicketDto dto)
        {
            await _supportService.UpdateSupportTicket(id, dto);
            Log.Information("Updated support ticket with ID: {TicketId}", id);
            return Ok(new { message = "Updated support ticket success!" });
        }

        [HttpDelete("supportTicket-delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteSupportTicket(int id)
        {
            await _supportService.DeleteSupportTicket(id);
            Log.Information("Deleted support ticket with ID: {TicketId}", id);
            return Ok(new { message = "Deleted support ticket success!" });
        }

        [HttpGet("{id}/ticketComments")]
        [Authorize]
        public async Task<IActionResult> GetAllTicketComments(int id, [FromQuery] GridifyQuery query)
        {
            var result = await _supportService.getTicketComments(id, query);
            return Ok(result);
        }

        [HttpPost("{id}/ticketComment-create")]
        [Authorize]
        public async Task<IActionResult> AddTicketComment(int id, [FromBody] AddSupportTicketCommentDto dto)
        {
            await _supportService.AddTicketComment(id, dto);
            Log.Information("Added comment to ticket with ID: {TicketId}", id);
            return Ok(new { message = "Added comment to support ticket success!" });
        }

        [HttpPut("{id}/ticketComment-update/{commentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateTicketComment(int id, int commentId, [FromBody] UpdateSupportTicketCommentDto dto)
        {
            await _supportService.UpdateTicketComment(id, commentId, dto);
            Log.Information("Updated comment with ID: {CommentId} on ticket with ID: {TicketId}", commentId, id);
            return Ok(new { message = "Updated support ticket comment success!" });
        }

        [HttpDelete("{id}/ticketComment-delete/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteTicketComment(int id, int commentId)
        {
            await _supportService.DeleteTicketComment(id, commentId);
            Log.Information("Deleted comment with ID: {CommentId} from ticket with ID: {TicketId}", commentId, id);
            return Ok(new { message = "Deleted support ticket comment success!" });
        }
    }
}
