﻿using Microsoft.AspNetCore.Mvc;
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
    [HttpPost("image/{type}")]
    public async Task<IActionResult> UploadImage(string type, [FromForm] string base64, [FromForm] string email, [FromForm] string token)
    {

        if (UserModel.TryGetUserWithToken(email, token, out UserModel? user))
        {
            await user.UploadImage(base64, type == "profile");
            return Ok(new
            {
                message = "Image was uploaded"
            });
        }
        return BadRequest(new
        {
            message = "User not found"
        });
    }
    [HttpGet("image/{type}")]
    public IActionResult GetProfileImage(string type, int id)
    {
        UserModel? user = UserModel.GetByID(id);
        if (user != null)
        {
            string path = type switch
            {
                "profile" => user.ProfileImage,
                "banner" => user.ProfileBannerImage,
                _ => user.ProfileImage
            };
            if (System.IO.File.Exists(path))
            {
                return new FileStreamResult(new FileStream(path, FileMode.Open, FileAccess.Read), "image/png");
            }
        }
        if (type == "profile")
        {
            return Redirect("/assets/images/missing-profile-image.svg");
        }
        else if (type == "banner")
        {
            return Redirect("/assets/images/missing-banner.jpg");
        }
        return NotFound(new
        {
            message = $"Image type not found: {type}"
        });
    }
}
