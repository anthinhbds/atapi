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
    public class ProjectController : GenericController
    {
        private readonly IProjectService _service;

        public ProjectController(
            IProjectService service
        )
        {
            _service = service;
        }

        // POST: api/PurchaseOrder
        [HttpPost("query")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<ProjectInfo>>>> Query([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.Query(model, page, pageSize);
            var total = await _service.QueryTotal(model);

            return WrapPagingModel<IEnumerable<ProjectInfo>>.Ok(data, total);
        }

        [HttpPost("getSummary")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<ProjectSummaryInfo>>>> GetSumByStatus([FromBody] QueryParamModel model)
        {
            var data = await _service.GetSummary(model);

            return WrapPagingModel<IEnumerable<ProjectSummaryInfo>>.Ok(data, 0);
        }

        [HttpPost()]
        public async Task<ActionResult<WrapPagingModel<ProjectInfo>>> Add([FromBody] ProjectInfo model)
        {
            var data = await _service.Add(model);
            return WrapPagingModel<ProjectInfo>.Ok(data, 0);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WrapModel<ProjectInfo>>> Upd([FromRoute] Guid nbpayitemId, [FromBody] ProjectInfo model)
        {
            var data = await _service.Update(model);
            return WrapModel<ProjectInfo>.Ok(data);
        }

        [HttpPut("archive")]
        public async Task<ActionResult<WrapModel<bool>>> Archive([FromBody] String[] ids)
        {
            var data = await _service.Archive(ids);
            return WrapModel<bool>.Ok(data);
        }
        [HttpPost("archiveAll")]
        public async Task<ActionResult<WrapModel<bool>>> ArchiveAll()
        {
            var data = await _service.ArchiveAll();
            return WrapModel<bool>.Ok(data);
        }

        [HttpPut("restore")]
        public async Task<ActionResult<WrapModel<bool>>> Restore([FromBody] String[] ids)
        {
            var data = await _service.Restore(ids);
            return WrapModel<bool>.Ok(data);
        }
        [HttpPost("restoreAll")]
        public async Task<ActionResult<WrapModel<bool>>> RestoreAll()
        {
            var data = await _service.RestoreAll();
            return WrapModel<bool>.Ok(data);
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

        [HttpPost("getDistrict")]
        public async Task<ActionResult<WrapModel<IEnumerable<DistrictInfo>>>> QueryDistrict([FromBody] QueryParamModel model)
        {
            var data = await _service.QueryDistrict(model);

            return WrapModel<IEnumerable<DistrictInfo>>.Ok(data);
        }
    }
}
