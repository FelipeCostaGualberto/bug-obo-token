using Blazored.LocalStorage;
using ExampleObo.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
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
    private RequestResult Result { get; set; }
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
        Result = null;
        var token = await LocalStorage.GetItemAsStringAsync("authToken");
        RequestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var json = await RequestClient.GetStringAsync("api/index/gettoken");
        Result = JsonConvert.DeserializeObject<RequestResult>(json);
    }
}