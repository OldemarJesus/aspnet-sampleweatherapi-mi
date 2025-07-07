using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace SampleWeatherApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SampleController : ControllerBase
{
    [HttpGet]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = "AzureAd:Scopes:Read",
        RequiredAppPermissionsConfigurationKey = "AzureAd:AppPermissions:Read"
    )]
    public IActionResult Get()
    {
        return Ok(new[]
        {
            new
            {
                Message = "Hello World!",
                UserId = GetUserId(),
                Date = DateTime.UtcNow,

            },
            new
            {
                Message = "Hello World 2!",
                UserId = GetUserId(),
                Date = DateTime.UtcNow,
            }
        });
    }
    private bool IsAppMakingRequest()
    {
        if (HttpContext.User.Claims.Any(c => c.Type == "idtyp"))
        {
            return HttpContext.User.Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
        }
        else
        {
            return HttpContext.User.Claims.Any(c => c.Type == "roles") && !HttpContext.User.Claims.Any(c => c.Type == "scp");
        }
    }

    private Guid GetUserId()
    {
        Guid userId;
        if (!Guid.TryParse(HttpContext.User.GetObjectId(), out userId))
        {
            throw new Exception("User ID is not valid.");
        }
        return userId;
    }
}
