using System.Net.Http.Headers;

namespace Target.Ui.Services;

public class ApiAuthorizationMessageHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;

    public ApiAuthorizationMessageHandler(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _tokenStore.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
