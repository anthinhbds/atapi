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
    public class HistoryController : GenericController
    {
        private readonly IHistoryService _service;

        public HistoryController(
            IHistoryService service
        )
        {
            _service = service;
        }

        [HttpPost()]
        public async Task<ActionResult<WrapPagingModel<HistoryInfo>>> Add([FromBody] HistoryInfo model)
        {
            var data = await _service.Add(model);
            return WrapPagingModel<HistoryInfo>.Ok(data, 0);
        }

        [HttpPost("get")]
        public async Task<ActionResult<WrapModel<IEnumerable<HistoryInfo>>>> Get(HistoryParamsModel model)
        {
            var data = await _service.Get(model);

            return WrapModel<IEnumerable<HistoryInfo>>.Ok(data);
        }
    }
}
