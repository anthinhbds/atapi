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
    public class HomeController : GenericController
    {
        private readonly IHomeService _service;

        public HomeController(
            IHomeService service
        )
        {
            _service = service;
        }

        // POST: api/PurchaseOrder
        [HttpPost("revenueMonthly")]
        public async Task<ActionResult<WrapModel<IEnumerable<MonthlyRevenueByUserInfo>>>> GetRevenueMonthly([FromBody] MonthlyRevenueByUserModel model)
        {
            var data = await _service.GetRevenueMonthly(model);

            return WrapModel<IEnumerable<MonthlyRevenueByUserInfo>>.Ok(data);
        }
    }
}
