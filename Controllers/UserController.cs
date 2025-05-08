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
    public class UserController : GenericController
    {
        private readonly IUserService _service;

        public UserController(
            IUserService service
        )
        {
            _service = service;
        }

        // POST: api/PurchaseOrder
        [HttpPost("query")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<UserInfo>>>> Query([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.Query(model, page, pageSize);
            var total = await _service.QueryTotal(model);

            return WrapPagingModel<IEnumerable<UserInfo>>.Ok(data, total);
        }
        [HttpPost("listUser")]
        public async Task<ActionResult<WrapModel<IEnumerable<UserListInfo>>>> GetListUser([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.GetListUser(model, page, pageSize);

            return WrapModel<IEnumerable<UserListInfo>>.Ok(data);
        }

        [HttpPost("getSummary")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<UserSummaryInfo>>>> GetSumByStatus([FromBody] QueryParamModel model)
        {
            var data = await _service.GetSummary(model);

            return WrapPagingModel<IEnumerable<UserSummaryInfo>>.Ok(data, 0);
        }

        [HttpPost()]
        public async Task<ActionResult<WrapPagingModel<UserInfo>>> Add([FromBody] UserInfo model)
        {
            var data = await _service.Add(model);
            return WrapPagingModel<UserInfo>.Ok(data, 0);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WrapModel<UserInfo>>> Upd([FromRoute] Guid nbpayitemId, [FromBody] UserInfo model)
        {
            var data = await _service.Update(model);
            return WrapModel<UserInfo>.Ok(data);
        }

        [HttpGet("getMe/{refreshToken}")]
        public async Task<ActionResult<WrapModel<UserResultInfo>>> GetMe(string refreshToken)
        {
            var data = await _service.GetMe(refreshToken);
            if (data == null)
            {
                return Unauthorized();
            }
            return WrapModel<UserResultInfo>.Ok(data);
        }

        [HttpPost("searchProfile")]
        public async Task<ActionResult<WrapModel<IEnumerable<UserSearchProfileInfo>>>> GetSearchProfile([FromBody] QueryParamModel model)
        {
            var formid = model.Filter.FirstOrDefault(f => f.Property == "formId")?.Value;
            var data = await _service.GetSearchProfile(formid.ToString());

            return WrapModel<IEnumerable<UserSearchProfileInfo>>.Ok(data);
        }

        [HttpPost("addSearchProfile")]
        public async Task<ActionResult<WrapModel<UserSearchProfileInfo>>> AddSearchProfile([FromBody] UserSearchProfileInfo model)
        {
            var data = await _service.AddSearchProfile(model);
            return WrapModel<UserSearchProfileInfo>.Ok(data);
        }

        [HttpPut("updSearchProfile/{id}")]
        public async Task<ActionResult<WrapModel<UserSearchProfileInfo>>> Upd([FromRoute] Guid id, [FromBody] UserSearchProfileInfo model)
        {
            model.Id = id;
            var data = await _service.UpdateSearchProfile(model);
            return WrapModel<UserSearchProfileInfo>.Ok(data);
        }

        [HttpDelete("delSearchProfile/{id}")]
        public async Task<ActionResult<WrapModel<bool>>> Delete([FromRoute] Guid id)
        {
            var data = await _service.DeleteSearchProfile(id);
            return WrapModel<bool>.Ok(data);
        }

        [HttpPost("checkExistSearchProfile")]
        public async Task<ActionResult<WrapModel<bool>>> CheckExistSearchProfile([FromBody] UserSearchProfileInfo obj)
        {
            var data = await _service.CheckExistSearchProfile(obj);
            return WrapModel<bool>.Ok(data);
        }

        [HttpPost("getCombo")]
        public async Task<ActionResult<WrapPagingModel<IEnumerable<UserComboInfo>>>> QueryCombo([FromBody] QueryParamModel model, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var data = await _service.QueryCombo(model, page, pageSize);
            var total = await _service.QueryComboTotal(model);

            return WrapPagingModel<IEnumerable<UserComboInfo>>.Ok(data, total);
        }
    }
}
