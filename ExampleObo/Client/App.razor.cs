using Blazored.LocalStorage;
using ExampleObo.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExampleObo.Client;

public partial class App : ComponentBase
{
    [Inject] private ILocalStorageService LocalStorage { get; set; }
    [Inject] private NavigationManager Navigation { get; set; }
    [Inject] private HttpClient RequestClient { get; set; }
    public bool Initialized { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        QueryHelpers.ParseQuery(uri.Query).TryGetValue("userSerial", out var userSerial);
        if (string.IsNullOrWhiteSpace(userSerial))
        {
            Initialized = true;
            return;
        }
        var token = await RequestClient.GetStringAsync($"api/account/GetToken/{userSerial}");
        if (string.IsNullOrWhiteSpace(token))
        {
            Initialized = true;
            return;
        }

        await LocalStorage.SetItemAsStringAsync("authToken", token);
        Navigation.NavigateTo("/");

        Initialized = true;
    }
}