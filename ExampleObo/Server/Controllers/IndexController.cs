using ExampleObo.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExampleObo.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class IndexController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ITokenAcquisition _tokenAcquisition;

    public IndexController
    (
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition
    )
    {
        _configuration = configuration;
        _tokenAcquisition = tokenAcquisition;
    }

    public RequestResult GetCustomClaim()
    {
        var result = new RequestResult();

        try
        {
            // This custom claim is defined in AccountController.cs
            var customClaimValue = User.FindFirstValue("custom");
            if (customClaimValue != "This is a custom value")
            {
                if(string.IsNullOrEmpty(customClaimValue)) throw new Exception("The claim 'custom' was not found!");
                throw new Exception("The claim 'custom' has an invalid value: " + customClaimValue + ". It as expected: 'This is a custom value'.");
            };
            result.Message = "The claim 'custom' was found and has the correct value!";
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Message = ex.Message + " --- " + ex.InnerException;
        }

        return result;
    }

    public async Task<RequestResult> GetToken()
    {
        var result = new RequestResult();

        try
        {
            var scopes = new List<string>(_configuration["AzureAd:Scopes"].Split(new char[] { ' ' }));
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            result.Message = token;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Message = ex.Message + " --- " + ex.InnerException;
        }

        return result;
    }
}