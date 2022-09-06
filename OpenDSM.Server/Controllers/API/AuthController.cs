// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;
using OpenDSM.SQL;

namespace OpenDSM.Server.Controllers.API;

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{

    #region Public Methods

    [HttpGet("user")]
    public IActionResult GetUser([FromQuery] int id, [FromQuery] bool? includeImages)
    {
        if (UserListHandler.TryGetByID(id, out UserModel? user))
        {
            return new JsonResult(user.ToObject(includeImages.GetValueOrDefault(false)));
        }
        return BadRequest(new
        {
            message = $"No user exists with id of {id}"
        });
    }

    [HttpPost("activate-dev-account")]
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

    [HttpGet("readme/{id}")]
    public IActionResult GetReadme(int id, bool? git)
    {
        UserModel? user = UserListHandler.GetByID(id);
        if (user != null)
        {
            return Ok(new
            {
                about = git.HasValue ? user.GitReadme : user.About
            });
        }
        return BadRequest(new
        {
            message = $"User with id of {id} doesn't exist"
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> TryLogin([FromForm] string username, [FromForm] string password, [FromQuery] bool? useToken)
    {
        return await Task.Run(() =>
        {
            if (useToken.GetValueOrDefault(false))
            {
                if (UserListHandler.TryGetUserWithToken(username, password, out UserModel? user))
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
            }
            else
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
            }
        });
    }

    [HttpPost("signup")]
    public async Task<IActionResult> TrySignup([FromForm] string email, [FromForm] string username, [FromForm] string password)
    {
        return await Task.Run(() =>
        {
            try
            {

                if (Authorization.CreateUser(username, email, password, out var reason))
                {
                    if (UserListHandler.TryGetUser(username, password, out UserModel? user))
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

    [HttpPatch("settings")]
    public IActionResult UpdateSettings([FromForm] string name, [FromForm] string value)
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

    [HttpPost("image/{type}")]
    public async Task<IActionResult> UploadImage(string type, [FromForm] string base64)
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            if (type == "profile")
            {

                user.ProfileImage = base64;
            }
            else if (type == "banner")
            {
                user.BannerImage = base64;
            }
            return Ok(new
            {
                message = "Image was uploaded"
            });
        }
        return BadRequest(new
        {
            message = "Invalid Credentials"
        });
    }

    #endregion Public Methods

}