using System.Net.Http.Headers;
using Azure.Identity;
using Azure.Core;

namespace ApiDraft.Providers;
public interface IConfidentialClientAccessTokenProvider
{
    Task<string> GetAccessTokenAsync(string scope, CancellationToken cancellationToken = default);
}

public class TokenAuthorizationHandler : DelegatingHandler
{
    private readonly IConfidentialClientAccessTokenProvider _provider;
    private readonly string _scope;
    public TokenAuthorizationHandler(IConfidentialClientAccessTokenProvider provider, string scope)
    {
        _provider = provider;
        _scope = scope;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _provider.GetAccessTokenAsync(_scope, cancellationToken);

        Console.WriteLine($"Acquired Token: {token}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // return new HttpResponseMessage(System.Net.HttpStatusCode.OK) 
        // { 
        //     Content = new StringContent("Token acquisition test successful!") 
        // };

        return await base.SendAsync(request, cancellationToken);
    }
}

public class AzureProvider : IConfidentialClientAccessTokenProvider
{
    private readonly TokenCredential _credential;

    public AzureProvider(TokenCredential credential)
    {
        _credential = credential;
    }

    public async Task<string> GetAccessTokenAsync(string scope, CancellationToken cancellationToken = default)
    {
        var context = new TokenRequestContext(new[] { scope });
        var token = await _credential.GetTokenAsync(context, cancellationToken);

        return token.Token;
    }
}
