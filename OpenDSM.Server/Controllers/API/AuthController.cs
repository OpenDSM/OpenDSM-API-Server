using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Models;
using OpenDSM.SQL;

namespace OpenDSM.Server.Controllers.API;

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> TryLogin([FromForm] string username, [FromForm] string password)
    {
        return await Task.Run(() =>
        {
            if (UserModel.TryGetUser(username, password, out UserModel? user, out var reason))
            {
                return new JsonResult(new
                {
                    success = true,
                    message = "Logged in Successfully",
                    user,
                });
            }
            return new JsonResult(new
            {
                success = false,
                message = string.Concat(reason.ToString().Select(x => char.IsUpper(x) ? " " + x : x.ToString())).Trim()
            });
        });
    }

    [HttpPost("signup")]
    public async Task<IActionResult> TrySignup([FromForm] string email, [FromForm] string username, [FromForm] string password)
    {
        return await Task.Run(() =>
        {
            if (Authoriztaion.Register(username, email, AccountType.User, password, out var reason))
            {
                if (UserModel.TryGetUser(username, password, out UserModel? user))
                {
                    return new JsonResult(new
                    {
                        success = true,
                        message = "Account Created Successfully",
                        user
                    });
                }
            }
            return new JsonResult(new
            {
                success = false,
                message = string.Concat(reason.ToString().Select(x => char.IsUpper(x) ? " " + x : x.ToString())).Trim()
            });
        });
    }
}
