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
    public class AddressController : GenericController
    {
        private readonly IAddressService _service;

        public AddressController(
            IAddressService service
        )
        {
            _service = service;
        }

        // POST: api/PurchaseOrder
        [HttpPost("district")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<DistrictInfo>>>> GetDistrict([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.GetDistrict(model, page, pageSize);
            var total = await _service.GetDistrictTotal(model);

            return WrapPagingModel<IEnumerable<DistrictInfo>>.Ok(data, total);
        }

        [HttpPost("ward")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<WardInfo>>>> GetWard([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.GetWard(model, page, pageSize);
            var total = await _service.GetWardTotal(model);

            return WrapPagingModel<IEnumerable<WardInfo>>.Ok(data, total);
        }
    }
}
