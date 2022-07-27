using Blazored.LocalStorage;
using ExampleObo.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExampleObo.Client.Pages;

public partial class Index : ComponentBase
{
    [Inject] private HttpClient RequestClient { get; set; }
    [Inject] private NavigationManager Navigation { get; set; }
    [Inject] private AuthenticationStateProvider AuthProvider { get; set; }
    [Inject] private ILocalStorageService LocalStorage { get; set; }
    private RequestResult TokenResult { get; set; }
    private RequestResult CustomClaimResult { get; set; }
    private ClaimsPrincipal User { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity.IsAuthenticated) return;
        User = authState.User;
    }

    private void OnLoginClick()
    {
        Navigation.NavigateTo($"api/account/MsalLogin", true);
    }

    private async Task OnGetTokenClick()
    {
        TokenResult = null;
        try
        {
            var token = await LocalStorage.GetItemAsStringAsync("authToken");
            RequestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = await RequestClient.GetStringAsync("api/index/gettoken");
            TokenResult = JsonConvert.DeserializeObject<RequestResult>(json);
        }
        catch (Exception ex)
        {
            TokenResult = new();
            TokenResult.Message = ex.Message + " --- " + ex.InnerException;
        }
    }

    private async Task OnGetCustomClaimClick()
    {
        CustomClaimResult = null;
        try
        {
            var token = await LocalStorage.GetItemAsStringAsync("authToken");
            RequestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = await RequestClient.GetStringAsync("api/index/getcustomclaim");
            Console.WriteLine(json);
            CustomClaimResult = JsonConvert.DeserializeObject<RequestResult>(json);
        }
        catch (Exception ex)
        {
            CustomClaimResult = new();
            CustomClaimResult.Message = ex.Message + " --- " + ex.InnerException;
        }
    }
}