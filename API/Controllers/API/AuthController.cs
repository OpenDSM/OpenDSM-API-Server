using CLMath;
using Microsoft.AspNetCore.Mvc;
using OpenDSM.Lib.Auth;

namespace API.Controllers.API;

[Route("/api/auth")]
public class AuthController : ControllerBase
{
    #region Public Methods

    [HttpPost("login")]
    public IActionResult Login([FromForm] string username, [FromForm] string password)
    {
        if (AccountManagement.Instance.TryAttemptLogin(username, password, out User user, out string reason))
        {
            return new JsonResult(new
            {
                user.ID,
                user.Username,
                user.Email,
                Token = user.Password,
                Success = true,
            });
        }

        return new JsonResult(new
        {
            Success = false,
            reason,
        });
    }

    [HttpPost("register")]
    public IActionResult Register([FromForm] string username, [FromForm] string email, [FromForm] string password)
    {
        User user = AccountManagement.Instance.CreateUser(email, username, password);

        return new JsonResult(new
        {
            user.ID,
            user.Username,
            user.Email,
            Token = user.Password,
            Success = true,
        });
    }

    #endregion Public Methods
}