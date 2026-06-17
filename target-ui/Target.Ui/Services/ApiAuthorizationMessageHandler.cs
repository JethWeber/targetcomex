using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Target.Ui.Services;

public class ApiAuthorizationMessageHandler : DelegatingHandler
{
    private readonly AuthService _authService;
    private readonly NavigationManager _navigationManager;

    public ApiAuthorizationMessageHandler(AuthService authService, NavigationManager navigationManager)
    {
        _authService = authService;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authService.GetBearerTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // JS interop indisponível durante prerendering — continuar sem token
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            try
            {
                await _authService.RemoveTokenAsync();
                _navigationManager.NavigateTo("/login", forceLoad: true);
            }
            catch
            {
                // JS interop indisponível durante prerendering — ignorar
            }
        }

        return response;
    }
}