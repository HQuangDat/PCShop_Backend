using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Service;

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
    }
}
