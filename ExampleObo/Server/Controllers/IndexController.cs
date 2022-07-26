using ExampleObo.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
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