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
    public class CustomerController : GenericController
    {
        private readonly ICustomerService _service;

        public CustomerController(
            ICustomerService service
        )
        {
            _service = service;
        }

        // POST: api/PurchaseOrder
        [HttpPost("query")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<CustomerInfo>>>> Query([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.Query(model, page, pageSize);
            var total = await _service.QueryTotal(model);

            return WrapPagingModel<IEnumerable<CustomerInfo>>.Ok(data, total);
        }

        [HttpPost("queryAll")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<CustomerInfo>>>> QueryAll([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryAll(model, page, pageSize);
            var total = await _service.QueryAllTotal(model);

            return WrapPagingModel<IEnumerable<CustomerInfo>>.Ok(data, total);
        }

        [HttpPost("combo")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<CustomerInfo>>>> Combo([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.Combo(model, page, pageSize);
            var total = await _service.ComboTotal(model);

            return WrapPagingModel<IEnumerable<CustomerInfo>>.Ok(data, total);
        }

        [HttpGet("notes/{id}")]
        public async Task<ActionResult<WrapModel<IEnumerable<CustomerNoteInfo>>>> GetDetail(string id)
        {
            var data = await _service.GetNotes(id);

            return WrapModel<IEnumerable<CustomerNoteInfo>>.Ok(data);
        }

        [HttpPost("getSummary")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<CustomerSummaryInfo>>>> GetSumByStatus([FromBody] QueryParamModel model)
        {
            var data = await _service.GetSummary(model);

            return WrapPagingModel<IEnumerable<CustomerSummaryInfo>>.Ok(data, 0);
        }

        [HttpPost()]
        public async Task<ActionResult<WrapPagingModel<CustomerInfo>>> Add([FromBody] CustomerInfo model)
        {
            var data = await _service.Add(model);
            return WrapPagingModel<CustomerInfo>.Ok(data, 0);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WrapModel<CustomerInfo>>> Upd([FromRoute] Guid nbpayitemId, [FromBody] CustomerInfo model)
        {
            var data = await _service.Update(model);
            return WrapModel<CustomerInfo>.Ok(data);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<WrapModel<bool>>> Delete([FromRoute] String id)
        {
            var data = await _service.UserDelete(id);
            return WrapModel<bool>.Ok(data);
        }
        [HttpPost("deletes")]
        public async Task<ActionResult<WrapModel<bool>>> Deletes([FromBody] String[] ids)
        {
            var data = await _service.UserDeletes(ids);
            return WrapModel<bool>.Ok(data);
        }

        [HttpPost("assigment")]
        public async Task<ActionResult<WrapModel<bool>>> Add([FromBody] AssignmentAaprtmentModel model)
        {
            var data = await _service.AssignmenCustomer(model);
            return WrapModel<bool>.Ok(data);
        }
        [HttpPost("getAssignment")]
        public async Task<ActionResult<WrapModel<IEnumerable<AssignmentLogInfo>>>> GetAssignment([FromQuery] string id)
        {
            var data = await _service.GetAssignment(id);
            return WrapModel<IEnumerable<AssignmentLogInfo>>.Ok(data);


        }

        [HttpPost("checkTelephone")]
        public async Task<ActionResult<WrapModel<TelephoneExistsInfo>>> CheckTelephone([FromBody] TelephoneExistsModel model)
        {
            var data = await _service.IsTelephoneExists(model.apartmentId, model.phones);

            return WrapModel<TelephoneExistsInfo>.Ok(data);
        }
    }
}
