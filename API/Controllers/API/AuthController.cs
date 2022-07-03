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
            message = reason,
        });
    }

    [HttpPost("register")]
    public IActionResult Register([FromForm] string username, [FromForm] string email, [FromForm] string password)
    {
        try
        {
            if (AccountManagement.Instance.CreateUser(email, username, password, out User user, out string reason))
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
                success = false,
                message = reason
            });
        }
        catch (Exception ex)
        {
            log.Error(ex.Message, ex.StackTrace);
            return BadRequest(new
            {
                success = false,
                message = "Unknown Server Issue"
            });
        }
    }

    [HttpPost("save-profile/{key}")]
    public IActionResult SaveProfile(string key, [FromForm] string value)
    {
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                switch (key.ToLower())
                {
                    case "image":
                        break;

                    case "show_github":
                        user.ShowGithub = bool.Parse(value);
                        break;

                    default:
                        break;
                }
                return Ok();
            }
        }

        return BadRequest();
    }

    #endregion Public Methods
}