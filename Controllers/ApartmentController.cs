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
    public class ApartmentController : GenericController
    {
        private readonly IApartmentService _service;

        public ApartmentController(
            IApartmentService service
        )
        {
            _service = service;
        }

        // POST: api/PurchaseOrder
        [HttpPost("query")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<ApartmentInfo>>>> Query([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.Query(model, page, pageSize);
            var total = await _service.QueryTotal(model);

            return WrapPagingModel<IEnumerable<ApartmentInfo>>.Ok(data, total);
        }

        [HttpPost("queryPartner")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<ApartmentInfo>>>> QueryPartner([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryPartner(model, page, pageSize);
            var total = await _service.QueryPartnerTotal(model);

            return WrapPagingModel<IEnumerable<ApartmentInfo>>.Ok(data, total);
        }

        [HttpPost("queryAll")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<ApartmentInfo>>>> QueryAll([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryAll(model, page, pageSize);
            var total = await _service.QueryAllTotal(model);

            return WrapPagingModel<IEnumerable<ApartmentInfo>>.Ok(data, total);
        }

        [HttpPost("getExpired")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<ApartmentInfo>>>> QueryExpried([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryExpired(model, page, pageSize);
            var total = await _service.QueryExpiredTotal(model);

            return WrapPagingModel<IEnumerable<ApartmentInfo>>.Ok(data, total);
        }

        [HttpPost("queryAssignment")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<ApartmentInfo>>>> QueryAssignment([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryAssignment(model, page, pageSize);
            var total = await _service.QueryAssignmentTotal(model);

            return WrapPagingModel<IEnumerable<ApartmentInfo>>.Ok(data, total);
        }

        [HttpGet("notes/{id}")]
        public async Task<ActionResult<WrapModel<IEnumerable<ApartmentNoteInfo>>>> GetDetail(string id)
        {
            var data = await _service.GetNotes(id);

            return WrapModel<IEnumerable<ApartmentNoteInfo>>.Ok(data);
        }

        [HttpPost("getSummary")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<ApartmentSummaryInfo>>>> GetSumByStatus([FromBody] QueryParamModel model)
        {
            var data = await _service.GetSummary(model);

            return WrapPagingModel<IEnumerable<ApartmentSummaryInfo>>.Ok(data, 0);
        }

        [HttpPost()]
        public async Task<ActionResult<WrapPagingModel<ApartmentInfo>>> Add([FromBody] ApartmentInfo model)
        {
            var data = await _service.Add(model);
            return WrapPagingModel<ApartmentInfo>.Ok(data, 0);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WrapModel<ApartmentInfo>>> Upd([FromRoute] Guid nbpayitemId, [FromBody] ApartmentInfo model)
        {
            var data = await _service.Update(model);
            return WrapModel<ApartmentInfo>.Ok(data);
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
            var data = await _service.AssignmentAaprtment(model);
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

        [HttpPost("getCombo")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<ApartmentComboInfo>>>> QueryCombo([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryCombo(model, page, pageSize);
            var total = await _service.QueryComboTotal(model);

            return WrapPagingModel<IEnumerable<ApartmentComboInfo>>.Ok(data, total);
        }
    }
}
