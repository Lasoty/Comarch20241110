using System.Net.Http.Headers;
using System.Security.Claims;
using BibliotekarzBlazor.Shared;
using BibliotekarzBlazor.Shared.Auth;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace BibliotekarzBlazor.Services;

/// <summary>
/// Nasza implementacja AuthenticationStateProvidera. Pozwala dodatkowo zapisać użytkownika w local storage przeglądarki. (Nominalnie zapisywani są w ciasteczkach)
/// </summary>
/// <param name="httpClient"></param>
/// <param name="localStorage"></param>
public class AuthStateProvider (HttpClient httpClient, ILocalStorageService localStorage) 
    :AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorage.GetItemAsStringAsync(Constants.Local_Token);
        if (token == null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("bearer", token);
        return new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    JwtParser.ParseClaimsFromJwt(token), "jwtAuthType")));
    }

    public void NotifyUserLoggedIn(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(
            new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity())));
        NotifyAuthenticationStateChanged(authState);
    }
}
