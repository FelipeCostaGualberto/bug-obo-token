using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using ExampleObo.Shared;
using Newtonsoft.Json;

namespace ExampleObo.Server.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]/[action]")]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AccountController
    (
        IConfiguration configuration
    )
    {
        _configuration = configuration;
    }

    [HttpGet]
    public ActionResult MsalLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(MsalLoginCallback)),
        };

        var challenge = Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        return challenge;
    }

    public async Task<IActionResult> MsalLoginCallback()
    {
        var result = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);

        try
        {
            var principal = result.Principal;
            var user = new UserModel();
            user.Email = principal.Identity.Name;
            user.Uid = principal.FindFirstValue("uid");
            user.Utid = principal.FindFirstValue("utid");
            var userSerial = UserModel.Base64Encode(JsonConvert.SerializeObject(user));
            var localRedirect = $"/?userSerial={userSerial}";
            return LocalRedirect(localRedirect);
        }

        catch (Exception ex)
        {
            using var writer = new StringWriter();
            writer.Write($@"<html><body>");
            writer.Write($@"<h2>SERVER ERROR</h2>");
            writer.Write($@"<p><strong>{ex.Message}</strong></p>");
            writer.Write($@"<p>{ex.InnerException}</p>");
            writer.Write($@"</body></html>");
            return Content(writer.ToString(), "text/html; charset=utf-8");
        }
    }

    [Route("{userSerial}")]
    public string GetToken(string userSerial)
    {
        var user = JsonConvert.DeserializeObject<UserModel>(UserModel.Base64Decode(userSerial));
        var claimList = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("utid", user.Utid),
            new Claim("uid", user.Uid),
            new Claim("custom", "This is a custom value"),
        };
        var token = CreateToken(claimList);
        return token;
    }

    private string CreateToken(List<Claim> claimList)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecurityKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claimList),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenValue = tokenHandler.WriteToken(token);
        return tokenValue;
    }
}