using Microsoft.AspNetCore.Mvc;

namespace atmnr_api.Controllers;

public class GenericController : ControllerBase
{
    // Add your controller actions here
    protected string GetUserId()
    {
        return User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? string.Empty;
    }
}

