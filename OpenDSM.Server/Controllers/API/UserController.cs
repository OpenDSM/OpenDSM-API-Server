using Microsoft.AspNetCore.Mvc;

using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.API;
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    /// <summary>
    /// Gets information on specified user
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public IActionResult GetUser([FromRoute] int id, [FromQuery] bool? includeImages)
    {
        if (UserListHandler.TryGetByID(id, out UserModel user))
        {
            return new JsonResult(user.ToObject(includeImages.GetValueOrDefault(false)));
        }

        return BadRequest(new
        {
            message = $"No user found with id of '{id}'"
        });
    }

    /// <summary>
    /// Creates user
    /// </summary>
    /// <param name="email">The users email</param>
    /// <param name="username">The users username</param>
    /// <param name="password">The users password</param>
    /// <returns></returns>
    [HttpPost()]
    public async Task<IActionResult> CreateUser([FromForm] string email, [FromForm] string username, [FromForm] string password)
    {
        return await Task.Run(() =>
        {
            try
            {

                if (SQL.Authorization.CreateUser(username, email, password, out var reason))
                {
                    if (UserListHandler.TryGetUser(username, password, out UserModel user))
                    {
                        return new JsonResult(new
                        {
                            success = true,
                            message = "Account Created Successfully",
                            user = user.ToObject(false)
                        });
                    }
                }
                return new JsonResult(new
                {
                    success = false,
                    message = string.Concat(reason.ToString().Select(x => char.IsUpper(x) ? " " + x : x.ToString())).Trim()
                });
            }
            catch (Exception e)
            {
                log.Error($"Unable to create user", e);
                return new JsonResult(new
                {
                    success = false,
                    error = e.Message,
                    stacktrace = e.StackTrace,
                });
            }
        });
    }

    /// <summary>
    /// Gets a list of all users Github repositories
    /// </summary>
    /// <returns></returns>
    [HttpGet("git/repositories")]
    public IActionResult GetUserRepositories()
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            if (user.IsDeveloperAccount)
            {
                return new JsonResult(user.Repositories);
            }

            return BadRequest(new
            {
                message = "Developer account not activated!"
            });
        }
        return BadRequest(new
        {
            message = "User is not authorized!"
        });
    }

    /// <summary>
    /// Links the users Github Account.
    /// </summary>
    /// <param name="git_username"></param>
    /// <param name="git_token"></param>
    /// <returns></returns>
    [HttpPost("git/activate")]
    public IActionResult ActivateDevAccount([FromForm] string git_username, [FromForm] string git_token)
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            user.GitUsername = git_username;
            user.GitToken = git_token;
            if (user.IsDeveloperAccount)
            {
                user.UpdateSetting("git_username", user.GitUsername);
                user.UpdateSetting("git_token", user.GitToken);
                return Ok(new
                {
                    message = $"Account activated successfully"
                });
            }
        }
        return BadRequest(new
        {
            message = "Invalid Credentials"
        });
    }


    /// <summary>
    /// Updates settings specified by name to content of body
    /// </summary>
    /// <param name="name">The name of the setting</param>
    /// <param name="value">The value of the setting</param>
    /// <returns></returns>
    [HttpPatch("{name}")]
    public IActionResult UpdateSettings([FromRoute] string name, [FromBody] string value)
    {

        if (IsLoggedIn(Request, out UserModel? user))
        {
            switch (name)
            {
                case "about":
                    user.UpdateAbout(value);
                    break;

                default:
                    if (!string.IsNullOrWhiteSpace(value))
                        user.UpdateSetting(name, value);
                    break;
            }
            return Ok(new
            {
                message = $"\"{name}\" has been updated"
            });
        }
        return BadRequest(new
        {
            message = "Invalid Credentials"
        });
    }

    /// <summary>
    /// Validates user credentials
    /// </summary>
    /// <param name="username">The users username or email</param>
    /// <param name="password">The users password</param>
    /// <returns></returns>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateUserCredentials([FromForm] string username, [FromForm] string password) => await Task.Run(() =>
        {
            if (UserListHandler.TryGetUser(username, password, out UserModel? user, out var reason))
            {
                return new JsonResult(new
                {
                    success = true,
                    message = "Logged in Successfully",
                    user = user.ToObject(),
                });
            }
            return new JsonResult(new
            {
                success = false,
                message = string.Concat(reason.ToString().Select(x => char.IsUpper(x) ? " " + x : x.ToString())).Trim()
            });
        });

    /// <summary>
    /// Validates user credentials using a token
    /// </summary>
    /// <param name="username">The users username or email</param>
    /// <param name="token">The users token</param>
    /// <returns></returns>
    [HttpPost("validate/token")]
    public async Task<IActionResult> ValidateUserCredentialsWithToken([FromForm] string username, [FromForm] string token) => await Task.Run(() =>
        {
            if (UserListHandler.TryGetUserWithToken(username, token, out UserModel? user))
            {
                return new JsonResult(new
                {
                    success = true,
                    message = "Logged in Successfully",
                    user = user.ToObject(),
                });
            }
            return new JsonResult(new
            {
                success = false,
                message = "Unable to login with token provided!"
            });
        });

}