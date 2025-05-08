using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using atmnr_api.Models;
using atmnr_api.Services;

namespace atmnr_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : GenericController
    {
        private readonly INotificationService _service;

        public NotificationController(
            INotificationService service
        )
        {
            _service = service;
        }

        // POST: api/PurchaseOrder
        [HttpGet()]
        public async Task<ActionResult<WrapModel<IEnumerable<NotificationInfo>>>> Get()
        {
            var data = await _service.Get();

            return WrapModel<IEnumerable<NotificationInfo>>.Ok(data);
        }
        [HttpGet("badgetCount")]
        public async Task<ActionResult<WrapModel<int>>> GetBadgetCount()
        {
            var count = await _service.GetTotal();

            return WrapModel<int>.Ok(count);
        }

        [HttpPost("markRead/{id}")]
        public async Task<ActionResult<WrapModel<bool>>> MarkRead(Guid id)
        {
            var data = await _service.MarkRead(id);

            return WrapModel<bool>.Ok(data);
        }

    }
}
