using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.SupportDtos.CreateDtos;
using PCShop_Backend.Dtos.SupportDtos.UpdateDtos;
using PCShop_Backend.Service;
using Serilog;
using System.Threading.Tasks;

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

        // GET: api/Support/tickets

        [HttpGet("tickets")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTickets([FromQuery] GridifyQuery query)
        {
            await _supportService.getTickets(query);
            return Ok();
        }

        [HttpGet("user-tickets")]
        public async Task<IActionResult> GetUserTickets([FromQuery] GridifyQuery query)
        {
            await _supportService.getTicketsForUser(query);
            return Ok();
        }


        [HttpGet("ticket/{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            await _supportService.getTicketById(id);
            Log.Information("Fetched ticket with ID: {TicketId}", id);
            return Ok();
        }

        [HttpPost("supportTicket-create")]
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
        public async Task<IActionResult> DeleteSupportTicket(int id)
        {
            await _supportService.DeleteSupportTicket(id);
            Log.Information("Deleted support ticket with ID: {TicketId}", id);
            return Ok(new { message = "Deleted support ticket success!" });
        }

        //--------- Additional endpoints for support ticket comments can be added here

        [HttpGet("{id}/ticketComments")]
        public async Task<IActionResult> GetAllTicketComments(int ticketId, [FromQuery]GridifyQuery query)
        {
            await _supportService.getTicketComments(ticketId, query);
            return Ok();
        }

        [HttpPost("{id}/ticketComment-create")]
        public async Task<IActionResult> AddTicketComment(int ticketId, [FromBody] AddSupportTicketCommentDto dto)
        {
            await _supportService.AddTicketComment(ticketId, dto);
            Log.Information("Added comment to ticket with ID: {TicketId}", ticketId);
            return Ok(new { message = "Added comment to support ticket success!" });
        }

        [HttpPut("{ticketId}/ticketComment-update/{commentId}")]
        public async Task<IActionResult> UpdateTicketComment(int ticketId, int commentId, [FromBody] UpdateSupportTicketCommentDto dto)
        {
            await _supportService.UpdateTicketComment(ticketId, commentId, dto);
            Log.Information("Updated comment with ID: {CommentId} on ticket with ID: {TicketId}", commentId, ticketId);
            return Ok(new { message = "Updated support ticket comment success!" });
        }

        [HttpDelete("{ticketId}/ticketComment-delete/{commentId}")]
        public async Task<IActionResult> DeleteTicketComment(int ticketId, int commentId)
        {
            await _supportService.DeleteTicketComment(ticketId, commentId);
            Log.Information("Deleted comment with ID: {CommentId} from ticket with ID: {TicketId}", commentId, ticketId);
            return Ok(new { message = "Deleted support ticket comment success!" });
        }
    }
}
