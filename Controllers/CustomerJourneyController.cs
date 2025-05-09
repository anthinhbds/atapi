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
    public class CustomerJourneyController : GenericController
    {
        private readonly ICustomerJourneyService _service;

        public CustomerJourneyController(
            ICustomerJourneyService service
        )
        {
            _service = service;
        }

        // POST: api/PurchaseOrder
        [HttpPost("query")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<CustomerJourneyInfo>>>> Query([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.Query(model, page, pageSize);
            var total = await _service.QueryTotal(model);

            return WrapPagingModel<IEnumerable<CustomerJourneyInfo>>.Ok(data, total);
        }

        [HttpPost("queryPlanned")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<CustomerJourneyInfo>>>> QueryPlanned([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryPlanned(model, page, pageSize);
            var total = await _service.QueryPlannedTotal(model);

            return WrapPagingModel<IEnumerable<CustomerJourneyInfo>>.Ok(data, total);
        }

        [HttpPost("queryMet")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<CustomerJourneyInfo>>>> QueryMet([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryMet(model, page, pageSize);
            var total = await _service.QueryMetTotal(model);

            return WrapPagingModel<IEnumerable<CustomerJourneyInfo>>.Ok(data, total);
        }

        [HttpPost("queryOther")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<CustomerJourneyInfo>>>> QueryOther([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryOther(model, page, pageSize);
            var total = await _service.QueryOtherTotal(model);

            return WrapPagingModel<IEnumerable<CustomerJourneyInfo>>.Ok(data, total);
        }

        [HttpPost("getSummary")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<CustomerJourneySummaryInfo>>>> GetSumByStatus([FromBody] QueryParamModel model)
        {
            var data = await _service.GetSummary(model);

            return WrapPagingModel<IEnumerable<CustomerJourneySummaryInfo>>.Ok(data, 0);
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<WrapModel<IEnumerable<CustomerJourneyDetInfo>>>> GetDetail(string id)
        {
            var data = await _service.GetDetails(id);

            return WrapModel<IEnumerable<CustomerJourneyDetInfo>>.Ok(data);
        }


        [HttpPost()]
        public async Task<ActionResult<WrapPagingModel<CustomerJourneyInfo>>> Add([FromBody] CustomerJourneyInfo model)
        {
            var data = await _service.Add(model);
            return WrapPagingModel<CustomerJourneyInfo>.Ok(data, 0);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WrapModel<CustomerJourneyInfo>>> Upd([FromRoute] Guid nbpayitemId, [FromBody] CustomerJourneyInfo model)
        {
            var data = await _service.Update(model);
            return WrapModel<CustomerJourneyInfo>.Ok(data);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<WrapModel<bool>>> Delete([FromRoute] String id)
        {
            var data = await _service.Delete(id);
            return WrapModel<bool>.Ok(data);
        }
        [HttpPost("deletes")]
        public async Task<ActionResult<WrapModel<bool>>> Deletes([FromBody] String[] ids)
        {
            var data = await _service.Deletes(ids);
            return WrapModel<bool>.Ok(data);
        }
        [HttpPost("deleteAll")]
        public async Task<ActionResult<WrapModel<bool>>> DeleteAll(DeleteAllInfo obj)
        {
            var data = await _service.DeleteAll(obj);
            return WrapModel<bool>.Ok(data);
        }

        [HttpPost("queryCustomer")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<CustomerInfo>>>> QueryCustomer([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryCustomer(model, page, pageSize);
            var total = await _service.QueryCustomerTotal(model);

            return WrapPagingModel<IEnumerable<CustomerInfo>>.Ok(data, total);
        }

    }
}
