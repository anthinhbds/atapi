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
    public class ReportController : GenericController
    {
        private readonly IReportService _service;

        public ReportController(
            IReportService service
        )
        {
            _service = service;
        }

        [HttpGet("getTotalView")]
        public async Task<ActionResult<WrapModel<IEnumerable<ViewStatisticInfo>>>> GetTotalView()
        {
            var data = await _service.GetTotalView();

            return WrapModel<IEnumerable<ViewStatisticInfo>>.Ok(data);
        }
    }
}
