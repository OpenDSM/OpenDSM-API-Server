using HashidsNet;
using Microsoft.AspNetCore.Mvc;
using OpenDSM.Authentication.Models;

namespace OpenDSM.Server.Controllers;
[ApiController]
[Route("user")]
public class UserController : ControllerBase
{

    #region Public Methods

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
            if (user.ActivateDeveloperAccount(new() { Username = git_username, Token = git_token }))
            {
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
    /// Creates user
    /// </summary>
    /// <param name="email">The users email</param>
    /// <param name="username">The users username</param>
    /// <param name="password">The users password</param>
    /// <returns></returns>
    [HttpPost()]
    public IActionResult CreateUser([FromForm] string email, [FromForm] string username, [FromForm] string password)
    {
        try
        {
            if (Authentication.Collections.UserCollection.CreateUser(email, username, password, out UserModel? user))
            {
                return new JsonResult(new
                {
                    success = true,
                    message = "Account Created Successfully",
                    user = user?.ToObject()
                });
            }
            if (SQL.Authorization.TryCreateUser(username, email, password, out var reason))
            {
                if (UserListHandler.TryGetUser(username, password, out UserModel user))
                {
                }
            }
            return BadRequest(new
            {
                success = false,
                message = string.Concat(reason.ToString().Select(x => char.IsUpper(x) ? " " + x : x.ToString())).Trim()
            });
        }
        catch (Exception e)
        {
            return BadRequest(new
            {
                success = false,
                error = e.Message,
                stacktrace = e.StackTrace,
            });
        }
    }

    /// <summary>
    /// Gets the users api key
    /// </summary>
    /// <returns></returns>
    [HttpGet("api_key")]
    public IActionResult GetAPIKey([FromQuery] bool? regen)
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            if (regen.GetValueOrDefault(false) || string.IsNullOrWhiteSpace(user.API_KEY))
                user.GenerateAPIKey();
            return new JsonResult(new
            {
                api_key = user?.API_KEY
            });
        }
        return BadRequest(new
        {
            message = $"Unable to authenticate user"
        });
    }

    /// <summary>
    /// Gets information on specified user
    /// </summary>
    /// <param name="id_hash"></param>
    /// <returns></returns>
    [HttpGet("{id_hash?}")]
    public IActionResult GetUser([FromRoute] string? id_hash, [FromQuery] bool? includeImages)
    {
        UserModel? user = null;
        if (id_hash == null || string.IsNullOrWhiteSpace(id_hash))
        {
            if (IsLoggedIn(Request, out user))
            {
                return new JsonResult(user.ToObject(includeImages.GetValueOrDefault(false)));
            }
            return BadRequest(new
            {
                message = "User not authenticated, ID must be provided!"
            });
        }
        else
        {

            try
            {
                int id = HashIds.DecodeSingle(id_hash);
                if (UserListHandler.TryGetByID(id, out user))
                {
                    return new JsonResult(user.ToObject(includeImages.GetValueOrDefault(false)));
                }

            }
            catch (NoResultException)
            {
                return BadRequest(new
                {
                    message = $"Id provided was not valid: '{id_hash}'"
                });
            }
            return BadRequest(new
            {
                message = $"No user found with id of '{id_hash}'"
            });
        }
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
            if (user?.IsDeveloperAccount ?? false)
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
    /// Updates settings specified by name to content of body
    /// </summary> 
    /// <param name="name">The name of the setting</param>
    /// <returns></returns>
    [HttpPatch("{name}")]
    public IActionResult UpdateSettings([FromRoute] string name)
    {
        string value = new StreamReader(Request.Body).ReadToEndAsync().Result;
        if (string.IsNullOrWhiteSpace(value))
            return BadRequest(new
            {
                message = "Body can't be empty"
            });
        if (IsLoggedIn(Request, out UserModel? user))
        {
            switch (name)
            {
                case "about":
                    user.UpdateAbout(value);
                    break;

                default:
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        string[] accepted = { "username" };
                        if (accepted.Contains(name.ToLower().Trim()))
                            user.UpdateSetting(name, value);
                        else
                        {
                            return BadRequest(new
                            {
                                message = $"\"{name}\" is not a valid setting"
                            });
                        }
                    }
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

    #endregion Public Methods

}