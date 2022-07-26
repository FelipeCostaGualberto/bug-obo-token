using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExampleObo.Client.Services;

public interface IAuthenticationStateProvider
{
    Task Logout(string logoutPath);
}

public class MyAuthenticationStateProvider : AuthenticationStateProvider, IAuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorageService;

    public MyAuthenticationStateProvider
    (
        ILocalStorageService localStorage
    )
    {
        _localStorageService = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var savedToken = await _localStorageService.GetItemAsync<string>("authToken");
        if (string.IsNullOrWhiteSpace(savedToken))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "Bearer"));
        var authenticationState = new AuthenticationState(principal);
        var expirationMs = Convert.ToDouble(principal.FindFirst("exp").Value);
        var expirationDateTime = UnixTimeStampToDateTime(expirationMs);
        if (DateTime.UtcNow > expirationDateTime)
        {
            await _localStorageService.RemoveItemAsync("authToken");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
        return authenticationState;
    }

    public async Task Logout(string logoutPath)
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        await _localStorageService.RemoveItemAsync("authToken");
        NotifyAuthenticationStateChanged(authState);
        //await LogoutRedirect();
        //_navigationManager.NavigateTo("", true);
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        foreach (var kvp in keyValuePairs)
        {
            switch (kvp.Key)
            {
                case "nameid":
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, kvp.Value.ToString()));
                    break;
                case "unique_name":
                    claims.Add(new Claim(ClaimTypes.Name, kvp.Value.ToString()));
                    break;
                case "email":
                    claims.Add(new Claim(ClaimTypes.Email, kvp.Value.ToString()));
                    break;
                case "role":
                    claims.Add(new Claim(ClaimTypes.Role, kvp.Value.ToString()));
                    break;
                default:
                    claims.Add(new Claim(kvp.Key.ToString(), kvp.Value.ToString()));
                    break;
            }
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }
}