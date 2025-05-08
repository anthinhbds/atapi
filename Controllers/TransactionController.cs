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
    public class TransactionController : GenericController
    {
        private readonly ITransactionService _service;

        public TransactionController(
            ITransactionService service
        )
        {
            _service = service;
        }

        // POST: api/PurchaseOrder
        [HttpPost("query")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<TransactionInfo>>>> Query([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.Query(model, page, pageSize);
            var total = await _service.QueryTotal(model);

            return WrapPagingModel<IEnumerable<TransactionInfo>>.Ok(data, total);
        }

        [HttpPost("expense")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<TransactionInfo>>>> QueryExpense([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryExpense(model, page, pageSize);
            var total = await _service.QueryExpenseTotal(model);

            return WrapPagingModel<IEnumerable<TransactionInfo>>.Ok(data, total);
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<WrapModel<IEnumerable<TransactionDetailInfo>>>> GetDetail(Guid id)
        {
            var data = await _service.GetDetails(id);

            return WrapModel<IEnumerable<TransactionDetailInfo>>.Ok(data);
        }
        [HttpGet("members/{id}")]
        public async Task<ActionResult<WrapModel<IEnumerable<TransactionMemberInfo>>>> GetMember(Guid id)
        {
            var data = await _service.GetMembers(id);

            return WrapModel<IEnumerable<TransactionMemberInfo>>.Ok(data);
        }

        [HttpPost("getSummary")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<TransactionSummaryInfo>>>> GetSumByStatus([FromBody] QueryParamModel model)
        {
            var data = await _service.GetSummary(model);

            return WrapPagingModel<IEnumerable<TransactionSummaryInfo>>.Ok(data, 0);
        }

        [HttpPost()]
        public async Task<ActionResult<WrapPagingModel<TransactionInfo>>> Add([FromBody] TransactionInfo model)
        {
            var data = await _service.Add(model);
            return WrapPagingModel<TransactionInfo>.Ok(data, 0);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WrapModel<TransactionInfo>>> Upd([FromRoute] Guid id, [FromBody] TransactionInfo model)
        {
            var data = await _service.Update(model);
            return WrapModel<TransactionInfo>.Ok(data);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<WrapModel<bool>>> Delete([FromRoute] Guid id)
        {
            var data = await _service.Delete(id);
            return WrapModel<bool>.Ok(data);
        }
        [HttpPost("deletes")]
        public async Task<ActionResult<WrapModel<bool>>> Deletes([FromBody] Guid[] ids)
        {
            var data = await _service.Deletes(ids);
            return WrapModel<bool>.Ok(data);
        }

        [HttpPost("transactionNo")]
        public async Task<ActionResult<WrapModel<String>>> GetTransactionNo([FromBody] TransactionInfo model)
        {
            var data = await _service.GetTransNo(model);

            return WrapModel<String>.Ok(data);
        }
        [HttpPost("bctcByMonth")]
        public async Task<ActionResult<WrapModel<IEnumerable<MonthlyDetailInfo>>>> GetRevenueMonthly([FromBody] MonthlyDetailModel model)
        {
            var data = await _service.GetBctcByMonth(model);

            return WrapModel<IEnumerable<MonthlyDetailInfo>>.Ok(data);
        }
    }
}
